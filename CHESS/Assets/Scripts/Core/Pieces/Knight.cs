using System.Collections.Generic;
using UnityEngine;
using Chess.Core.Board;

namespace Chess.Core.Pieces
{
    public sealed class Knight : Piece
    {
        private static readonly int[] FileDelta = {  2,  2, -2, -2,  1,  1, -1, -1 };
        private static readonly int[] RankDelta = {  1, -1,  1, -1,  2, -2,  2, -2 };

        public Knight(PieceColor color, Vector2Int position)
            : base(color, position, PieceType.Knight) { }

        public override List<Vector2Int> GetValidMoves(Square[,] board)
        {
            var moves = new List<Vector2Int>();
            for (int i = 0; i < FileDelta.Length; i++)
            {
                int f = Position.x + FileDelta[i];
                int r = Position.y + RankDelta[i];
                if (IsInBounds(f, r) && !board[f, r].IsOccupiedByColor(Color))
                    moves.Add(new Vector2Int(f, r));
            }
            return moves;
        }
    }
}
