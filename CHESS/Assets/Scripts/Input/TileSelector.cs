using System;
using UnityEngine;

namespace Chess.Input
{
    public class TileSelector : MonoBehaviour
    {
        [SerializeField] private Camera _camera;

        public event Action<Vector2Int> OnSquareSelected;

        private void Awake()
        {
            if (_camera == null)
                _camera = Camera.main;
        }

        private void Update()
        {
            if (!TryGetInputPosition(out Vector2 screenPos))
                return;

            Ray ray = _camera.ScreenPointToRay(screenPos);
            if (!Physics.Raycast(ray, out RaycastHit hit))
                return;

            if (TryParseSquare(hit.collider.gameObject, out Vector2Int coord))
                OnSquareSelected?.Invoke(coord);
        }

        private static bool TryGetInputPosition(out Vector2 screenPos)
        {
            if (UnityEngine.Input.touchCount > 0 &&
                UnityEngine.Input.GetTouch(0).phase == TouchPhase.Began)
            {
                screenPos = UnityEngine.Input.GetTouch(0).position;
                return true;
            }

            if (UnityEngine.Input.GetMouseButtonDown(0))
            {
                screenPos = UnityEngine.Input.mousePosition;
                return true;
            }

            screenPos = default;
            return false;
        }

        private static bool TryParseSquare(GameObject hit, out Vector2Int coord)
        {
            // Tiles are named "A1"–"H8".
            // Piece cubes are named "White_King_E1" — the third token is the tile.
            string token = hit.name.Contains("_")
                ? hit.name.Split('_')[2]
                : hit.name;

            if (token.Length == 2 &&
                token[0] >= 'A' && token[0] <= 'H' &&
                token[1] >= '1' && token[1] <= '8')
            {
                coord = new Vector2Int(token[0] - 'A', token[1] - '1');
                return true;
            }

            coord = default;
            return false;
        }
    }
}
