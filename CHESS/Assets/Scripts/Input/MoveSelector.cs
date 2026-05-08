using System.Collections.Generic;
using UnityEngine;
using Chess.Core.Board;
using Chess.Core.Pieces;

namespace Chess.Input
{
    public class MoveSelector : MonoBehaviour
    {
        [SerializeField] private BoardManager _boardManager;
        [SerializeField] private BoardVisualizer _boardVisualizer;
        [SerializeField] private TileSelector _tileSelector;
        [SerializeField] private Color _selectedTint = Color.yellow;
        [SerializeField] private Color _moveTint = new Color(0f, 0.8f, 0f);
        [SerializeField] private Color _captureTint = new Color(0.8f, 0f, 0f);

        // GameManager sets this each turn (Issue #15). Defaults to White for standalone testing.
        public PieceColor CurrentPlayer { get; set; } = PieceColor.White;

        private PieceView _selectedView;
        private readonly List<(GameObject tile, Color originalColor)> _highlights
            = new List<(GameObject, Color)>();

        private void OnEnable() => _tileSelector.OnSquareSelected += HandleSquareSelected;
        private void OnDisable() => _tileSelector.OnSquareSelected -= HandleSquareSelected;

        private void HandleSquareSelected(Vector2Int coord)
        {
            Square sq = _boardManager.GetSquare(coord.x, coord.y);

            if (sq.IsOccupied && sq.Piece.Color == CurrentPlayer)
                Select(_boardVisualizer.GetPieceView(coord), sq.Piece);
            else
                Deselect();
        }

        private void Select(PieceView view, Piece piece)
        {
            if (view == null) return;
            Deselect();
            _selectedView = view;
            _selectedView.SetTint(_selectedTint);
            HighlightMoves(piece);
        }

        private void HighlightMoves(Piece piece)
        {
            Square[,] board = _boardManager.GetBoard();
            foreach (Vector2Int coord in piece.GetValidMoves(board))
            {
                GameObject tile = _boardVisualizer.GetTile(coord.x, coord.y);
                if (tile == null) continue;
                var rend = tile.GetComponent<MeshRenderer>();
                _highlights.Add((tile, rend.material.color));
                rend.material.color = board[coord.x, coord.y].IsOccupied ? _captureTint : _moveTint;
            }
        }

        public void Deselect()
        {
            foreach (var (tile, originalColor) in _highlights)
                tile.GetComponent<MeshRenderer>().material.color = originalColor;
            _highlights.Clear();

            if (_selectedView == null) return;
            _selectedView.ResetTint();
            _selectedView = null;
        }

        public PieceView SelectedView => _selectedView;
    }
}
