namespace Chess.Core.Board
{
    public struct Square
    {
        public int File { get; }
        public int Rank { get; }

        public Square(int file, int rank)
        {
            File = file;
            Rank = rank;
        }

        public override string ToString() => $"{(char)('A' + File)}{Rank + 1}";
    }
}
