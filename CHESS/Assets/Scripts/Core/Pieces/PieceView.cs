using UnityEngine;

namespace Chess.Core.Pieces
{
    public class PieceView : MonoBehaviour
    {
        public Piece Data { get; private set; }

        public void Initialise(Piece piece, Color tint)
        {
            Data = piece;
            GetComponent<MeshRenderer>().material.color = tint;
        }
    }
}
