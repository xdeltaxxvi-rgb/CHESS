using System.Collections.Generic;
using UnityEngine;
using Chess.Core.Pieces;

namespace Chess.Core.Board
{
    public static class MoveValidator
    {
        public static bool IsKingInCheck(Square[,] board, PieceColor color)
        {
            PieceColor opponent = color == PieceColor.White ? PieceColor.Black : PieceColor.White;
            for (int f = 0; f < BoardConstants.Size; f++)
                for (int r = 0; r < BoardConstants.Size; r++)
                    if (board[f, r].IsOccupied &&
                        board[f, r].Piece.Color == color &&
                        board[f, r].Piece.Type == PieceType.King)
                        return AttackChecker.IsAttackedBy(board, f, r, opponent);
            return false;
        }

        // Returns only moves that do not leave the moving player's King in check.
        public static List<Vector2Int> GetLegalMoves(Piece piece, Square[,] board)
        {
            var legal = new List<Vector2Int>();
            foreach (Vector2Int to in piece.GetValidMoves(board))
            {
                Square[,] sim = Simulate(board, piece.Position, to);
                if (!IsKingInCheck(sim, piece.Color))
                    legal.Add(to);
            }
            return legal;
        }

        // Shallow-clones the board and applies one move. Safe because only Square
        // struct values change — Piece objects are never mutated during simulation.
        private static Square[,] Simulate(Square[,] board, Vector2Int from, Vector2Int to)
        {
            var sim = (Square[,])board.Clone();
            Piece piece = sim[from.x, from.y].Piece;

            Square fs = sim[from.x, from.y]; fs.Piece = null; sim[from.x, from.y] = fs;
            Square ts = sim[to.x, to.y]; ts.Piece = piece; sim[to.x, to.y] = ts;

            return sim;
        }
    }
}
