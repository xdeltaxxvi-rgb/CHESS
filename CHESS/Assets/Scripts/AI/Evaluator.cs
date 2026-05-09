using Chess.Core.Board;
using Chess.Core.Pieces;

namespace Chess.AI
{
    /// <summary>
    /// Static board evaluator. Returns a score in centipawns from
    /// <c>perspective</c>'s point of view: positive = good for perspective,
    /// negative = bad.
    /// </summary>
    public static class Evaluator
    {
        // Total non-pawn material threshold for endgame detection.
        // Rook(500)*2 + Knight(320) + Bishop(330) ~ 1650 per side → below 1300 = endgame.
        private const int EndgameMaterialThreshold = 1300;

        public static int Evaluate(Square[,] board, PieceColor perspective)
        {
            int whiteScore = ScoreForColor(board, PieceColor.White);
            int blackScore = ScoreForColor(board, PieceColor.Black);
            int score = whiteScore - blackScore;
            return perspective == PieceColor.White ? score : -score;
        }

        private static int ScoreForColor(Square[,] board, PieceColor color)
        {
            bool isEndgame = IsEndgame(board);
            int score = 0;

            for (int f = 0; f < BoardConstants.Size; f++)
                for (int r = 0; r < BoardConstants.Size; r++)
                {
                    Square sq = board[f, r];
                    if (!sq.IsOccupied || sq.Piece.Color != color) continue;

                    Piece p = sq.Piece;
                    score += PieceValues.Of(p.Type);
                    score += PositionalBonus(p.Type, p.Color, f, r, isEndgame);
                }

            return score;
        }

        private static int PositionalBonus(PieceType type, PieceColor color, int file, int rank, bool isEndgame)
        {
            int idx = color == PieceColor.White
                ? PieceSquareTables.WhiteIndex(file, rank)
                : PieceSquareTables.BlackIndex(file, rank);

            return type switch
            {
                PieceType.Pawn   => PieceSquareTables.Pawn[idx],
                PieceType.Knight => PieceSquareTables.Knight[idx],
                PieceType.Bishop => PieceSquareTables.Bishop[idx],
                PieceType.Rook   => PieceSquareTables.Rook[idx],
                PieceType.Queen  => PieceSquareTables.Queen[idx],
                PieceType.King   => isEndgame
                    ? PieceSquareTables.KingEndgame[idx]
                    : PieceSquareTables.KingMiddlegame[idx],
                _                => 0
            };
        }

        private static bool IsEndgame(Square[,] board)
        {
            int whiteMajors = 0, blackMajors = 0;
            for (int f = 0; f < BoardConstants.Size; f++)
                for (int r = 0; r < BoardConstants.Size; r++)
                {
                    Square sq = board[f, r];
                    if (!sq.IsOccupied || sq.Piece.Type == PieceType.Pawn || sq.Piece.Type == PieceType.King)
                        continue;
                    if (sq.Piece.Color == PieceColor.White) whiteMajors += PieceValues.Of(sq.Piece.Type);
                    else                                     blackMajors += PieceValues.Of(sq.Piece.Type);
                }
            return whiteMajors < EndgameMaterialThreshold || blackMajors < EndgameMaterialThreshold;
        }
    }
}
