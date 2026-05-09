using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Chess.Core.Board;
using Chess.Core.Pieces;
using Chess.AI;

namespace Chess.Editor
{
    /// <summary>
    /// Automated AI legality validator.
    ///
    /// Runs AI vs AI self-play games entirely in C# (no scene required).
    /// After every AI move the validator checks the chosen move against
    /// MoveValidator.GetLegalMoves.  Any illegal move is a hard failure.
    ///
    /// Access via: Chess ▶ Validate AI ▶ Run Self-Play Test
    ///
    /// Search depth is capped at 2 for all difficulties so the test
    /// finishes in under 10 s for 9 games.  The point is to exercise the
    /// move-generation / legality pipeline at each difficulty's quiescence /
    /// opening-book settings, not to benchmark strength.
    /// </summary>
    public static class AIGameValidator
    {
        private const int GamesPerDifficulty = 3;   // 3 × 3 difficulties = 9 total
        private const int MaxMovesPerGame    = 60;   // ~30 moves each side — enough to hit most positions
        private const int MaxSearchDepth     = 2;    // depth 2 keeps each ply < 50 ms; total run < 10 s

        // ── Entry point ────────────────────────────────────────────────────

        [MenuItem("Chess/Validate AI/Run Self-Play Test (9 games, depth 2)")]
        public static void RunSelfPlayTest()
        {
            Debug.Log("[AIValidator] ── Starting self-play validation ──");
            var sw = System.Diagnostics.Stopwatch.StartNew();

            int totalGames  = 0;
            int illegalCount = 0;

            foreach (Difficulty diff in new[] { Difficulty.Easy, Difficulty.Medium, Difficulty.Hard })
            {
                int depth       = Mathf.Min(DifficultySettingFor(diff).depth, MaxSearchDepth);
                bool quiescence = DifficultySettingFor(diff).quiescence;

                for (int g = 1; g <= GamesPerDifficulty; g++)
                {
                    totalGames++;
                    bool hadIllegal = RunOneGame(diff, depth, quiescence, g);
                    if (hadIllegal) illegalCount++;
                }
            }

            sw.Stop();
            bool pass  = illegalCount == 0;
            string col = pass ? "green" : "red";
            string ico = pass ? "✓ PASS" : "✗ FAIL";

            Debug.Log(
                $"[AIValidator] <color={col}><b>{ico}</b></color> — " +
                $"{totalGames} games | {illegalCount} illegal-move game(s) | " +
                $"{sw.ElapsedMilliseconds} ms total");
        }

        // ── One complete game ───────────────────────────────────────────────

        private static bool RunOneGame(Difficulty diff, int depth, bool quiescence, int gameNum)
        {
            var board  = new HeadlessBoard();
            var ai     = new ChessAI();
            var turn   = PieceColor.White;
            string history = string.Empty;

            for (int ply = 0; ply < MaxMovesPerGame; ply++)
            {
                Square[,] state = board.GetBoard();

                // ── Termination ──────────────────────────────────────────
                if (!MoveValidator.HasAnyLegalMove(state, turn))
                {
                    bool inCheck = MoveValidator.IsKingInCheck(state, turn);
                    string result = inCheck
                        ? $"{Opponent(turn)} wins (checkmate)"
                        : "draw (stalemate)";
                    Debug.Log($"[AIValidator] {diff} #{gameNum}: {result} in {ply} plies");
                    return false; // clean finish — no illegal move
                }

                // ── AI chooses a move ────────────────────────────────────
                Move? chosen = ai.GetBestMove(state, turn, depth, quiescence, history);
                if (!chosen.HasValue)
                {
                    Debug.LogWarning(
                        $"[AIValidator] {diff} #{gameNum} ply {ply}: " +
                        "AI returned null despite legal moves existing — treating as draw.");
                    return false;
                }

                // ── Legality check ───────────────────────────────────────
                Piece moving = state[chosen.Value.From.x, chosen.Value.From.y].Piece;
                if (moving == null)
                {
                    Debug.LogError(
                        $"[AIValidator] {diff} #{gameNum} ply {ply}: " +
                        $"ILLEGAL — AI moved from empty square {chosen.Value.From} as {turn}.");
                    return true;
                }

                var legal = MoveValidator.GetLegalMoves(moving, state);
                if (!legal.Contains(chosen.Value.To))
                {
                    Debug.LogError(
                        $"[AIValidator] {diff} #{gameNum} ply {ply}: " +
                        $"ILLEGAL — {turn} {moving.Type} {chosen.Value.From}→{chosen.Value.To} " +
                        "is not in the legal-move list.");
                    return true;
                }

                // ── Apply move ───────────────────────────────────────────
                board.ExecuteMove(chosen.Value.From, chosen.Value.To);

                // Auto-promote to Queen (mirrors MoveSelector behaviour)
                Square arrived = board.GetBoard()[chosen.Value.To.x, chosen.Value.To.y];
                if (arrived.IsOccupied && arrived.Piece.Type == PieceType.Pawn)
                {
                    int backRank = arrived.Piece.Color == PieceColor.White
                        ? BoardConstants.Size - 1 : 0;
                    if (chosen.Value.To.y == backRank)
                        board.PromotePawn(chosen.Value.To, PieceType.Queen);
                }

                // Append move to history string (for opening book)
                string key = OpeningBook.MoveToKey(chosen.Value);
                history = string.IsNullOrEmpty(history) ? key : history + "," + key;

                turn = Opponent(turn);
            }

            // Reached move limit without a terminal position — not a failure.
            Debug.Log($"[AIValidator] {diff} #{gameNum}: move limit ({MaxMovesPerGame}) reached.");
            return false;
        }

        // ── Helpers ────────────────────────────────────────────────────────

        private static PieceColor Opponent(PieceColor c) =>
            c == PieceColor.White ? PieceColor.Black : PieceColor.White;

        private static (int depth, bool quiescence) DifficultySettingFor(Difficulty d) => d switch
        {
            Difficulty.Easy   => (2, false),
            Difficulty.Medium => (4, true),
            _                 => (6, true),   // Hard — clamped to MaxSearchDepth above
        };

        // ══════════════════════════════════════════════════════════════════
        // HeadlessBoard — a non-MonoBehaviour mirror of BoardManager.
        // Mirrors ExecuteMove / PromotePawn / GetBoard exactly.
        // ══════════════════════════════════════════════════════════════════

        private sealed class HeadlessBoard
        {
            private readonly Square[,] _board =
                new Square[BoardConstants.Size, BoardConstants.Size];
            private Vector2Int? _enPassantTarget;

            public HeadlessBoard()
            {
                for (int f = 0; f < BoardConstants.Size; f++)
                    for (int r = 0; r < BoardConstants.Size; r++)
                        _board[f, r] = new Square(f, r);

                // White back rank (rank 0)
                Place(new Rook  (PieceColor.White, new Vector2Int(0, 0)), 0, 0);
                Place(new Knight(PieceColor.White, new Vector2Int(1, 0)), 1, 0);
                Place(new Bishop(PieceColor.White, new Vector2Int(2, 0)), 2, 0);
                Place(new Queen (PieceColor.White, new Vector2Int(3, 0)), 3, 0);
                Place(new King  (PieceColor.White, new Vector2Int(4, 0)), 4, 0);
                Place(new Bishop(PieceColor.White, new Vector2Int(5, 0)), 5, 0);
                Place(new Knight(PieceColor.White, new Vector2Int(6, 0)), 6, 0);
                Place(new Rook  (PieceColor.White, new Vector2Int(7, 0)), 7, 0);

                // White pawns (rank 1)
                for (int f = 0; f < BoardConstants.Size; f++)
                    Place(new Pawn(PieceColor.White, new Vector2Int(f, 1)), f, 1);

                // Black pawns (rank 6)
                for (int f = 0; f < BoardConstants.Size; f++)
                    Place(new Pawn(PieceColor.Black, new Vector2Int(f, 6)), f, 6);

                // Black back rank (rank 7)
                Place(new Rook  (PieceColor.Black, new Vector2Int(0, 7)), 0, 7);
                Place(new Knight(PieceColor.Black, new Vector2Int(1, 7)), 1, 7);
                Place(new Bishop(PieceColor.Black, new Vector2Int(2, 7)), 2, 7);
                Place(new Queen (PieceColor.Black, new Vector2Int(3, 7)), 3, 7);
                Place(new King  (PieceColor.Black, new Vector2Int(4, 7)), 4, 7);
                Place(new Bishop(PieceColor.Black, new Vector2Int(5, 7)), 5, 7);
                Place(new Knight(PieceColor.Black, new Vector2Int(6, 7)), 6, 7);
                Place(new Rook  (PieceColor.Black, new Vector2Int(7, 7)), 7, 7);
            }

            public Square[,] GetBoard() => _board;

            // Mirrors BoardManager.ExecuteMove exactly (castling, en passant, pawn double-step).
            public void ExecuteMove(Vector2Int from, Vector2Int to)
            {
                Piece piece = _board[from.x, from.y].Piece;

                bool isEnPassant =
                    piece.Type == PieceType.Pawn && _board[to.x, to.y].IsEnPassantTarget;

                // Clear previous en passant target
                if (_enPassantTarget.HasValue)
                {
                    var eq = _board[_enPassantTarget.Value.x, _enPassantTarget.Value.y];
                    eq.IsEnPassantTarget = false;
                    _board[_enPassantTarget.Value.x, _enPassantTarget.Value.y] = eq;
                    _enPassantTarget = null;
                }

                if (isEnPassant)
                {
                    var cap = _board[to.x, from.y];
                    cap.Piece = null;
                    _board[to.x, from.y] = cap;
                }

                var fromSq = _board[from.x, from.y];
                fromSq.Piece = null;
                _board[from.x, from.y] = fromSq;

                var toSq = _board[to.x, to.y];
                toSq.Piece = piece;
                _board[to.x, to.y] = toSq;

                piece.Position = to;
                piece.HasMoved = true;

                // En passant target for pawn double-step
                int rankDelta = to.y - from.y;
                if (piece.Type == PieceType.Pawn && (rankDelta == 2 || rankDelta == -2))
                {
                    int epRank = (from.y + to.y) / 2;
                    _enPassantTarget = new Vector2Int(from.x, epRank);
                    var epSq = _board[from.x, epRank];
                    epSq.IsEnPassantTarget = true;
                    _board[from.x, epRank] = epSq;
                }

                // Castling: relocate the rook
                int fileDelta = to.x - from.x;
                if (piece.Type == PieceType.King && (fileDelta == 2 || fileDelta == -2))
                {
                    bool kingside    = fileDelta > 0;
                    int rookFromFile = kingside ? BoardConstants.Size - 1 : 0;
                    int rookToFile   = kingside ? to.x - 1 : to.x + 1;
                    int rank         = from.y;

                    Piece rook = _board[rookFromFile, rank].Piece;

                    var rs = _board[rookFromFile, rank];
                    rs.Piece = null;
                    _board[rookFromFile, rank] = rs;

                    var rd = _board[rookToFile, rank];
                    rd.Piece = rook;
                    _board[rookToFile, rank] = rd;

                    rook.Position = new Vector2Int(rookToFile, rank);
                    rook.HasMoved = true;
                }
            }

            // Mirrors BoardManager.PromotePawn
            public void PromotePawn(Vector2Int pos, PieceType promoteTo)
            {
                PieceColor color = _board[pos.x, pos.y].Piece.Color;
                Piece promoted = promoteTo switch
                {
                    PieceType.Queen  => (Piece)new Queen (color, pos),
                    PieceType.Rook   =>        new Rook  (color, pos),
                    PieceType.Bishop =>        new Bishop(color, pos),
                    PieceType.Knight =>        new Knight(color, pos),
                    _ => throw new System.ArgumentException($"Cannot promote to {promoteTo}")
                };
                promoted.HasMoved = true;
                Place(promoted, pos.x, pos.y);
            }

            private void Place(Piece piece, int f, int r)
            {
                var sq = _board[f, r];
                sq.Piece = piece;
                _board[f, r] = sq;
            }
        }
    }
}
