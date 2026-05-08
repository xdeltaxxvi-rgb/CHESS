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

        // True if the given color has at least one legal move available.
        public static bool HasAnyLegalMove(Square[,] board, PieceColor color)
        {
            for (int f = 0; f < BoardConstants.Size; f++)
                for (int r = 0; r < BoardConstants.Size; r++)
                    if (board[f, r].IsOccupied && board[f, r].Piece.Color == color)
                        if (GetLegalMoves(board[f, r].Piece, board).Count > 0)
                            return true;
            return false;
        }

        // Shallow-clones the board and applies one move. Safe because only Square
        // struct values change — Piece objects are never mutated during simulation.
        private static Square[,] Simulate(Square[,] board, Vector2Int from, Vector2Int to)
        {
            var sim = (Square[,])board.Clone();
            Piece piece = sim[from.x, from.y].Piece;

            // En passant: remove the bypassed pawn so check detection sees the correct board.
            if (piece.Type == PieceType.Pawn && sim[to.x, to.y].IsEnPassantTarget)
            {
                Square capSq = sim[to.x, from.y]; capSq.Piece = null; sim[to.x, from.y] = capSq;
            }

            Square fs = sim[from.x, from.y]; fs.Piece = null; sim[from.x, from.y] = fs;
            Square ts = sim[to.x, to.y]; ts.Piece = piece; sim[to.x, to.y] = ts;

            // Castling: also relocate the Rook so check detection sees the correct board.
            int fileDelta = to.x - from.x;
            if (piece.Type == PieceType.King && (fileDelta == 2 || fileDelta == -2))
            {
                bool kingside = fileDelta > 0;
                int rookFromFile = kingside ? BoardConstants.Size - 1 : 0;
                int rookToFile = kingside ? to.x - 1 : to.x + 1;
                int rank = from.y;

                Piece rook = sim[rookFromFile, rank].Piece;
                Square rs = sim[rookFromFile, rank]; rs.Piece = null; sim[rookFromFile, rank] = rs;
                Square rd = sim[rookToFile, rank]; rd.Piece = rook; sim[rookToFile, rank] = rd;
            }

            return sim;
        }
    }
}
