using System.Collections.Generic;
using UnityEngine;
using Chess.Core.Board;

namespace Chess.Core.Pieces
{
    public sealed class King : Piece
    {
        public King(PieceColor color, Vector2Int position)
            : base(color, position, PieceType.King) { }

        public override List<Vector2Int> GetValidMoves(Square[,] board)
        {
            var moves = new List<Vector2Int>();
            for (int fd = -1; fd <= 1; fd++)
            {
                for (int rd = -1; rd <= 1; rd++)
                {
                    if (fd == 0 && rd == 0) continue;
                    int f = Position.x + fd;
                    int r = Position.y + rd;
                    if (!IsInBounds(f, r)) continue;
                    if (board[f, r].IsOccupiedByColor(Color)) continue;
                    if (AttackChecker.IsAttackedBy(board, f, r, Opponent())) continue;
                    moves.Add(new Vector2Int(f, r));
                }
            }
            return moves;
        }
    }
}
