using Chess.Core.Board;
using Chess.Core.Pieces;

namespace Chess.AI
{
    /// <summary>
    /// Pre-generated Zobrist random numbers for fast, deterministic board hashing.
    /// Used by the transposition table to identify repeated positions.
    ///
    /// Piece index maps directly to (int)PieceType:
    ///   King=0, Queen=1, Rook=2, Bishop=3, Knight=4, Pawn=5
    /// Square index = rank*8 + file.
    /// </summary>
    public static class ZobristTable
    {
        private const int PieceTypes = 6;  // Must match PieceType enum count
        private const int Colors     = 2;
        private const int Squares    = 64;

        // [pieceType, color (0=White,1=Black), square]
        private static readonly ulong[,,] _pieceKeys = new ulong[PieceTypes, Colors, Squares];
        private static readonly ulong     _blackToMoveKey;
        private static readonly ulong[]   _enPassantFileKeys = new ulong[8];

        static ZobristTable()
        {
            // Fixed seed — same hash values every run (deterministic TT across iterative depths).
            var rng = new System.Random(20260508);

            for (int p = 0; p < PieceTypes; p++)
                for (int c = 0; c < Colors; c++)
                    for (int s = 0; s < Squares; s++)
                        _pieceKeys[p, c, s] = NextUlong(rng);

            _blackToMoveKey = NextUlong(rng);

            for (int f = 0; f < 8; f++)
                _enPassantFileKeys[f] = NextUlong(rng);
        }

        private static ulong NextUlong(System.Random rng)
        {
            byte[] buf = new byte[8];
            rng.NextBytes(buf);
            return System.BitConverter.ToUInt64(buf, 0);
        }

        /// <summary>Compute Zobrist hash for the given board state.</summary>
        public static ulong ComputeHash(Square[,] board, PieceColor sideToMove)
        {
            ulong hash = 0;

            for (int f = 0; f < 8; f++)
                for (int r = 0; r < 8; r++)
                {
                    Square sq = board[f, r];

                    if (sq.IsEnPassantTarget)
                        hash ^= _enPassantFileKeys[f];

                    if (!sq.IsOccupied) continue;

                    int p = (int)sq.Piece.Type;
                    int c = sq.Piece.Color == PieceColor.White ? 0 : 1;
                    int s = r * 8 + f;
                    hash ^= _pieceKeys[p, c, s];
                }

            if (sideToMove == PieceColor.Black)
                hash ^= _blackToMoveKey;

            return hash;
        }
    }
}
