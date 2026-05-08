using UnityEngine;

namespace Chess.Core.Board
{
    public class BoardVisualizer : MonoBehaviour
    {
        [SerializeField] private Material _lightMaterial;
        [SerializeField] private Material _darkMaterial;
        [SerializeField] private float _tileSize = 1f;

        private GameObject[,] _tiles;

        private void Awake() => GenerateBoard();

        private void GenerateBoard()
        {
            _tiles = new GameObject[BoardConstants.Size, BoardConstants.Size];
            for (int file = 0; file < BoardConstants.Size; file++)
                for (int rank = 0; rank < BoardConstants.Size; rank++)
                    CreateTile(file, rank);
        }

        private void CreateTile(int file, int rank)
        {
            GameObject tile = GameObject.CreatePrimitive(PrimitiveType.Quad);
            tile.name = $"{(char)('A' + file)}{rank + 1}";
            tile.transform.SetParent(transform);
            tile.transform.localPosition = new Vector3(
                (file - BoardConstants.Size / 2f + 0.5f) * _tileSize,
                0f,
                (rank - BoardConstants.Size / 2f + 0.5f) * _tileSize
            );
            tile.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            tile.transform.localScale = Vector3.one * _tileSize;

            bool isLight = (file + rank) % 2 != 0;
            tile.GetComponent<MeshRenderer>().material = isLight ? _lightMaterial : _darkMaterial;

            _tiles[file, rank] = tile;
        }

        public GameObject GetTile(int file, int rank)
        {
            Debug.Assert(IsValidCoordinate(file, rank), $"({file},{rank}) out of bounds.");
            return _tiles[file, rank];
        }

        private bool IsValidCoordinate(int file, int rank) =>
            file >= 0 && file < BoardConstants.Size &&
            rank >= 0 && rank < BoardConstants.Size;
    }
}
