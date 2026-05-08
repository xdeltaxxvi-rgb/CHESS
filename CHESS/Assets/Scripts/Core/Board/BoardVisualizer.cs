using System.Collections.Generic;
using UnityEngine;
using Chess.Core.Pieces;

namespace Chess.Core.Board
{
    public class BoardVisualizer : MonoBehaviour
    {
        [SerializeField] private BoardManager _boardManager;
        [SerializeField] private Material _lightMaterial;
        [SerializeField] private Material _darkMaterial;
        [SerializeField] private Color _whiteTint = Color.white;
        [SerializeField] private Color _blackTint = new Color(0.15f, 0.15f, 0.15f);
        [SerializeField] private float _tileSize = 1f;

        private GameObject[,] _tiles;
        private readonly Dictionary<Vector2Int, PieceView> _pieceViews = new Dictionary<Vector2Int, PieceView>();

        // Start runs after all Awake calls, so BoardManager data is ready.
        private void Start()
        {
            GenerateBoard();
            SpawnPieces();
        }

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
            tile.transform.localPosition = TilePosition(file, rank, 0f);
            tile.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            tile.transform.localScale = Vector3.one * _tileSize;

            bool isLight = (file + rank) % 2 != 0;
            tile.GetComponent<MeshRenderer>().material = isLight ? _lightMaterial : _darkMaterial;

            _tiles[file, rank] = tile;
        }

        private void SpawnPieces()
        {
            for (int file = 0; file < BoardConstants.Size; file++)
            {
                for (int rank = 0; rank < BoardConstants.Size; rank++)
                {
                    Square sq = _boardManager.GetSquare(file, rank);
                    if (sq.IsOccupied)
                        SpawnPiece(sq.Piece, file, rank);
                }
            }
        }

        private void SpawnPiece(Piece piece, int file, int rank)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = $"{piece.Color}_{piece.Type}_{(char)('A' + file)}{rank + 1}";
            go.transform.SetParent(transform);
            go.transform.localPosition = TilePosition(file, rank, _tileSize * 0.5f);
            go.transform.localScale = Vector3.one * _tileSize * 0.8f;

            Color tint = piece.Color == PieceColor.White ? _whiteTint : _blackTint;
            PieceView view = go.AddComponent<PieceView>();
            view.Initialise(piece, tint);

            _pieceViews[piece.Position] = view;
        }

        public GameObject GetTile(int file, int rank)
        {
            Debug.Assert(IsValidCoordinate(file, rank), $"({file},{rank}) out of bounds.");
            return _tiles[file, rank];
        }

        public PieceView GetPieceView(Vector2Int position) =>
            _pieceViews.TryGetValue(position, out PieceView view) ? view : null;

        public void MovePieceView(Vector2Int from, Vector2Int to)
        {
            if (!_pieceViews.TryGetValue(from, out PieceView view)) return;
            _pieceViews.Remove(from);
            _pieceViews[to] = view;
            view.transform.localPosition = TilePosition(to.x, to.y, _tileSize * 0.5f);
        }

        public void RemovePieceView(Vector2Int at)
        {
            if (!_pieceViews.TryGetValue(at, out PieceView view)) return;
            _pieceViews.Remove(at);
            Destroy(view.gameObject);
        }

        // Destroys the existing view and spawns a fresh one for the promoted piece.
        public void ReplacePieceView(Vector2Int position)
        {
            RemovePieceView(position);
            Square sq = _boardManager.GetSquare(position.x, position.y);
            if (sq.IsOccupied)
                SpawnPiece(sq.Piece, position.x, position.y);
        }

        private Vector3 TilePosition(int file, int rank, float yOffset) =>
            new Vector3(
                (file - BoardConstants.Size / 2f + 0.5f) * _tileSize,
                yOffset,
                (rank - BoardConstants.Size / 2f + 0.5f) * _tileSize
            );

        private bool IsValidCoordinate(int file, int rank) =>
            file >= 0 && file < BoardConstants.Size &&
            rank >= 0 && rank < BoardConstants.Size;
    }
}
