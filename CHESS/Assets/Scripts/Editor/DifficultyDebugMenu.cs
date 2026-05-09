using UnityEditor;
using UnityEngine;
using Chess.AI;

namespace Chess.Editor
{
    /// <summary>
    /// Editor menu shortcuts for switching AI difficulty during playtesting.
    /// Access via: Chess ▶ Difficulty ▶ …
    ///
    /// Changes take effect immediately — press Play after switching so
    /// GameManager.Start() picks up the new PlayerPrefs value.
    /// </summary>
    public static class DifficultyDebugMenu
    {
        [MenuItem("Chess/Difficulty/Easy (depth 2 · no quiescence)")]
        private static void SetEasy()
        {
            DifficultySettings.Current = Difficulty.Easy;
            Log();
        }

        [MenuItem("Chess/Difficulty/Medium (depth 4 · quiescence + opening book)")]
        private static void SetMedium()
        {
            DifficultySettings.Current = Difficulty.Medium;
            Log();
        }

        [MenuItem("Chess/Difficulty/Hard (depth 6 · quiescence + opening book)")]
        private static void SetHard()
        {
            DifficultySettings.Current = Difficulty.Hard;
            Log();
        }

        [MenuItem("Chess/Difficulty/Show Current")]
        private static void ShowCurrent() => Log();

        // ── Validation: menu items show a checkmark next to the active level ──

        [MenuItem("Chess/Difficulty/Easy (depth 2 · no quiescence)", true)]
        private static bool ValidateEasy()
        {
            Menu.SetChecked("Chess/Difficulty/Easy (depth 2 · no quiescence)",
                DifficultySettings.Current == Difficulty.Easy);
            return true;
        }

        [MenuItem("Chess/Difficulty/Medium (depth 4 · quiescence + opening book)", true)]
        private static bool ValidateMedium()
        {
            Menu.SetChecked("Chess/Difficulty/Medium (depth 4 · quiescence + opening book)",
                DifficultySettings.Current == Difficulty.Medium);
            return true;
        }

        [MenuItem("Chess/Difficulty/Hard (depth 6 · quiescence + opening book)", true)]
        private static bool ValidateHard()
        {
            Menu.SetChecked("Chess/Difficulty/Hard (depth 6 · quiescence + opening book)",
                DifficultySettings.Current == Difficulty.Hard);
            return true;
        }

        // ── Helper ─────────────────────────────────────────────────────────

        private static void Log()
        {
            Difficulty d = DifficultySettings.Current;
            Debug.Log(
                $"[Difficulty] <b>{d}</b> — " +
                $"depth {DifficultySettings.Depth} | " +
                $"quiescence {DifficultySettings.UseQuiescence} | " +
                $"opening book {DifficultySettings.UseOpeningBook}");
        }
    }
}
