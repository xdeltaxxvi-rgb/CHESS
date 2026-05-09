using Unity.Profiling;
using UnityEngine;

namespace Chess.AI
{
    /// <summary>
    /// Centralises AI performance logging and Unity Profiler markers.
    ///
    /// Profiler markers are active in Development Builds + Editor, stripped in
    /// Release builds. They appear in the Unity Profiler window and in Android
    /// profiling sessions captured via adb or the Profiler over Wi-Fi/USB.
    ///
    /// Timing thresholds match the Phase 2 acceptance criteria (Issue #33):
    ///   Depth 2 → &lt; 500 ms
    ///   Depth 4 → &lt; 2 000 ms
    ///   Depth 6 → &lt; 3 000 ms
    /// </summary>
    public static class AIPerformanceLogger
    {
        // ----- Unity Profiler markers (safe on background threads, Unity 2019.3+) ----

        /// <summary>Wraps the full iterative-deepening search for one AI move.</summary>
        public static readonly ProfilerMarker SearchMarker =
            new ProfilerMarker(ProfilerCategory.Ai, "ChessAI.Search");

        /// <summary>Wraps each negamax node visit.</summary>
        public static readonly ProfilerMarker NegamaxMarker =
            new ProfilerMarker(ProfilerCategory.Ai, "ChessAI.Negamax");

        /// <summary>Wraps quiescence search calls.</summary>
        public static readonly ProfilerMarker QuiescenceMarker =
            new ProfilerMarker(ProfilerCategory.Ai, "ChessAI.Quiescence");

        /// <summary>Wraps board evaluation calls.</summary>
        public static readonly ProfilerMarker EvaluateMarker =
            new ProfilerMarker(ProfilerCategory.Ai, "ChessAI.Evaluate");

        // ----- Acceptance-criteria thresholds (ms) --------------------------------

        private const long Depth2Ms = 500;
        private const long Depth4Ms = 2_000;
        private const long Depth6Ms = 3_000;

        // ----- Public API ---------------------------------------------------------

        /// <summary>
        /// Logs one AI move's timing to the Unity Console and marks whether the
        /// result meets the acceptance-criteria threshold for the given depth.
        /// Call this from the main thread after Task.Run returns.
        /// </summary>
        public static void LogMoveTime(int depth, long elapsedMs, int nodesSearched)
        {
            long threshold = ThresholdFor(depth);
            bool pass      = elapsedMs <= threshold;

            string icon   = pass ? "✓" : "⚠ OVER TARGET";
            string colour = pass ? "green" : "red";

            Debug.Log(
                $"[AI Perf] depth={depth} | <color={colour}>{elapsedMs} ms</color> " +
                $"(target <{threshold} ms) | {nodesSearched:N0} nodes | {icon}");
        }

        /// <summary>
        /// Logs a memory warning if Unity's GC allocation in the last AI move
        /// exceeded a reasonable threshold. Call after Profiler.GetTotalAllocatedMemoryLong()
        /// comparison if you want exact figures; this version uses a simple heuristic.
        /// </summary>
        public static void LogMemorySnapshot(long allocatedBytesBefore, long allocatedBytesAfter)
        {
            long delta = allocatedBytesAfter - allocatedBytesBefore;
            if (delta > 5 * 1024 * 1024) // > 5 MB per move is unexpected
                Debug.LogWarning($"[AI Perf] High GC allocation during AI move: {delta / 1024} KB. " +
                                 "Check for List<Move> churn in move generation.");
        }

        // ----- Helpers ------------------------------------------------------------

        private static long ThresholdFor(int depth)
        {
            if (depth <= 2) return Depth2Ms;
            if (depth <= 4) return Depth4Ms;
            return Depth6Ms;
        }
    }
}
