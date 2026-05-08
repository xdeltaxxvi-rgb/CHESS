using System.Collections.Generic;
using UnityEngine;
using Chess.Core.Board;

namespace Chess.Core.Pieces
{
    public abstract class Piece
    {
        public PieceType Type { get; protected set; }
        public PieceColor Color { get; protected set; }
        public Vector2Int Position { get; set; }
        public bool HasMoved { get; set; }

        protected Piece(PieceColor color, Vector2Int position, PieceType type)
        {
            Color = color;
            Position = position;
            Type = type;
            HasMoved = false;
        }

        public abstract List<Vector2Int> GetValidMoves(Square[,] board);

        protected PieceColor Opponent() =>
            Color == PieceColor.White ? PieceColor.Black : PieceColor.White;

        protected static bool IsInBounds(int file, int rank) =>
            file >= 0 && file < BoardConstants.Size &&
            rank >= 0 && rank < BoardConstants.Size;

        // Shared by Rook, Bishop, Queen. Pass parallel arrays of file/rank deltas for each ray direction.
        protected void AddSlidingMoves(List<Vector2Int> moves, Square[,] board, int[] fileDirs, int[] rankDirs)
        {
            for (int i = 0; i < fileDirs.Length; i++)
            {
                for (int step = 1; step < BoardConstants.Size; step++)
                {
                    int f = Position.x + fileDirs[i] * step;
                    int r = Position.y + rankDirs[i] * step;
                    if (!IsInBounds(f, r)) break;
                    if (board[f, r].IsOccupiedByColor(Color)) break;
                    moves.Add(new Vector2Int(f, r));
                    if (board[f, r].IsOccupied) break;
                }
            }
        }
    }
}
