using Chess.Core.Pieces;

namespace Chess.Core.Board
{
    public struct Square
    {
        public int File { get; }
        public int Rank { get; }
        public SquareColor Color { get; }
        public Piece Piece { get; set; }

        public bool IsOccupied => Piece != null;

        public Square(int file, int rank)
        {
            File = file;
            Rank = rank;
            Color = (file + rank) % 2 == 0 ? SquareColor.Dark : SquareColor.Light;
            Piece = null;
        }

        public bool IsOccupiedByColor(PieceColor color) =>
            IsOccupied && Piece.Color == color;

        public override string ToString() => $"{(char)('A' + File)}{Rank + 1}";
    }
}
