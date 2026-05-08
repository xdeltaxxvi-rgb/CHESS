using System.Collections.Generic;
using UnityEngine;
using Chess.Core.Board;

namespace Chess.Core.Pieces
{
    public sealed class Bishop : Piece
    {
        public Bishop(PieceColor color, Vector2Int position)
            : base(color, position, PieceType.Bishop) { }

        public override List<Vector2Int> GetValidMoves(Square[,] board)
        {
            var moves = new List<Vector2Int>();
            AddSlidingMoves(moves, board,
                new[] {  1,  1, -1, -1 },
                new[] {  1, -1,  1, -1 });
            return moves;
        }
    }
}
