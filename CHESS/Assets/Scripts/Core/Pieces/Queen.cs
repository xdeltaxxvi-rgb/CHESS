using System.Collections.Generic;
using UnityEngine;
using Chess.Core.Board;

namespace Chess.Core.Pieces
{
    public sealed class Queen : Piece
    {
        public Queen(PieceColor color, Vector2Int position)
            : base(color, position, PieceType.Queen) { }

        public override List<Vector2Int> GetValidMoves(Square[,] board)
        {
            var moves = new List<Vector2Int>();
            AddSlidingMoves(moves, board,
                new[] {  1, -1,  0,  0,  1,  1, -1, -1 },
                new[] {  0,  0,  1, -1,  1, -1,  1, -1 });
            return moves;
        }
    }
}
