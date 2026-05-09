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
        /// <param name="depth">Negamax search depth (2=Easy, 4=Medium, 6=Hard).</param>
        /// <param name="useQuiescence">Extend search through captures at leaf nodes (off on Easy).</param>
        /// <param name="moveSequenceKey">Comma-separated move history in "e2e4" notation.
        ///   Used for opening book lookup. Pass empty string if book is disabled.</param>
        Move? GetBestMove(Square[,] board, PieceColor color, int depth,
                          bool useQuiescence, string moveSequenceKey);
    }

    /// <summary>
    /// Chess AI using negamax with:
    ///   - Iterative deepening
    ///   - Alpha-beta pruning
    ///   - Transposition table (Zobrist hashing, 32 MB)
    ///   - Killer moves (2 per ply)
    ///   - Null move pruning (R = 2 or 3, skipped in endgame / check)
    ///   - Late Move Reductions (quiet, non-killer moves beyond move 3)
    ///   - Quiescence search (captures until quiet — prevents horizon effect)
    ///   - Move ordering: TT move → MVV captures → killers → quiet moves
    ///
    /// Called from a background thread by GameManager — all board work is done
    /// on a cloned copy so the main thread's board is never touched.
    /// </summary>
    public class ChessAI : IChessAI
    {
        private const int Infinity  = 1_000_000;
        private const int MateScore =   900_000;
        private const int MaxPly    = 64; // Killer table depth limit

        private readonly TranspositionTable _tt      = new TranspositionTable(32);
        private readonly Move?[,]           _killers = new Move?[MaxPly, 2];

        // Set at the start of each GetBestMove call; read by Negamax leaf nodes.
        private bool _useQuiescence;

        // ----- Public entry point ------------------------------------------------

        public Move? GetBestMove(Square[,] board, PieceColor color, int maxDepth,
                                  bool useQuiescence, string moveSequenceKey)
        {
            // ----- Opening book (Medium / Hard only) ----------------------------
            if (!string.IsNullOrEmpty(moveSequenceKey) &&
                OpeningBook.TryGetMove(moveSequenceKey, out Move bookMove))
            {
                // Verify the book move is actually legal before playing it.
                Square sq = board[bookMove.From.x, bookMove.From.y];
                if (sq.IsOccupied && sq.Piece.Color == color)
                {
                    var legal = MoveValidator.GetLegalMoves(sq.Piece, board);
                    if (legal.Contains(bookMove.To))
                        return bookMove;
                }
            }

            // ----- Engine search ------------------------------------------------
            Square[,] clone = CloneBoard(board);
            _useQuiescence = useQuiescence;

            // Reset search state between calls.
            _tt.Clear();
            System.Array.Clear(_killers, 0, _killers.Length);

            Move? bestMove = null;

            // Iterative deepening: always have a valid result even if deeper search
            // is slow. Each iteration benefits from TT / killer moves of the previous.
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

            ulong hash   = ZobristTable.ComputeHash(board, color);
            Move? ttMove = _tt.GetBestMove(hash);

            List<Move> moves = GenerateOrderedMoves(board, color, 0, ttMove);
            if (moves.Count == 0) return null;

            foreach (Move move in moves)
            {
                Square[,] next  = ApplyMove(board, move);
                int score = -Negamax(next, Opponent(color), depth - 1, -beta, -alpha, ply: 1);

                if (score > alpha)
                {
                    alpha    = score;
                    bestMove = move;
                }
            }

            _tt.Store(hash, depth, alpha, TTFlag.Exact, bestMove);
            return bestMove;
        }

        // ----- Negamax -----------------------------------------------------------

        private int Negamax(Square[,] board, PieceColor color, int depth,
                            int alpha, int beta, int ply)
        {
            // ----- Transposition table lookup ------------------------------------
            ulong hash = ZobristTable.ComputeHash(board, color);
            if (_tt.TryGet(hash, depth, alpha, beta, out int ttScore, out Move? ttMove))
                return ttScore;

            // ----- Leaf node -----------------------------------------------------
            if (depth == 0)
                return _useQuiescence
                    ? QuiescenceSearch(board, color, alpha, beta)
                    : Evaluator.Evaluate(board, color);

            bool inCheck = MoveValidator.IsKingInCheck(board, color);

            // ----- Null move pruning ---------------------------------------------
            // Skip if: in check, shallow depth, or endgame (zugzwang risk).
            if (!inCheck && depth >= 3 && !IsEndgame(board))
            {
                int R = depth >= 6 ? 3 : 2;

                Square[,] nullBoard = CloneBoard(board);
                ClearEnPassant(nullBoard); // Pass turn, keep board same

                int nullScore = -Negamax(nullBoard, Opponent(color),
                                         depth - 1 - R, -beta, -beta + 1, ply + 1);

                if (nullScore >= beta)
                {
                    _tt.Store(hash, depth, beta, TTFlag.LowerBound, null);
                    return beta;
                }
            }

            // ----- Move loop -----------------------------------------------------
            List<Move> moves = GenerateOrderedMoves(board, color, ply, ttMove);

            if (moves.Count == 0)
            {
                // No legal moves: checkmate or stalemate.
                if (inCheck) return -(MateScore + depth); // Prefer faster mates
                return 0;                                  // Stalemate
            }

            int   originalAlpha = alpha;
            Move? bestMove      = null;
            int   moveCount     = 0;

            foreach (Move move in moves)
            {
                Square[,] next = ApplyMove(board, move);

                bool isCapture = board[move.To.x, move.To.y].IsOccupied ||
                                 board[move.To.x, move.To.y].IsEnPassantTarget;
                bool isKiller  = IsKillerMove(move, ply);

                int score;
                moveCount++;

                // ----- Late Move Reductions (LMR) --------------------------------
                // Reduce depth for quiet, non-killer moves after the first 3.
                bool doLMR = !inCheck && !isCapture && !isKiller && moveCount > 3 && depth >= 3;

                if (doLMR)
                {
                    int reduction = moveCount > 8 ? 2 : 1;
                    score = -Negamax(next, Opponent(color),
                                     depth - 1 - reduction, -beta, -alpha, ply + 1);
                    // Re-search at full depth if the reduced search improves alpha.
                    if (score > alpha)
                        score = -Negamax(next, Opponent(color), depth - 1, -beta, -alpha, ply + 1);
                }
                else
                {
                    score = -Negamax(next, Opponent(color), depth - 1, -beta, -alpha, ply + 1);
                }

                // ----- Beta cutoff -----------------------------------------------
                if (score >= beta)
                {
                    if (!isCapture)
                        StoreKiller(move, ply); // Only quiet moves are useful killers
                    _tt.Store(hash, depth, beta, TTFlag.LowerBound, move);
                    return beta;
                }

                if (score > alpha)
                {
                    alpha    = score;
                    bestMove = move;
                }
            }

            TTFlag flag = alpha > originalAlpha ? TTFlag.Exact : TTFlag.UpperBound;
            _tt.Store(hash, depth, alpha, flag, bestMove);
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

        // ----- Killer moves ------------------------------------------------------

        private bool IsKillerMove(Move move, int ply)
        {
            if (ply >= MaxPly) return false;
            return (_killers[ply, 0].HasValue &&
                    _killers[ply, 0].Value.From == move.From &&
                    _killers[ply, 0].Value.To   == move.To)
                || (_killers[ply, 1].HasValue &&
                    _killers[ply, 1].Value.From == move.From &&
                    _killers[ply, 1].Value.To   == move.To);
        }

        private void StoreKiller(Move move, int ply)
        {
            if (ply >= MaxPly) return;
            // Don't store duplicate of slot 0.
            if (_killers[ply, 0].HasValue &&
                _killers[ply, 0].Value.From == move.From &&
                _killers[ply, 0].Value.To   == move.To)
                return;

            _killers[ply, 1] = _killers[ply, 0]; // Shift older killer down
            _killers[ply, 0] = move;
        }

        // ----- Move generation ---------------------------------------------------

        private List<Move> GenerateOrderedMoves(Square[,] board, PieceColor color,
                                                int ply, Move? ttMove)
        {
            var ttMoves  = new List<Move>();
            var captures = new List<Move>();
            var killers  = new List<Move>();
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
                        var move      = new Move(from, to);
                        bool isCapture = board[to.x, to.y].IsOccupied ||
                                         board[to.x, to.y].IsEnPassantTarget;

                        if (ttMove.HasValue &&
                            ttMove.Value.From == from &&
                            ttMove.Value.To   == to)
                        {
                            ttMoves.Add(move); // TT move gets top priority
                        }
                        else if (isCapture)
                        {
                            captures.Add(move);
                        }
                        else if (ply < MaxPly && IsKillerMove(move, ply))
                        {
                            killers.Add(move);
                        }
                        else
                        {
                            quiets.Add(move);
                        }
                    }
                }

            // MVV-LVA: sort captures by victim value descending (captures most valuable first).
            captures.Sort((a, b) =>
                VictimValue(board, b.To) - VictimValue(board, a.To));

            var result = new List<Move>(
                ttMoves.Count + captures.Count + killers.Count + quiets.Count);
            result.AddRange(ttMoves);
            result.AddRange(captures);
            result.AddRange(killers);
            result.AddRange(quiets);
            return result;
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

        private static bool IsEndgame(Square[,] board)
        {
            int whiteMajors = 0, blackMajors = 0;
            for (int f = 0; f < BoardConstants.Size; f++)
                for (int r = 0; r < BoardConstants.Size; r++)
                {
                    Square sq = board[f, r];
                    if (!sq.IsOccupied ||
                        sq.Piece.Type == PieceType.Pawn ||
                        sq.Piece.Type == PieceType.King) continue;

                    if (sq.Piece.Color == PieceColor.White)
                        whiteMajors += PieceValues.Of(sq.Piece.Type);
                    else
                        blackMajors += PieceValues.Of(sq.Piece.Type);
                }
            return whiteMajors < 1300 || blackMajors < 1300;
        }

        private static void ClearEnPassant(Square[,] board)
        {
            for (int f = 0; f < BoardConstants.Size; f++)
                for (int r = 0; r < BoardConstants.Size; r++)
                    if (board[f, r].IsEnPassantTarget)
                    {
                        var sq = board[f, r];
                        sq.IsEnPassantTarget = false;
                        board[f, r] = sq;
                    }
        }

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
                bool kingside  = fileDelta > 0;
                int  rookFromF = kingside ? BoardConstants.Size - 1 : 0;
                int  rookToF   = kingside ? tx - 1 : tx + 1;
                Piece rook     = next[rookFromF, fy].Piece;
                var   rFrom    = next[rookFromF, fy]; rFrom.Piece = null; next[rookFromF, fy] = rFrom;
                var   rTo      = next[rookToF,   fy]; rTo.Piece   = rook; next[rookToF,   fy] = rTo;
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
