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
    }
}
