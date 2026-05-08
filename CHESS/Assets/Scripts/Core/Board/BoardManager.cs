using UnityEngine;
using Chess.Core.Pieces;

namespace Chess.Core.Board
{
    public class BoardManager : MonoBehaviour
    {
        private Square[,] _board;
        private Vector2Int? _enPassantTarget;

        private void Awake()
        {
            InitialiseBoard();
            InitialiseStartingPieces();
        }

        private void InitialiseBoard()
        {
            _board = new Square[BoardConstants.Size, BoardConstants.Size];
            for (int file = 0; file < BoardConstants.Size; file++)
                for (int rank = 0; rank < BoardConstants.Size; rank++)
                    _board[file, rank] = new Square(file, rank);
        }

        private void InitialiseStartingPieces()
        {
            // White back rank (rank 0)
            PlacePiece(new Rook(PieceColor.White, new Vector2Int(0, 0)), 0, 0);
            PlacePiece(new Knight(PieceColor.White, new Vector2Int(1, 0)), 1, 0);
            PlacePiece(new Bishop(PieceColor.White, new Vector2Int(2, 0)), 2, 0);
            PlacePiece(new Queen(PieceColor.White, new Vector2Int(3, 0)), 3, 0);
            PlacePiece(new King(PieceColor.White, new Vector2Int(4, 0)), 4, 0);
            PlacePiece(new Bishop(PieceColor.White, new Vector2Int(5, 0)), 5, 0);
            PlacePiece(new Knight(PieceColor.White, new Vector2Int(6, 0)), 6, 0);
            PlacePiece(new Rook(PieceColor.White, new Vector2Int(7, 0)), 7, 0);

            // White pawns (rank 1)
            for (int file = 0; file < BoardConstants.Size; file++)
                PlacePiece(new Pawn(PieceColor.White, new Vector2Int(file, 1)), file, 1);

            // Black pawns (rank 6)
            for (int file = 0; file < BoardConstants.Size; file++)
                PlacePiece(new Pawn(PieceColor.Black, new Vector2Int(file, 6)), file, 6);

            // Black back rank (rank 7)
            PlacePiece(new Rook(PieceColor.Black, new Vector2Int(0, 7)), 0, 7);
            PlacePiece(new Knight(PieceColor.Black, new Vector2Int(1, 7)), 1, 7);
            PlacePiece(new Bishop(PieceColor.Black, new Vector2Int(2, 7)), 2, 7);
            PlacePiece(new Queen(PieceColor.Black, new Vector2Int(3, 7)), 3, 7);
            PlacePiece(new King(PieceColor.Black, new Vector2Int(4, 7)), 4, 7);
            PlacePiece(new Bishop(PieceColor.Black, new Vector2Int(5, 7)), 5, 7);
            PlacePiece(new Knight(PieceColor.Black, new Vector2Int(6, 7)), 6, 7);
            PlacePiece(new Rook(PieceColor.Black, new Vector2Int(7, 7)), 7, 7);
        }

        public void PlacePiece(Piece piece, int file, int rank)
        {
            Debug.Assert(IsValidCoordinate(file, rank), $"({file},{rank}) out of bounds.");
            Square sq = _board[file, rank];
            sq.Piece = piece;
            _board[file, rank] = sq;
        }

        public Square GetSquare(int file, int rank)
        {
            Debug.Assert(IsValidCoordinate(file, rank), $"({file},{rank}) out of bounds.");
            return _board[file, rank];
        }

        public bool IsValidCoordinate(int file, int rank) =>
            file >= 0 && file < BoardConstants.Size &&
            rank >= 0 && rank < BoardConstants.Size;

        public Square[,] GetBoard() => _board;

        // Replaces a pawn that reached the back rank with the chosen piece type.
        public void PromotePawn(Vector2Int position, PieceType promoteTo)
        {
            PieceColor color = _board[position.x, position.y].Piece.Color;
            Piece promoted = CreatePiece(promoteTo, color, position);
            promoted.HasMoved = true;
            PlacePiece(promoted, position.x, position.y);
        }

        private static Piece CreatePiece(PieceType type, PieceColor color, Vector2Int pos) => type switch
        {
            PieceType.Queen  => (Piece)new Queen (color, pos),
            PieceType.Rook   =>        new Rook  (color, pos),
            PieceType.Bishop =>        new Bishop(color, pos),
            PieceType.Knight =>        new Knight(color, pos),
            _ => throw new System.ArgumentException($"Cannot promote to {type}")
        };

        public void ExecuteMove(Vector2Int from, Vector2Int to)
        {
            Piece piece = _board[from.x, from.y].Piece;

            // En passant: detect before clearing the flag, then remove the bypassed pawn.
            bool isEnPassant = piece.Type == PieceType.Pawn && _board[to.x, to.y].IsEnPassantTarget;

            // Clear previous en passant target.
            if (_enPassantTarget.HasValue)
            {
                Square eq = _board[_enPassantTarget.Value.x, _enPassantTarget.Value.y];
                eq.IsEnPassantTarget = false;
                _board[_enPassantTarget.Value.x, _enPassantTarget.Value.y] = eq;
                _enPassantTarget = null;
            }

            if (isEnPassant)
            {
                Square capSq = _board[to.x, from.y];
                capSq.Piece = null;
                _board[to.x, from.y] = capSq;
            }

            Square fromSq = _board[from.x, from.y];
            fromSq.Piece = null;
            _board[from.x, from.y] = fromSq;

            Square toSq = _board[to.x, to.y];
            toSq.Piece = piece;
            _board[to.x, to.y] = toSq;

            piece.Position = to;
            piece.HasMoved = true;

            // Set en passant target for pawn double-step.
            int rankDelta = to.y - from.y;
            if (piece.Type == PieceType.Pawn && (rankDelta == 2 || rankDelta == -2))
            {
                int epRank = (from.y + to.y) / 2;
                _enPassantTarget = new Vector2Int(from.x, epRank);
                Square epSq = _board[from.x, epRank];
                epSq.IsEnPassantTarget = true;
                _board[from.x, epRank] = epSq;
            }

            // Castling: also relocate the Rook in data.
            int fileDelta = to.x - from.x;
            if (piece.Type == PieceType.King && (fileDelta == 2 || fileDelta == -2))
            {
                bool kingside = fileDelta > 0;
                int rookFromFile = kingside ? BoardConstants.Size - 1 : 0;
                int rookToFile = kingside ? to.x - 1 : to.x + 1;
                int rank = from.y;

                Piece rook = _board[rookFromFile, rank].Piece;
                Square rs = _board[rookFromFile, rank];
                rs.Piece = null;
                _board[rookFromFile, rank] = rs;
                Square rd = _board[rookToFile, rank];
                rd.Piece = rook;
                _board[rookToFile, rank] = rd;
                rook.Position = new Vector2Int(rookToFile, rank);
                rook.HasMoved = true;
            }
        }
    }
}
