using System.Collections.Generic;
using UnityEngine;
using Chess.Core.Board;
using Chess.Core.Pieces;

namespace Chess.AI
{
    /// <summary>
    /// Pluggable AI contract. GameManager always talks to this interface,
    /// never to ChessAI directly — swapping implementations requires zero
    /// changes outside this file.
    /// </summary>
    public interface IChessAI
    {
        // Returns null when the player has no legal moves (checkmate / stalemate).
        Move? GetBestMove(Square[,] board, PieceColor color, int depth);
    }

    /// <summary>
    /// Chess AI using negamax with iterative deepening and quiescence search.
    /// Called from a background thread by GameManager — all board work is done
    /// on a cloned copy so the main thread's board is never touched.
    /// </summary>
    public class ChessAI : IChessAI
    {
        private const int Infinity     =  1_000_000;
        private const int MateScore    =    900_000; // Returned when no legal moves exist.

        // ----- Public entry point ------------------------------------------------

        public Move? GetBestMove(Square[,] board, PieceColor color, int maxDepth)
        {
            // Clone board so the search never touches the live game state.
            Square[,] clone = CloneBoard(board);

            Move? bestMove = null;

            // Iterative deepening: search depth 1 → maxDepth.
            // Always returns the best move found so far, so a timeout mid-search
            // still produces a valid (shallower) result.
            for (int depth = 1; depth <= maxDepth; depth++)
            {
                Move? candidate = SearchRoot(clone, color, depth);
                if (candidate.HasValue)
                    bestMove = candidate;
            }

            return bestMove;
        }

        // ----- Root search -------------------------------------------------------

        private Move? SearchRoot(Square[,] board, PieceColor color, int depth)
        {
            int alpha = -Infinity;
            int beta  =  Infinity;
            Move? bestMove = null;

            List<Move> moves = GenerateOrderedMoves(board, color);
            if (moves.Count == 0) return null;

            foreach (Move move in moves)
            {
                Square[,] next = ApplyMove(board, move);
                PieceColor opponent = Opponent(color);
                int score = -Negamax(next, opponent, depth - 1, -beta, -alpha);

                if (score > alpha)
                {
                    alpha    = score;
                    bestMove = move;
                }
            }

            return bestMove;
        }

        // ----- Negamax -----------------------------------------------------------

        private int Negamax(Square[,] board, PieceColor color, int depth, int alpha, int beta)
        {
            if (depth == 0)
                return QuiescenceSearch(board, color, alpha, beta);

            List<Move> moves = GenerateOrderedMoves(board, color);

            if (moves.Count == 0)
            {
                // No legal moves: checkmate or stalemate.
                if (MoveValidator.IsKingInCheck(board, color))
                    return -(MateScore + depth); // Prefer faster mates.
                return 0; // Stalemate.
            }

            foreach (Move move in moves)
            {
                Square[,] next = ApplyMove(board, move);
                int score = -Negamax(next, Opponent(color), depth - 1, -beta, -alpha);

                if (score >= beta)
                    return beta; // Beta cutoff — opponent won't allow this.

                if (score > alpha)
                    alpha = score;
            }

            return alpha;
        }

        // ----- Quiescence search -------------------------------------------------
        // Extends the search through captures until the position is quiet,
        // preventing the horizon effect where the AI misses a capture on move N+1.

        private int QuiescenceSearch(Square[,] board, PieceColor color, int alpha, int beta)
        {
            int standPat = Evaluator.Evaluate(board, color);

            if (standPat >= beta) return beta;
            if (standPat > alpha) alpha = standPat;

            List<Move> captures = GenerateCapturesOrdered(board, color);

            foreach (Move move in captures)
            {
                Square[,] next = ApplyMove(board, move);
                int score = -QuiescenceSearch(next, Opponent(color), -beta, -alpha);

                if (score >= beta) return beta;
                if (score > alpha) alpha = score;
            }

            return alpha;
        }

        // ----- Move generation ---------------------------------------------------

        private List<Move> GenerateOrderedMoves(Square[,] board, PieceColor color)
        {
            var captures = new List<Move>();
            var quiets   = new List<Move>();

            for (int f = 0; f < BoardConstants.Size; f++)
                for (int r = 0; r < BoardConstants.Size; r++)
                {
                    Square sq = board[f, r];
                    if (!sq.IsOccupied || sq.Piece.Color != color) continue;

                    var legal = MoveValidator.GetLegalMoves(sq.Piece, board);
                    var from  = new Vector2Int(f, r);

                    foreach (Vector2Int to in legal)
                    {
                        var move = new Move(from, to);
                        if (board[to.x, to.y].IsOccupied || board[to.x, to.y].IsEnPassantTarget)
                            captures.Add(move);
                        else
                            quiets.Add(move);
                    }
                }

            // Simple MVV ordering: sort captures by victim value descending.
            captures.Sort((a, b) =>
                VictimValue(board, b.To) - VictimValue(board, a.To));

            captures.AddRange(quiets);
            return captures;
        }

        private List<Move> GenerateCapturesOrdered(Square[,] board, PieceColor color)
        {
            var captures = new List<Move>();

            for (int f = 0; f < BoardConstants.Size; f++)
                for (int r = 0; r < BoardConstants.Size; r++)
                {
                    Square sq = board[f, r];
                    if (!sq.IsOccupied || sq.Piece.Color != color) continue;

                    var legal = MoveValidator.GetLegalMoves(sq.Piece, board);
                    var from  = new Vector2Int(f, r);

                    foreach (Vector2Int to in legal)
                        if (board[to.x, to.y].IsOccupied || board[to.x, to.y].IsEnPassantTarget)
                            captures.Add(new Move(from, to));
                }

            captures.Sort((a, b) =>
                VictimValue(board, b.To) - VictimValue(board, a.To));

            return captures;
        }

        private static int VictimValue(Square[,] board, Vector2Int to)
        {
            Square sq = board[to.x, to.y];
            return sq.IsOccupied ? PieceValues.Of(sq.Piece.Type) : 0;
        }

        // ----- Board helpers -----------------------------------------------------

        private static Square[,] CloneBoard(Square[,] src)
        {
            var dst = new Square[BoardConstants.Size, BoardConstants.Size];
            for (int f = 0; f < BoardConstants.Size; f++)
                for (int r = 0; r < BoardConstants.Size; r++)
                    dst[f, r] = src[f, r]; // Square is a value type — copy is deep.
            return dst;
        }

        /// <summary>
        /// Returns a new board with <paramref name="move"/> applied.
        /// Handles en passant and castling in data (mirrors BoardManager.ExecuteMove).
        /// Promotion auto-queens (same as Phase 1 visual layer).
        /// </summary>
        private static Square[,] ApplyMove(Square[,] board, Move move)
        {
            Square[,] next = CloneBoard(board);
            int fx = move.From.x, fy = move.From.y;
            int tx = move.To.x,   ty = move.To.y;

            Piece piece = next[fx, fy].Piece;

            // Clear previous en passant targets.
            for (int f = 0; f < BoardConstants.Size; f++)
                for (int r = 0; r < BoardConstants.Size; r++)
                    if (next[f, r].IsEnPassantTarget)
                    {
                        var sq = next[f, r]; sq.IsEnPassantTarget = false; next[f, r] = sq;
                    }

            // En passant capture: remove the bypassed pawn.
            bool isEnPassant = piece.Type == PieceType.Pawn && board[tx, ty].IsEnPassantTarget;
            if (isEnPassant)
            {
                var capSq = next[tx, fy]; capSq.Piece = null; next[tx, fy] = capSq;
            }

            // Move piece.
            var fromSq = next[fx, fy]; fromSq.Piece = null;           next[fx, fy] = fromSq;
            var toSq   = next[tx, ty]; toSq.Piece   = piece;          next[tx, ty] = toSq;
            piece.Position = move.To;
            piece.HasMoved = true;

            // Set new en passant target on pawn double-step.
            int rankDelta = ty - fy;
            if (piece.Type == PieceType.Pawn && (rankDelta == 2 || rankDelta == -2))
            {
                int epRank = (fy + ty) / 2;
                var epSq = next[fx, epRank]; epSq.IsEnPassantTarget = true; next[fx, epRank] = epSq;
            }

            // Castling: also move the rook.
            int fileDelta = tx - fx;
            if (piece.Type == PieceType.King && (fileDelta == 2 || fileDelta == -2))
            {
                bool kingside    = fileDelta > 0;
                int  rookFromF   = kingside ? BoardConstants.Size - 1 : 0;
                int  rookToF     = kingside ? tx - 1 : tx + 1;
                Piece rook       = next[rookFromF, fy].Piece;
                var   rFrom      = next[rookFromF, fy]; rFrom.Piece = null;  next[rookFromF, fy] = rFrom;
                var   rTo        = next[rookToF,   fy]; rTo.Piece   = rook;  next[rookToF,   fy] = rTo;
                if (rook != null) { rook.Position = new Vector2Int(rookToF, fy); rook.HasMoved = true; }
            }

            // Promotion: auto-queen.
            if (piece.Type == PieceType.Pawn)
            {
                int backRank = piece.Color == PieceColor.White ? BoardConstants.Size - 1 : 0;
                if (ty == backRank)
                {
                    var promoted = new Queen(piece.Color, move.To) { HasMoved = true };
                    var promSq   = next[tx, ty]; promSq.Piece = promoted; next[tx, ty] = promSq;
                }
            }

            return next;
        }

        private static PieceColor Opponent(PieceColor c) =>
            c == PieceColor.White ? PieceColor.Black : PieceColor.White;
    }
}
