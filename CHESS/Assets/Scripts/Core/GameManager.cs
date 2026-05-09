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

        [SerializeField] private MoveSelector _moveSelector;
        [SerializeField] private BoardManager _boardManager;
        [SerializeField] private BoardVisualizer _boardVisualizer;
        // Ch1→depth 2, Ch2-3→3, Ch4-5→4, Ch6-7→5, Ch8→6 (set by NarrativeController later).
        [SerializeField] private int _aiDepth = 2;

        private readonly IChessAI _ai = new ChessAI();

        public PieceColor CurrentTurn { get; private set; } = PieceColor.White;
        public bool IsInCheck { get; private set; }
        public bool IsGameOver { get; private set; }

        // HUDManager subscribes to update the turn indicator and check display.
        public event Action<PieceColor> OnTurnChanged;
        public event Action<bool> OnCheckChanged;
        // winner == null → stalemate (draw); winner != null → that color won.
        public event Action<PieceColor?> OnGameOver;

        private PieceView _checkedKingView;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start() => _moveSelector.CurrentPlayer = CurrentTurn;

        private void OnEnable() => _moveSelector.OnMoveExecuted += SwitchTurn;
        private void OnDisable() => _moveSelector.OnMoveExecuted -= SwitchTurn;

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
                // Checkmate: in check + no moves → opponent wins. Stalemate: not in check → draw.
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

            Square[,] board = _boardManager.GetBoard();
            int depth = _aiDepth;
            PieceColor color = CurrentTurn;

            Move? move = await Task.Run(() => _ai.GetBestMove(board, color, depth));

            // Game may have ended while the AI was computing (shouldn't happen with
            // the stub, but guards against future long searches).
            if (IsGameOver) return;

            if (move.HasValue)
                _moveSelector.SubmitMove(move.Value.From, move.Value.To);

            // Re-enable player input only if the AI move didn't end the game.
            if (!IsGameOver)
                _moveSelector.enabled = true;
        }

        private void ShowCheckVisual(Square[,] board, PieceColor color)
        {
            for (int f = 0; f < BoardConstants.Size; f++)
                for (int r = 0; r < BoardConstants.Size; r++)
                    if (board[f, r].IsOccupied &&
                        board[f, r].Piece.Color == color &&
                        board[f, r].Piece.Type == PieceType.King)
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
