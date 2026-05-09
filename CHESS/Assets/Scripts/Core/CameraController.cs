using UnityEngine;

namespace Chess.Core
{
    /// <summary>
    /// Drives the main orthographic camera for portrait-first play.
    ///
    /// Targets the same visual style as Clash Royale:
    ///   • True isometric angle  — pitch 35.264° (arctan(1/√2)), yaw 45°
    ///   • Board in upper ~70%   — _verticalViewOffset shifts the view to
    ///     leave room for the Phase-5 HUD at the bottom
    ///   • Dynamic orthoSize     — board corners are projected onto the
    ///     camera axes at runtime so the size is always correct regardless
    ///     of pitch, yaw, or screen resolution
    ///
    /// Call Apply() after a screen-orientation change (Phase 7).
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class CameraController : MonoBehaviour
    {
        // ── Inspector ─────────────────────────────────────────────────────────

        [Tooltip("Camera pitch in degrees.\n" +
                 "55° = Clash Royale portrait style — board faces the player, ranks are\n" +
                 "clearly readable, fills portrait width as a rectangle (not a diamond).\n" +
                 "35.264° = true isometric (arctan 1/√2) — diagonal view, board appears\n" +
                 "as a rotated square. Do not go below 30°.")]
        [SerializeField] private float _pitch = 55f;

        [Tooltip("Camera yaw in degrees.\n" +
                 "0° = board faces player straight-on (Clash Royale style, rectangle fills\n" +
                 "portrait width). 45° = diagonal isometric (board appears as a diamond).")]
        [SerializeField] private float _yaw = 0f;

        [Tooltip("World units the camera sits above the look-at plane.")]
        [SerializeField] private float _height = 18f;

        [Tooltip("World-unit padding added around the board when computing orthographic size.")]
        [SerializeField] private float _padding = 0.3f;

        [Tooltip("Shifts the look-at point below board centre (in camera-up space), " +
                 "pushing the board toward the top of the screen. " +
                 "Increase to raise the board, decrease to lower it.")]
        [SerializeField] private float _verticalViewOffset = 2.0f;

        // ── Fixed geometry ────────────────────────────────────────────────────

        // Board occupies (0,0,0)→(7,0,7); centre at (3.5, 0, 3.5).
        private static readonly Vector3 BoardCentre = new Vector3(3.5f, 0f, 3.5f);

        // World-space corners of the board used to compute the bounding box.
        private static readonly Vector3[] BoardCorners =
        {
            new Vector3(0f, 0f, 0f),
            new Vector3(7f, 0f, 0f),
            new Vector3(0f, 0f, 7f),
            new Vector3(7f, 0f, 7f),
        };

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
        /// Re-applies all camera settings. Call after a screen-orientation
        /// change or a board-layout change (Phase 7 responsive scaling).
        /// </summary>
        public void Apply()
        {
            if (_cam == null) _cam = GetComponent<Camera>();
            _cam.orthographic = true;

            // ── 1. Rotation ───────────────────────────────────────────────────
            transform.rotation = Quaternion.Euler(_pitch, _yaw, 0f);

            // ── 2. Position ───────────────────────────────────────────────────
            // The look-at point is shifted below BoardCentre along the camera-up
            // axis so the board sits in the upper portion of the portrait screen
            // (leaving the bottom ~30% for the Phase-5 HUD).
            //
            // Camera position = lookAt - forward × d,   d = height / sin(pitch)
            float   sinPitch = Mathf.Sin(_pitch * Mathf.Deg2Rad);
            float   d        = _height / sinPitch;
            Vector3 lookAt   = BoardCentre - transform.up * _verticalViewOffset;
            transform.position = lookAt - transform.forward * d;

            // ── 3. Orthographic size ──────────────────────────────────────────
            // Project every board corner onto the camera's right and up axes.
            // The bounding box of those projections gives the exact world-unit
            // extents the camera must show — no hardcoded constants needed.
            float aspect  = (float)Screen.width / Screen.height;
            Vector3 right = transform.right;
            Vector3 up    = transform.up;

            float maxHalfWidth  = 0f;
            float maxHalfHeight = 0f;

            foreach (Vector3 corner in BoardCorners)
            {
                // Project relative to look-at so vertical offset is baked in.
                Vector3 offset = corner - lookAt;
                float sx = Mathf.Abs(Vector3.Dot(offset, right));
                float sy = Vector3.Dot(offset, up);   // signed: we care about the top

                maxHalfWidth  = Mathf.Max(maxHalfWidth,  sx);
                maxHalfHeight = Mathf.Max(maxHalfHeight, sy);
            }

            // Width  constraint (binding in portrait  ~0.46): half-width ÷ aspect
            // Height constraint (binding in landscape ~1.78): half-height + padding
            float sizeForWidth  = (maxHalfWidth  + _padding) / aspect;
            float sizeForHeight =  maxHalfHeight + _padding;

            _cam.orthographicSize = Mathf.Max(sizeForWidth, sizeForHeight);
        }

        // ── Editor support ────────────────────────────────────────────────────

#if UNITY_EDITOR
        private void OnValidate()
        {
            _cam = GetComponent<Camera>();
            if (_cam != null) Apply();
        }
#endif
    }
}
