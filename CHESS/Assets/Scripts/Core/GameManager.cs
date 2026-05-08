using System;
using UnityEngine;
using Chess.Core.Board;
using Chess.Core.Pieces;
using Chess.Input;

namespace Chess.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] private MoveSelector _moveSelector;
        [SerializeField] private BoardManager _boardManager;
        [SerializeField] private BoardVisualizer _boardVisualizer;

        public PieceColor CurrentTurn { get; private set; } = PieceColor.White;
        public bool IsInCheck { get; private set; }

        // HUDManager subscribes to update the turn indicator and check display.
        public event Action<PieceColor> OnTurnChanged;
        public event Action<bool> OnCheckChanged;

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

            OnTurnChanged?.Invoke(CurrentTurn);
            OnCheckChanged?.Invoke(IsInCheck);
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
