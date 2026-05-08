using UnityEngine;

namespace Chess.Core.Board
{
    public class BoardManager : MonoBehaviour
    {
        private Square[,] _board;

        private void Awake() => InitialiseBoard();

        private void InitialiseBoard()
        {
            _board = new Square[BoardConstants.Size, BoardConstants.Size];
            for (int file = 0; file < BoardConstants.Size; file++)
                for (int rank = 0; rank < BoardConstants.Size; rank++)
                    _board[file, rank] = new Square(file, rank);
        }

        public Square GetSquare(int file, int rank)
        {
            Debug.Assert(IsValidCoordinate(file, rank), $"({file},{rank}) out of bounds.");
            return _board[file, rank];
        }

        public bool IsValidCoordinate(int file, int rank) =>
            file >= 0 && file < BoardConstants.Size &&
            rank >= 0 && rank < BoardConstants.Size;

        // Always false until Square holds a Piece reference.
        public bool IsOccupied(int file, int rank)
        {
            Debug.Assert(IsValidCoordinate(file, rank));
            return false;
        }
    }
}
