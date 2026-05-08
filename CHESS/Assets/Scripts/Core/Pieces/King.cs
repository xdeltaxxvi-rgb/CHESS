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
            AddCastlingMoves(moves, board);
            return moves;
        }

        private void AddCastlingMoves(List<Vector2Int> moves, Square[,] board)
        {
            if (HasMoved) return;
            PieceColor opp = Opponent();
            int rank = Position.y;

            // King must not currently be in check.
            if (AttackChecker.IsAttackedBy(board, Position.x, rank, opp)) return;

            // Kingside: Rook at file 7, squares 5 and 6 empty and not attacked.
            if (board[7, rank].IsOccupied &&
                board[7, rank].Piece.Type == PieceType.Rook &&
                board[7, rank].Piece.Color == Color &&
                !board[7, rank].Piece.HasMoved &&
                !board[5, rank].IsOccupied &&
                !board[6, rank].IsOccupied &&
                !AttackChecker.IsAttackedBy(board, 5, rank, opp) &&
                !AttackChecker.IsAttackedBy(board, 6, rank, opp))
            {
                moves.Add(new Vector2Int(6, rank));
            }

            // Queenside: Rook at file 0, squares 1–3 empty, squares 2–3 not attacked.
            if (board[0, rank].IsOccupied &&
                board[0, rank].Piece.Type == PieceType.Rook &&
                board[0, rank].Piece.Color == Color &&
                !board[0, rank].Piece.HasMoved &&
                !board[1, rank].IsOccupied &&
                !board[2, rank].IsOccupied &&
                !board[3, rank].IsOccupied &&
                !AttackChecker.IsAttackedBy(board, 2, rank, opp) &&
                !AttackChecker.IsAttackedBy(board, 3, rank, opp))
            {
                moves.Add(new Vector2Int(2, rank));
            }
        }
    }
}
