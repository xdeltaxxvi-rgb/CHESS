using Chess.Core.Board;

namespace Chess.AI
{
    /// <summary>
    /// Describes what kind of bound a TT entry represents.
    /// </summary>
    public enum TTFlag : byte
    {
        Exact,       // Score is exact for this position at this depth.
        LowerBound,  // Score caused a beta cutoff — real score is >= this value.
        UpperBound,  // All moves failed low  — real score is <= this value.
    }

    /// <summary>One slot in the transposition table.</summary>
    public struct TTEntry
    {
        public ulong  Hash;
        public int    Depth;
        public int    Score;
        public TTFlag Flag;
        public Move?  BestMove;
    }

    /// <summary>
    /// Fixed-size transposition table backed by a flat array.
    /// Uses always-replace replacement policy (simplest, works well in practice).
    /// Thread-safe for a single writer (the search thread) + no concurrent readers.
    /// </summary>
    public sealed class TranspositionTable
    {
        private readonly TTEntry[] _table;
        private readonly int       _size;

        /// <param name="sizeMB">Approximate memory budget in megabytes.</param>
        public TranspositionTable(int sizeMB = 32)
        {
            // TTEntry ≈ 32 bytes (hash 8 + depth 4 + score 4 + flag 1 + padding + Move? 16)
            _size  = (sizeMB * 1024 * 1024) / 32;
            _table = new TTEntry[_size];
        }

        public void Clear() => System.Array.Clear(_table, 0, _size);

        private int Index(ulong hash) => (int)(hash % (ulong)_size);

        // ----- Write -------------------------------------------------------------

        public void Store(ulong hash, int depth, int score, TTFlag flag, Move? bestMove)
        {
            int idx = Index(hash);
            ref TTEntry e = ref _table[idx];
            // Always-replace: newer / deeper entries overwrite older ones.
            e.Hash     = hash;
            e.Depth    = depth;
            e.Score    = score;
            e.Flag     = flag;
            e.BestMove = bestMove;
        }

        // ----- Read --------------------------------------------------------------

        /// <summary>
        /// Returns true and sets <paramref name="score"/> if the stored entry is
        /// usable at the requested depth with the current alpha/beta window.
        /// Always sets <paramref name="bestMove"/> from the TT (may be null).
        /// </summary>
        public bool TryGet(ulong hash, int depth, int alpha, int beta,
                           out int score, out Move? bestMove)
        {
            score    = 0;
            bestMove = null;

            int idx = Index(hash);
            TTEntry e = _table[idx];

            if (e.Hash != hash || e.Depth < depth) return false;

            bestMove = e.BestMove;

            switch (e.Flag)
            {
                case TTFlag.Exact:
                    score = e.Score;
                    return true;

                case TTFlag.LowerBound when e.Score >= beta:
                    score = beta;
                    return true;

                case TTFlag.UpperBound when e.Score <= alpha:
                    score = alpha;
                    return true;

                default:
                    return false;
            }
        }

        /// <summary>Returns the best move stored for this hash, if any.</summary>
        public Move? GetBestMove(ulong hash)
        {
            int idx = Index(hash);
            TTEntry e = _table[idx];
            return e.Hash == hash ? e.BestMove : null;
        }
    }
}
