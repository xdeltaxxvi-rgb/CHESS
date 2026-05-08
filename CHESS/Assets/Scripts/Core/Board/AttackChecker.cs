using Chess.Core.Pieces;

namespace Chess.Core.Board
{
    public static class AttackChecker
    {
        private static readonly int[] PawnFileDelta   = { -1,  1 };
        private static readonly int[] KnightFileDelta = {  2,  2, -2, -2,  1,  1, -1, -1 };
        private static readonly int[] KnightRankDelta = {  1, -1,  1, -1,  2, -2,  2, -2 };

        public static bool IsAttackedBy(Square[,] board, int file, int rank, PieceColor attacker)
        {
            return AttackedByPawn(board, file, rank, attacker)
                || AttackedByKnight(board, file, rank, attacker)
                || AttackedByStraightSlider(board, file, rank, attacker)
                || AttackedByDiagSlider(board, file, rank, attacker)
                || AttackedByKing(board, file, rank, attacker);
        }

        private static bool AttackedByPawn(Square[,] board, int file, int rank, PieceColor attacker)
        {
            int pawnRank = rank - (attacker == PieceColor.White ? 1 : -1);
            if (pawnRank < 0 || pawnRank >= BoardConstants.Size) return false;
            foreach (int df in PawnFileDelta)
            {
                int pf = file + df;
                if (pf < 0 || pf >= BoardConstants.Size) continue;
                Square sq = board[pf, pawnRank];
                if (sq.IsOccupied && sq.Piece.Color == attacker && sq.Piece.Type == PieceType.Pawn)
                    return true;
            }
            return false;
        }

        private static bool AttackedByKnight(Square[,] board, int file, int rank, PieceColor attacker)
        {
            for (int i = 0; i < KnightFileDelta.Length; i++)
            {
                int f = file + KnightFileDelta[i];
                int r = rank + KnightRankDelta[i];
                if (f < 0 || f >= BoardConstants.Size || r < 0 || r >= BoardConstants.Size) continue;
                Square sq = board[f, r];
                if (sq.IsOccupied && sq.Piece.Color == attacker && sq.Piece.Type == PieceType.Knight)
                    return true;
            }
            return false;
        }

        private static bool AttackedByStraightSlider(Square[,] board, int file, int rank, PieceColor attacker)
        {
            int[] fDirs = { 1, -1, 0,  0 };
            int[] rDirs = { 0,  0, 1, -1 };
            for (int d = 0; d < 4; d++)
            {
                for (int step = 1; step < BoardConstants.Size; step++)
                {
                    int f = file + fDirs[d] * step;
                    int r = rank + rDirs[d] * step;
                    if (f < 0 || f >= BoardConstants.Size || r < 0 || r >= BoardConstants.Size) break;
                    Square sq = board[f, r];
                    if (!sq.IsOccupied) continue;
                    if (sq.Piece.Color == attacker &&
                        (sq.Piece.Type == PieceType.Rook || sq.Piece.Type == PieceType.Queen))
                        return true;
                    break;
                }
            }
            return false;
        }

        private static bool AttackedByDiagSlider(Square[,] board, int file, int rank, PieceColor attacker)
        {
            int[] fDirs = { 1,  1, -1, -1 };
            int[] rDirs = { 1, -1,  1, -1 };
            for (int d = 0; d < 4; d++)
            {
                for (int step = 1; step < BoardConstants.Size; step++)
                {
                    int f = file + fDirs[d] * step;
                    int r = rank + rDirs[d] * step;
                    if (f < 0 || f >= BoardConstants.Size || r < 0 || r >= BoardConstants.Size) break;
                    Square sq = board[f, r];
                    if (!sq.IsOccupied) continue;
                    if (sq.Piece.Color == attacker &&
                        (sq.Piece.Type == PieceType.Bishop || sq.Piece.Type == PieceType.Queen))
                        return true;
                    break;
                }
            }
            return false;
        }

        private static bool AttackedByKing(Square[,] board, int file, int rank, PieceColor attacker)
        {
            for (int fd = -1; fd <= 1; fd++)
            {
                for (int rd = -1; rd <= 1; rd++)
                {
                    if (fd == 0 && rd == 0) continue;
                    int f = file + fd;
                    int r = rank + rd;
                    if (f < 0 || f >= BoardConstants.Size || r < 0 || r >= BoardConstants.Size) continue;
                    Square sq = board[f, r];
                    if (sq.IsOccupied && sq.Piece.Color == attacker && sq.Piece.Type == PieceType.King)
                        return true;
                }
            }
            return false;
        }
    }
}
