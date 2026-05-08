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

        // GameManager sets this each turn (Issue #15). Defaults to White for standalone testing.
        public PieceColor CurrentPlayer { get; set; } = PieceColor.White;

        private PieceView _selectedView;

        private void OnEnable() => _tileSelector.OnSquareSelected += HandleSquareSelected;
        private void OnDisable() => _tileSelector.OnSquareSelected -= HandleSquareSelected;

        private void HandleSquareSelected(Vector2Int coord)
        {
            Square sq = _boardManager.GetSquare(coord.x, coord.y);

            if (sq.IsOccupied && sq.Piece.Color == CurrentPlayer)
                Select(_boardVisualizer.GetPieceView(coord));
            else
                Deselect();
        }

        private void Select(PieceView view)
        {
            if (view == null) return;
            Deselect();
            _selectedView = view;
            _selectedView.SetTint(_selectedTint);
        }

        public void Deselect()
        {
            if (_selectedView == null) return;
            _selectedView.ResetTint();
            _selectedView = null;
        }

        public PieceView SelectedView => _selectedView;
    }
}
