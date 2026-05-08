using System.Collections.Generic;
using UnityEngine;
using Chess.Core.Board;

namespace Chess.Core.Pieces
{
    public sealed class King : Piece
    {
        public King(PieceColor color, Vector2Int position)
            : base(color, position, PieceType.King) { }

        public override List<Vector2Int> GetValidMoves(Square[,] board) =>
            new List<Vector2Int>();
    }
}
