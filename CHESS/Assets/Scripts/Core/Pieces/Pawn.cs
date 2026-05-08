using System.Collections.Generic;
using UnityEngine;
using Chess.Core.Board;

namespace Chess.Core.Pieces
{
    public sealed class Pawn : Piece
    {
        public Pawn(PieceColor color, Vector2Int position)
            : base(color, position, PieceType.Pawn) { }

        public override List<Vector2Int> GetValidMoves(Square[,] board)
        {
            var moves = new List<Vector2Int>();
            int file = Position.x;
            int rank = Position.y;
            int dir = Color == PieceColor.White ? 1 : -1;

            // Forward 1
            int fwd1 = rank + dir;
            if (IsInBounds(file, fwd1) && !board[file, fwd1].IsOccupied)
            {
                moves.Add(new Vector2Int(file, fwd1));

                // Forward 2 from starting square
                int fwd2 = rank + dir * 2;
                if (!HasMoved && IsInBounds(file, fwd2) && !board[file, fwd2].IsOccupied)
                    moves.Add(new Vector2Int(file, fwd2));
            }

            // Diagonal captures (normal and en passant)
            foreach (int df in new[] { -1, 1 })
            {
                int cf = file + df;
                int cr = rank + dir;
                if (IsInBounds(cf, cr) &&
                    (board[cf, cr].IsOccupiedByColor(Opponent()) || board[cf, cr].IsEnPassantTarget))
                    moves.Add(new Vector2Int(cf, cr));
            }

            return moves;
        }
    }
}
