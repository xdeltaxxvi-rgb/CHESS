using System;
using System.Threading.Tasks;
using UnityEngine;
using Chess.Core.Board;
using Chess.Core.Pieces;
using Chess.Input;
using Chess.AI;

namespace Chess.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] private MoveSelector    _moveSelector;
        [SerializeField] private BoardManager    _boardManager;
        [SerializeField] private BoardVisualizer _boardVisualizer;

        private readonly IChessAI _ai = new ChessAI();

        // ----- Difficulty / search config (loaded from DifficultySettings on Start) ----
        private int    _aiDepth        = 4;
        private bool   _useQuiescence  = true;
        private bool   _useOpeningBook = true;

        // ----- Move history (for opening book) -----------------------------------
        // Comma-separated coordinate moves ("e2e4,e7e5,...") built after every move.
        private string _moveHistoryKey = string.Empty;

        // ----- Turn state --------------------------------------------------------
        public PieceColor CurrentTurn { get; private set; } = PieceColor.White;
        public bool IsInCheck  { get; private set; }
        public bool IsGameOver { get; private set; }

        // HUDManager subscribes to update the turn indicator and check display.
        public event Action<PieceColor> OnTurnChanged;
        public event Action<bool>       OnCheckChanged;
        // winner == null → stalemate (draw); winner != null → that color won.
        public event Action<PieceColor?> OnGameOver;

        private PieceView _checkedKingView;

        // -------------------------------------------------------------------------

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            // Read difficulty from PlayerPrefs (set in Settings UI, Phase 5).
            _aiDepth        = DifficultySettings.Depth;
            _useQuiescence  = DifficultySettings.UseQuiescence;
            _useOpeningBook = DifficultySettings.UseOpeningBook;

            _moveSelector.CurrentPlayer = CurrentTurn;
        }

        private void OnEnable()
        {
            _moveSelector.OnMoveExecuted         += SwitchTurn;
            _moveSelector.OnMoveExecutedDetailed += AppendMoveHistory;
        }

        private void OnDisable()
        {
            _moveSelector.OnMoveExecuted         -= SwitchTurn;
            _moveSelector.OnMoveExecutedDetailed -= AppendMoveHistory;
        }

        // ----- Move history ------------------------------------------------------

        private void AppendMoveHistory(Vector2Int from, Vector2Int to)
        {
            string key = OpeningBook.MoveToKey(new Move(from, to));
            _moveHistoryKey = string.IsNullOrEmpty(_moveHistoryKey)
                ? key
                : _moveHistoryKey + "," + key;
        }

        // ----- Turn loop ---------------------------------------------------------

        private void SwitchTurn()
        {
            ClearCheckVisual();

            CurrentTurn = CurrentTurn == PieceColor.White ? PieceColor.Black : PieceColor.White;
            _moveSelector.CurrentPlayer = CurrentTurn;

            Square[,] board = _boardManager.GetBoard();
            IsInCheck = MoveValidator.IsKingInCheck(board, CurrentTurn);

            if (IsInCheck)
                ShowCheckVisual(board, CurrentTurn);

            if (!MoveValidator.HasAnyLegalMove(board, CurrentTurn))
            {
                IsGameOver = true;
                _moveSelector.enabled = false;
                // Checkmate: in check + no moves → opponent wins. Stalemate → draw.
                PieceColor? winner = IsInCheck
                    ? (CurrentTurn == PieceColor.White ? PieceColor.Black : PieceColor.White)
                    : (PieceColor?)null;
                OnGameOver?.Invoke(winner);
                return;
            }

            OnTurnChanged?.Invoke(CurrentTurn);
            OnCheckChanged?.Invoke(IsInCheck);

            if (CurrentTurn == PieceColor.Black)
                StartAITurn();
        }

        private async void StartAITurn()
        {
            _moveSelector.enabled = false;

            Square[,] board        = _boardManager.GetBoard();
            int       depth        = _aiDepth;
            bool      quiescence   = _useQuiescence;
            bool      useBook      = _useOpeningBook;
            PieceColor color       = CurrentTurn;
            // Capture the key at this moment (before the AI's move is appended).
            string historyKey = useBook ? _moveHistoryKey : string.Empty;

            Move? move = await Task.Run(() =>
                _ai.GetBestMove(board, color, depth, quiescence, historyKey));

            // Game may have ended while the AI was computing.
            if (IsGameOver) return;

            if (move.HasValue)
                _moveSelector.SubmitMove(move.Value.From, move.Value.To);

            // Re-enable player input only if the AI move didn't end the game.
            if (!IsGameOver)
                _moveSelector.enabled = true;
        }

        // ----- Check visuals -----------------------------------------------------

        private void ShowCheckVisual(Square[,] board, PieceColor color)
        {
            for (int f = 0; f < BoardConstants.Size; f++)
                for (int r = 0; r < BoardConstants.Size; r++)
                    if (board[f, r].IsOccupied &&
                        board[f, r].Piece.Color == color &&
                        board[f, r].Piece.Type  == PieceType.King)
                    {
                        _checkedKingView = _boardVisualizer.GetPieceView(new Vector2Int(f, r));
                        _checkedKingView?.SetTint(Color.red);
                        return;
                    }
        }

        private void ClearCheckVisual()
        {
            _checkedKingView?.ResetTint();
            _checkedKingView = null;
        }
    }
}
