using System.Collections.Generic;
using UnityEngine;
using Chess.Core.Board;

namespace Chess.Core.Pieces
{
    public sealed class Rook : Piece
    {
        public Rook(PieceColor color, Vector2Int position)
            : base(color, position, PieceType.Rook) { }

        public override List<Vector2Int> GetValidMoves(Square[,] board)
        {
            var moves = new List<Vector2Int>();
            AddSlidingMoves(moves, board,
                new[] { 1, -1, 0,  0 },
                new[] { 0,  0, 1, -1 });
            return moves;
        }
    }
}
