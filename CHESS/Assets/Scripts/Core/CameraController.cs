using UnityEngine;

namespace Chess.Core
{
    /// <summary>
    /// Drives the main orthographic camera for portrait-first play.
    ///
    /// The camera maintains a fixed isometric angle (pitch 50°, yaw 45°) and
    /// dynamically computes its orthographic size so the full board is always
    /// visible on any supported screen resolution.  A vertical offset shifts
    /// the board toward the top of the screen, reserving the lower portion for
    /// the Phase-5 HUD.
    ///
    /// Orthographic size is the maximum of two constraints:
    ///   • Portrait  (aspect &lt; 1): width binding  → (boardHalfWidth  + padding) / aspect
    ///   • Landscape (aspect ≥ 1): height binding → boardHalfHeight + padding + verticalOffset
    ///
    /// Board projected dimensions (derived from pitch=50°, yaw=45°, board=8×8 units):
    ///   Half-width  in screen space = 7 × sin 45° ≈ 4.95 world units
    ///   Half-height from centre     = 3.80 world units  (board centre to far corner)
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class CameraController : MonoBehaviour
    {
        // ── Inspector ─────────────────────────────────────────────────────────

        [Tooltip("World-unit padding added to each side of the board's projected extents " +
                 "before computing orthographic size. Increase to zoom out, decrease to zoom in.")]
        [SerializeField] private float _padding = 0.75f;

        [Tooltip("How far below board centre the camera's look-at point sits, measured in " +
                 "camera-up world units. Positive values push the board toward the top of the " +
                 "screen, leaving space for the Phase-5 HUD at the bottom. " +
                 "Tweak in the Inspector to taste; 4.5 centres the board in the upper ~70%.")]
        [SerializeField] private float _verticalViewOffset = 4.5f;

        // ── Fixed geometry (CLAUDE.md architecture decision) ──────────────────

        // 50° pitch is required — 30° compresses the far rank and makes it unclickable.
        private const float Pitch  = 50f;
        private const float Yaw    = 45f;
        // Camera sits 18 world units above the look-at plane.
        private const float Height = 18f;

        // Board occupies (0,0,0) → (7,0,7); its centre is at (3.5, 0, 3.5).
        private static readonly Vector3 BoardCentre = new Vector3(3.5f, 0f, 3.5f);

        // Precomputed projected extents of the 8×8 board at pitch=50°, yaw=45°.
        //   HalfWidthInScreen  = 7 × cos 45° = 4.95  (dominant for portrait)
        //   HalfHeightInScreen = distance from board centre to far corner ≈ 3.80
        // These are fixed because the camera angle is fixed.
        private const float HalfWidthInScreen  = 4.95f;
        private const float HalfHeightInScreen = 3.80f;

        // ── Runtime ───────────────────────────────────────────────────────────

        private Camera _cam;

        // ── Unity lifecycle ───────────────────────────────────────────────────

        private void Awake()
        {
            _cam = GetComponent<Camera>();
            Apply();
        }

        // ── Public API ────────────────────────────────────────────────────────

        /// <summary>
        /// Re-applies all camera settings.  Call after a screen-orientation
        /// change or if the board layout changes (Phase 7 responsive scaling).
        /// </summary>
        public void Apply()
        {
            if (_cam == null) _cam = GetComponent<Camera>();

            _cam.orthographic = true;

            // ── 1. Rotation ───────────────────────────────────────────────────
            transform.rotation = Quaternion.Euler(Pitch, Yaw, 0f);

            // ── 2. Position ───────────────────────────────────────────────────
            // Look-at is shifted below BoardCentre along the camera's up axis so
            // the rendered board sits in the upper portion of the portrait screen.
            //
            // Camera position = lookAt − forward × d,  where d = Height / sin(pitch).
            // d ≈ 18 / 0.766 ≈ 23.5 world units.
            float   d      = Height / Mathf.Sin(Pitch * Mathf.Deg2Rad);
            Vector3 lookAt = BoardCentre - transform.up * _verticalViewOffset;
            transform.position = lookAt - transform.forward * d;

            // ── 3. Orthographic size ──────────────────────────────────────────
            // Width  constraint: board half-width must fit in half the viewport width.
            // Height constraint: board half-height + vertical offset must fit in half height.
            float aspect        = (float)Screen.width / Screen.height;
            float sizeForWidth  = (HalfWidthInScreen  + _padding) / aspect;
            float sizeForHeight =  HalfHeightInScreen + _padding + _verticalViewOffset;

            // Portrait  (~0.46 aspect, e.g. 1080×2340): sizeForWidth  ≈ 12.3 (binding)
            // Landscape (~1.78 aspect, e.g. 1920×1080): sizeForHeight ≈  9.1 (binding)
            _cam.orthographicSize = Mathf.Max(sizeForWidth, sizeForHeight);
        }

        // ── Editor support ────────────────────────────────────────────────────

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Re-apply immediately when Inspector values are tweaked in the editor.
            _cam = GetComponent<Camera>();
            if (_cam != null) Apply();
        }
#endif
    }
}
