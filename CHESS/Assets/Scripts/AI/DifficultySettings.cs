using UnityEngine;

namespace Chess.AI
{
    /// <summary>
    /// Global difficulty levels. Player chooses once; applies to all chapters.
    /// Stored in PlayerPrefs (key: "Difficulty") — persists across sessions.
    /// SaveManager (Phase 5) will migrate this into SaveData.json.
    /// </summary>
    public enum Difficulty
    {
        Easy   = 0, // Depth 2, no quiescence, no opening book — accessible to beginners
        Medium = 1, // Depth 4, quiescence on, opening book on — solid tactical play
        Hard   = 2, // Depth 6, quiescence on, opening book on — strong resistance
    }

    /// <summary>
    /// Maps the active <see cref="Difficulty"/> to search parameters.
    /// Read by GameManager on scene load; also exposed for the Settings UI (Phase 5).
    /// </summary>
    public static class DifficultySettings
    {
        private const string PrefKey = "Difficulty";

        // ----- Persistence -------------------------------------------------------

        public static Difficulty Current
        {
            get  => (Difficulty)PlayerPrefs.GetInt(PrefKey, (int)Difficulty.Medium);
            set  { PlayerPrefs.SetInt(PrefKey, (int)value); PlayerPrefs.Save(); }
        }

        // ----- Search parameters -------------------------------------------------

        /// <summary>Negamax search depth for the current difficulty.</summary>
        public static int Depth => Current switch
        {
            Difficulty.Easy   => 2,
            Difficulty.Medium => 4,
            Difficulty.Hard   => 6,
            _                 => 4,
        };

        /// <summary>Quiescence search extends captures at leaf nodes (off on Easy).</summary>
        public static bool UseQuiescence => Current != Difficulty.Easy;

        /// <summary>Opening book used on Medium and Hard only.</summary>
        public static bool UseOpeningBook => Current != Difficulty.Easy;
    }
}
