using UnityEngine;

namespace Chess.Core.Board
{
    /// <summary>
    /// Immutable value type representing a single board move.
    /// Used by IChessAI and anywhere a from→to pair is needed.
    /// </summary>
    public readonly struct Move
    {
        public Vector2Int From { get; }
        public Vector2Int To   { get; }

        public Move(Vector2Int from, Vector2Int to)
        {
            From = from;
            To   = to;
        }

        public override string ToString() => $"{From} → {To}";
    }
}
