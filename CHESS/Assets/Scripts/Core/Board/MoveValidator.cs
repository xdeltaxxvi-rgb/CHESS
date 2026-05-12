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

        // FIDE Article 5.2.2 — insufficient material (auto draw).
        // Returns true for any of:
        //   • K vs K
        //   • K + Bishop vs K
        //   • K + Knight vs K
        //   • K + Bishop vs K + Bishop, both bishops on the same square colour
        // Two-knights-vs-king is NOT auto-draw under strict FIDE (mate is possible,
        // if extremely rare), so we don't return true for that case.
        public static bool IsInsufficientMaterial(Square[,] board)
        {
            int whiteKnights = 0, whiteBishops = 0, otherWhite = 0;
            int blackKnights = 0, blackBishops = 0, otherBlack = 0;
            bool whiteBishopLight = false, whiteBishopDark = false;
            bool blackBishopLight = false, blackBishopDark = false;

            for (int f = 0; f < BoardConstants.Size; f++)
                for (int r = 0; r < BoardConstants.Size; r++)
                {
                    Square sq = board[f, r];
                    if (!sq.IsOccupied) continue;
                    Piece p = sq.Piece;
                    if (p.Type == PieceType.King) continue;

                    bool light = (f + r) % 2 != 0;
                    bool isWhite = p.Color == PieceColor.White;

                    switch (p.Type)
                    {
                        case PieceType.Knight:
                            if (isWhite) whiteKnights++; else blackKnights++;
                            break;
                        case PieceType.Bishop:
                            if (isWhite) { whiteBishops++; if (light) whiteBishopLight = true; else whiteBishopDark = true; }
                            else         { blackBishops++; if (light) blackBishopLight = true; else blackBishopDark = true; }
                            break;
                        default: // Pawn, Rook, Queen — anything else means there's mating material
                            if (isWhite) otherWhite++; else otherBlack++;
                            break;
                    }
                }

            // Any pawn / rook / queen means there's mating material somewhere.
            if (otherWhite > 0 || otherBlack > 0) return false;

            int totalMinors = whiteKnights + whiteBishops + blackKnights + blackBishops;

            // K vs K.
            if (totalMinors == 0) return true;
            // K+B-or-N vs K (single minor piece, either side).
            if (totalMinors == 1) return true;
            // K+B vs K+B with both bishops on the same square colour.
            if (whiteKnights == 0 && blackKnights == 0 &&
                whiteBishops == 1 && blackBishops == 1)
            {
                if (whiteBishopLight == blackBishopLight && whiteBishopDark == blackBishopDark)
                    return true;
            }
            return false;
        }

        // Shallow-clones the board and applies one move. Safe because only Square
        // struct values change — Piece objects are NEVER mutated during simulation.
        //
        // ⚠️ DO NOT add `piece.Position = to` or `piece.HasMoved = true` lines below.
        // The cloned board shares Piece references with the live board (Square is a
        // struct, Piece is a class). Mutating a Piece here would corrupt the live
        // BoardManager board and every concurrent simulation. If full piece-state
        // simulation is ever needed, follow ChessAI.CloneBoard's deep-clone pattern
        // instead. See Chess Rules Audit → M4.
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
