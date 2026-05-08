using System;
using System.Collections.Generic;
using UnityEngine;
using Chess.Core.Board;
using Chess.Core.Pieces;
using static Chess.Core.Board.MoveValidator;

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

        // GameManager sets this each turn (Issue #20). Defaults to White for standalone testing.
        public PieceColor CurrentPlayer { get; set; } = PieceColor.White;

        // GameManager subscribes to switch turn after each move.
        public event Action OnMoveExecuted;

        private PieceView _selectedView;
        private List<Vector2Int> _validMoves = new List<Vector2Int>();
        private readonly List<(GameObject tile, Color originalColor)> _highlights
            = new List<(GameObject, Color)>();

        private void OnEnable() => _tileSelector.OnSquareSelected += HandleSquareSelected;
        private void OnDisable() => _tileSelector.OnSquareSelected -= HandleSquareSelected;

        private void HandleSquareSelected(Vector2Int coord)
        {
            if (_selectedView != null && _validMoves.Contains(coord))
            {
                ExecuteMove(_selectedView.Data.Position, coord);
                return;
            }

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
            _validMoves = GetLegalMoves(piece, _boardManager.GetBoard());
            HighlightMoves(_validMoves);
        }

        private void HighlightMoves(List<Vector2Int> moves)
        {
            Square[,] board = _boardManager.GetBoard();
            foreach (Vector2Int coord in moves)
            {
                GameObject tile = _boardVisualizer.GetTile(coord.x, coord.y);
                if (tile == null) continue;
                var rend = tile.GetComponent<MeshRenderer>();
                _highlights.Add((tile, rend.material.color));
                rend.material.color = board[coord.x, coord.y].IsOccupied ? _captureTint : _moveTint;
            }
        }

        private void ExecuteMove(Vector2Int from, Vector2Int to)
        {
            Square[,] board = _boardManager.GetBoard();
            if (board[to.x, to.y].IsOccupied)
                _boardVisualizer.RemovePieceView(to);

            _boardManager.ExecuteMove(from, to);
            _boardVisualizer.MovePieceView(from, to);

            Deselect();
            OnMoveExecuted?.Invoke();
        }

        public void Deselect()
        {
            foreach (var (tile, originalColor) in _highlights)
                tile.GetComponent<MeshRenderer>().material.color = originalColor;
            _highlights.Clear();
            _validMoves.Clear();

            if (_selectedView == null) return;
            _selectedView.ResetTint();
            _selectedView = null;
        }

        public PieceView SelectedView => _selectedView;
    }
}
