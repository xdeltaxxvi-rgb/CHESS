using UnityEngine;

namespace Chess.Core.Pieces
{
    public class PieceView : MonoBehaviour
    {
        public Piece Data { get; private set; }
        public Color OriginalTint { get; private set; }

        public void Initialise(Piece piece, Color tint)
        {
            Data = piece;
            OriginalTint = tint;
            GetComponent<MeshRenderer>().material.color = tint;
        }

        public void SetTint(Color color) =>
            GetComponent<MeshRenderer>().material.color = color;

        public void ResetTint() => SetTint(OriginalTint);
    }
}
