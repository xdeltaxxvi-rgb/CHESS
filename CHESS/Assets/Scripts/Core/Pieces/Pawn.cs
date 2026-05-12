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
            // Starting rank: belt-and-suspenders alongside !HasMoved. Pawns can't move
            // backward, so today the two checks are equivalent. Keeping the explicit
            // rank check makes the rule independent of any future state-reset path
            // (undo, history rewind, etc.). See Chess Rules Audit → M3.
            int startRank = Color == PieceColor.White ? 1 : BoardConstants.Size - 2;

            // Forward 1
            int fwd1 = rank + dir;
            if (IsInBounds(file, fwd1) && !board[file, fwd1].IsOccupied)
            {
                moves.Add(new Vector2Int(file, fwd1));

                // Forward 2 from starting square
                int fwd2 = rank + dir * 2;
                if (rank == startRank && !HasMoved && IsInBounds(file, fwd2) && !board[file, fwd2].IsOccupied)
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
