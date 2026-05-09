using Chess.Core.Board;
using Chess.Core.Pieces;

namespace Chess.AI
{
    /// <summary>
    /// Static board evaluator. Returns a score in centipawns from
    /// <c>perspective</c>'s point of view: positive = good, negative = bad.
    ///
    /// Scoring layers (in order of application):
    ///   1. Material value
    ///   2. Piece-square table bonus (positional)
    ///   3. Pawn structure (doubled, isolated, passed pawns)
    ///   4. King safety — pawn shield (middlegame only)
    ///   5. Bishop pair bonus
    ///   6. Mobility (pseudo-legal move count difference)
    /// </summary>
    public static class Evaluator
    {
        // Total non-pawn material threshold for endgame detection.
        // Rook(500)*2 + Knight(320) + Bishop(330) ~ 1650 per side → below 1300 = endgame.
        private const int EndgameMaterialThreshold = 1300;

        // Pawn structure
        private const int DoubledPawnPenalty   = -20;  // Per extra pawn above 1 on same file
        private const int IsolatedPawnPenalty  = -15;  // No friendly pawn on adjacent files
        private const int PassedPawnBase       =  25;  // Bonus per passed pawn (grows with rank)
        private const int PassedPawnRankScale  =   8;  // Added per rank advanced past start

        // Other positional
        private const int BishopPairBonus = 30; // Both bishops present
        private const int MobilityBonus   =  3; // Per pseudo-legal move advantage over opponent
        private const int PawnShieldBonus =  8; // Per pawn directly in front of the king

        // ----- Public API --------------------------------------------------------

        public static int Evaluate(Square[,] board, PieceColor perspective)
        {
            int whiteScore = ScoreForColor(board, PieceColor.White);
            int blackScore = ScoreForColor(board, PieceColor.Black);

            int score = whiteScore - blackScore;

            // Mobility: pseudo-legal move count (GetValidMoves, not GetLegalMoves — faster)
            int whiteMobility = CountMobility(board, PieceColor.White);
            int blackMobility = CountMobility(board, PieceColor.Black);
            score += MobilityBonus * (whiteMobility - blackMobility);

            return perspective == PieceColor.White ? score : -score;
        }

        // ----- Per-color scoring -------------------------------------------------

        private static int ScoreForColor(Square[,] board, PieceColor color)
        {
            bool isEndgame = IsEndgame(board);
            int  score     = 0;
            int  bishopCount = 0;

            // Track pawn count per file for structure evaluation.
            int[] pawnFileCounts = new int[8];

            for (int f = 0; f < BoardConstants.Size; f++)
                for (int r = 0; r < BoardConstants.Size; r++)
                {
                    Square sq = board[f, r];
                    if (!sq.IsOccupied || sq.Piece.Color != color) continue;

                    Piece p = sq.Piece;
                    score += PieceValues.Of(p.Type);
                    score += PositionalBonus(p.Type, p.Color, f, r, isEndgame);

                    if (p.Type == PieceType.Bishop) bishopCount++;
                    if (p.Type == PieceType.Pawn)   pawnFileCounts[f]++;
                }

            // Bishop pair
            if (bishopCount >= 2)
                score += BishopPairBonus;

            // Pawn structure
            score += EvaluatePawnStructure(board, color, pawnFileCounts);

            // King safety (middlegame only — in endgame, centralise instead)
            if (!isEndgame)
                score += EvaluateKingSafety(board, color);

            return score;
        }

        // ----- Pawn structure ----------------------------------------------------

        private static int EvaluatePawnStructure(Square[,] board, PieceColor color,
                                                  int[] pawnFileCounts)
        {
            int score = 0;
            PieceColor opponent = color == PieceColor.White ? PieceColor.Black : PieceColor.White;

            for (int f = 0; f < 8; f++)
            {
                if (pawnFileCounts[f] == 0) continue;

                // Doubled pawns: penalise each extra pawn above one on the same file.
                if (pawnFileCounts[f] >= 2)
                    score += DoubledPawnPenalty * (pawnFileCounts[f] - 1);

                // Isolated pawns: no friendly pawn on either adjacent file.
                bool hasLeft  = f > 0 && pawnFileCounts[f - 1] > 0;
                bool hasRight = f < 7 && pawnFileCounts[f + 1] > 0;
                if (!hasLeft && !hasRight)
                    score += IsolatedPawnPenalty;

                // Passed pawns: checked per pawn in EvaluatePassedPawns.
                score += EvaluatePassedPawns(board, color, opponent, f);
            }

            return score;
        }

        private static int EvaluatePassedPawns(Square[,] board, PieceColor color,
                                                PieceColor opponent, int file)
        {
            int score = 0;
            int direction = color == PieceColor.White ? 1 : -1;

            for (int r = 0; r < 8; r++)
            {
                Square sq = board[file, r];
                if (!sq.IsOccupied || sq.Piece.Color != color ||
                    sq.Piece.Type != PieceType.Pawn) continue;

                // A pawn is passed if no opponent pawn on the same or adjacent
                // files can block or capture it on the way to promotion.
                bool passed = true;

                int checkEnd = color == PieceColor.White ? 7 : 0;
                for (int cr = r + direction;
                     color == PieceColor.White ? cr <= checkEnd : cr >= checkEnd;
                     cr += direction)
                {
                    int fMin = System.Math.Max(0, file - 1);
                    int fMax = System.Math.Min(7, file + 1);
                    for (int cf = fMin; cf <= fMax; cf++)
                    {
                        Square csq = board[cf, cr];
                        if (csq.IsOccupied && csq.Piece.Color == opponent &&
                            csq.Piece.Type == PieceType.Pawn)
                        {
                            passed = false;
                            break;
                        }
                    }
                    if (!passed) break;
                }

                if (passed)
                {
                    // Bonus grows as the pawn advances toward promotion.
                    int advanceRank = color == PieceColor.White ? r : 7 - r;
                    score += PassedPawnBase + advanceRank * PassedPawnRankScale;
                }
            }

            return score;
        }

        // ----- King safety -------------------------------------------------------

        private static int EvaluateKingSafety(Square[,] board, PieceColor color)
        {
            // Locate the king.
            int kf = -1, kr = -1;
            for (int f = 0; f < 8 && kf == -1; f++)
                for (int r = 0; r < 8 && kf == -1; r++)
                    if (board[f, r].IsOccupied &&
                        board[f, r].Piece.Color == color &&
                        board[f, r].Piece.Type  == PieceType.King)
                    { kf = f; kr = r; }

            if (kf == -1) return 0;

            int shieldRank = color == PieceColor.White ? kr + 1 : kr - 1;
            if (shieldRank < 0 || shieldRank >= 8) return 0;

            // Count pawns directly in front of the king (3-square shield).
            int score = 0;
            for (int df = -1; df <= 1; df++)
            {
                int sf = kf + df;
                if (sf < 0 || sf >= 8) continue;

                if (board[sf, shieldRank].IsOccupied &&
                    board[sf, shieldRank].Piece.Color == color &&
                    board[sf, shieldRank].Piece.Type  == PieceType.Pawn)
                    score += PawnShieldBonus;
            }

            return score;
        }

        // ----- Mobility ----------------------------------------------------------

        private static int CountMobility(Square[,] board, PieceColor color)
        {
            int count = 0;
            for (int f = 0; f < BoardConstants.Size; f++)
                for (int r = 0; r < BoardConstants.Size; r++)
                {
                    Square sq = board[f, r];
                    if (!sq.IsOccupied || sq.Piece.Color != color) continue;
                    // Pseudo-legal (no check filtering) — fast and sufficient for scoring.
                    count += sq.Piece.GetValidMoves(board).Count;
                }
            return count;
        }

        // ----- Helpers -----------------------------------------------------------

        private static int PositionalBonus(PieceType type, PieceColor color,
                                           int file, int rank, bool isEndgame)
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
                    if (!sq.IsOccupied ||
                        sq.Piece.Type == PieceType.Pawn ||
                        sq.Piece.Type == PieceType.King) continue;

                    if (sq.Piece.Color == PieceColor.White)
                        whiteMajors += PieceValues.Of(sq.Piece.Type);
                    else
                        blackMajors += PieceValues.Of(sq.Piece.Type);
                }
            return whiteMajors < EndgameMaterialThreshold ||
                   blackMajors < EndgameMaterialThreshold;
        }
    }
}
