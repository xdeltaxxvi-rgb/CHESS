using UnityEngine;
using Chess.Core.Board;
using Chess.Core.Pieces;

namespace Chess.AI
{
    /// <summary>
    /// Pluggable AI contract. GameManager always talks to this interface,
    /// never to ChessAI directly — swapping implementations requires zero
    /// changes outside this file.
    /// </summary>
    public interface IChessAI
    {
        // Returns null when the player has no legal moves (checkmate / stalemate).
        Move? GetBestMove(Square[,] board, PieceColor color, int depth);
    }

    /// <summary>
    /// Concrete AI. Currently a stub that returns the first legal move found.
    /// Minimax + alpha-beta evaluation added in subsequent issues.
    /// </summary>
    public class ChessAI : IChessAI
    {
        // NOTE: board is the live array. For thread safety the real minimax
        // implementation must clone it before searching. The stub is safe
        // because GameManager disables player input while the AI is running.
        public Move? GetBestMove(Square[,] board, PieceColor color, int depth)
        {
            for (int f = 0; f < BoardConstants.Size; f++)
                for (int r = 0; r < BoardConstants.Size; r++)
                {
                    Square sq = board[f, r];
                    if (!sq.IsOccupied || sq.Piece.Color != color) continue;

                    var legal = MoveValidator.GetLegalMoves(sq.Piece, board);
                    if (legal.Count > 0)
                        return new Move(new Vector2Int(f, r), legal[0]);
                }

            return null;
        }
    }
}
