using System.Collections.Generic;
using Chess.Core.Board;
using UnityEngine;

namespace Chess.AI
{
    /// <summary>
    /// Hardcoded opening book for Medium and Hard difficulty.
    ///
    /// Keys: comma-separated move history in coordinate notation (e.g. "e2e4,e7e5,g1f3").
    ///       Each entry represents the full move sequence *before* the AI's turn.
    ///       The AI always plays Black, so keys end with White's most recent move.
    ///
    /// Values: the book move the AI should play, same notation ("b8c6").
    ///
    /// Covers: King's Pawn (e4 e5), Ruy Lopez, Italian / Two Knights, Scotch,
    ///         Queen's Pawn (d4 d5), Queen's Gambit Declined, Réti, English.
    /// </summary>
    public static class OpeningBook
    {
        // Move notation: "{fromFile}{fromRank}{toFile}{toRank}" — e.g. "e2e4"
        // File: a–h; Rank: 1–8.  Built by GameManager.MoveToBookKey(Move).

        private static readonly Dictionary<string, string> _book = new Dictionary<string, string>
        {
            // ── 1st move responses (after White's first move) ─────────────────────

            { "e2e4",   "e7e5" },   // 1.e4 → 1...e5  (King's Pawn)
            { "d2d4",   "d7d5" },   // 1.d4 → 1...d5  (Queen's Pawn)
            { "c2c4",   "e7e5" },   // 1.c4 → 1...e5  (English Opening)
            { "g1f3",   "d7d5" },   // 1.Nf3 → 1...d5 (Réti Opening)
            { "b1c3",   "d7d5" },   // 1.Nc3 → 1...d5

            // ── 2nd move responses (after White's 2nd move) ───────────────────────

            // 1.e4 e5 lines
            { "e2e4,e7e5,g1f3",   "b8c6" },  // 2.Nf3  → 2...Nc6 (Ruy Lopez / Italian setup)
            { "e2e4,e7e5,f1c4",   "g8f6" },  // 2.Bc4  → 2...Nf6 (Two Knights Defence)
            { "e2e4,e7e5,b1c3",   "g8f6" },  // 2.Nc3  → 2...Nf6 (Vienna Game)
            { "e2e4,e7e5,d2d4",   "e5d4" },  // 2.d4   → 2...exd4 (Centre Game / Scotch)
            { "e2e4,e7e5,f2f4",   "e5f4" },  // 2.f4   → 2...exf4 (King's Gambit Accepted)

            // 1.d4 d5 lines
            { "d2d4,d7d5,c2c4",   "e7e6" },  // 2.c4   → 2...e6 (QGD)
            { "d2d4,d7d5,g1f3",   "g8f6" },  // 2.Nf3  → 2...Nf6 (London territory)
            { "d2d4,d7d5,b1c3",   "g8f6" },  // 2.Nc3  → 2...Nf6

            // ── 3rd move responses (after White's 3rd move) ───────────────────────

            // Ruy Lopez: 1.e4 e5 2.Nf3 Nc6 3.Bb5
            { "e2e4,e7e5,g1f3,b8c6,f1b5",   "a7a6" },  // 3.Bb5 → 3...a6 (Morphy Defence)

            // Italian: 1.e4 e5 2.Nf3 Nc6 3.Bc4
            { "e2e4,e7e5,g1f3,b8c6,f1c4",   "f8c5" },  // 3.Bc4 → 3...Bc5 (Giuoco Piano)

            // Scotch: 1.e4 e5 2.Nf3 Nc6 3.d4
            { "e2e4,e7e5,g1f3,b8c6,d2d4",   "e5d4" },  // 3.d4  → 3...exd4

            // Two Knights: 1.e4 e5 2.Bc4 Nf6 3.Nc3
            { "e2e4,e7e5,f1c4,g8f6,b1c3",   "b8c6" },  // 3.Nc3 → 3...Nc6

            // QGD: 1.d4 d5 2.c4 e6 3.Nc3 / 3.Nf3
            { "d2d4,d7d5,c2c4,e7e6,b1c3",   "g8f6" },  // 3.Nc3 → 3...Nf6
            { "d2d4,d7d5,c2c4,e7e6,g1f3",   "g8f6" },  // 3.Nf3 → 3...Nf6

            // ── 4th move responses ────────────────────────────────────────────────

            // Ruy Lopez: 3...a6 4.Ba4
            { "e2e4,e7e5,g1f3,b8c6,f1b5,a7a6,b5a4",   "g8f6" },  // 4.Ba4 → 4...Nf6

            // Giuoco Piano: 3...Bc5 4.c3
            { "e2e4,e7e5,g1f3,b8c6,f1c4,f8c5,c2c3",   "g8f6" },  // 4.c3  → 4...Nf6

            // Scotch: 3...exd4 4.Nxd4
            { "e2e4,e7e5,g1f3,b8c6,d2d4,e5d4,f3d4",   "f8c5" },  // 4.Nxd4 → 4...Bc5

            // QGD: 3...Nf6 4.Bg5
            { "d2d4,d7d5,c2c4,e7e6,b1c3,g8f6,c1g5",   "f8e7" },  // 4.Bg5 → 4...Be7
        };

        // ----- Public API --------------------------------------------------------

        /// <summary>
        /// Returns true and outputs the book <see cref="Move"/> if the position
        /// is in the opening book. Returns false if not found or the move cannot
        /// be parsed into a valid board coordinate.
        /// </summary>
        public static bool TryGetMove(string moveSequenceKey, out Move bookMove)
        {
            bookMove = default;

            if (string.IsNullOrEmpty(moveSequenceKey)) return false;
            if (!_book.TryGetValue(moveSequenceKey, out string notation)) return false;

            if (!TryParseNotation(notation, out Vector2Int from, out Vector2Int to)) return false;

            bookMove = new Move(from, to);
            return true;
        }

        // ----- Helpers -----------------------------------------------------------

        // Parses "e7e5" → from=(4,6), to=(4,4)
        private static bool TryParseNotation(string n, out Vector2Int from, out Vector2Int to)
        {
            from = to = Vector2Int.zero;
            if (n == null || n.Length != 4) return false;

            int ff = n[0] - 'a'; int fr = n[1] - '1';
            int tf = n[2] - 'a'; int tr = n[3] - '1';

            if (ff < 0 || ff > 7 || fr < 0 || fr > 7 ||
                tf < 0 || tf > 7 || tr < 0 || tr > 7) return false;

            from = new Vector2Int(ff, fr);
            to   = new Vector2Int(tf, tr);
            return true;
        }

        /// <summary>
        /// Converts a <see cref="Move"/> to the coordinate string used as a book key
        /// segment (e.g. from=(4,1), to=(4,3) → "e2e4").
        /// Call this from GameManager after every move to build the history key.
        /// </summary>
        public static string MoveToKey(Move move) =>
            $"{(char)('a' + move.From.x)}{move.From.y + 1}" +
            $"{(char)('a' + move.To.x)}{move.To.y + 1}";
    }
}
