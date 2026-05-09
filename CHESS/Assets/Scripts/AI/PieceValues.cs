using Chess.Core.Pieces;

namespace Chess.AI
{
    public static class PieceValues
    {
        public const int Pawn   = 100;
        public const int Knight = 320;
        public const int Bishop = 330;
        public const int Rook   = 500;
        public const int Queen  = 900;
        public const int King   = 20000; // Effectively infinite — never traded.

        public static int Of(PieceType type) => type switch
        {
            PieceType.Pawn   => Pawn,
            PieceType.Knight => Knight,
            PieceType.Bishop => Bishop,
            PieceType.Rook   => Rook,
            PieceType.Queen  => Queen,
            PieceType.King   => King,
            _                => 0
        };
    }
}
