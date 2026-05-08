using UnityEngine;

namespace Chess.Core.Pieces
{
    public class PieceView : MonoBehaviour
    {
        public Piece Data { get; private set; }
        public Color OriginalTint { get; private set; }

        private MeshRenderer _renderer;

        public void Initialise(Piece piece, Color tint)
        {
            Data = piece;
            OriginalTint = tint;
            _renderer = GetComponent<MeshRenderer>();
            _renderer.material.color = tint;
        }

        public void SetTint(Color color) => _renderer.material.color = color;

        public void ResetTint() => SetTint(OriginalTint);
    }
}
