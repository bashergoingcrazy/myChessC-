using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace myChess.Resources.Classes
{
    public class GameState
    {
        public List<int> gameState;
        public string StartingFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR";

        public void LoadStartingState()
        {
            gameState = new List<int>();

            foreach (char fenChar in StartingFen)
            {
                if (char.IsDigit(fenChar))
                {
                    int emptySquareCount = int.Parse(fenChar.ToString());
                    for (int i = 0; i < emptySquareCount; i++)
                    {
                        gameState.Add(PieceType.CreatePiece(Piece.Empty, Color.NoCol));
                    }
                }
                else
                {
                    switch (fenChar)
                    {
                        case 'r':
                            gameState.Add(PieceType.CreatePiece(Piece.Rook, Color.Black)); break;
                        case 'n':
                            gameState.Add(PieceType.CreatePiece(Piece.Knight, Color.Black)); break;
                        case 'b':
                            gameState.Add(PieceType.CreatePiece(Piece.Bishop, Color.Black)); break;
                        case 'q':
                            gameState.Add(PieceType.CreatePiece(Piece.Queen, Color.Black));
                            break;
                        case 'k':
                            gameState.Add(PieceType.CreatePiece(Piece.King, Color.Black));
                            break;
                        case 'p':
                            gameState.Add(PieceType.CreatePiece(Piece.Pawn, Color.Black));
                            break;
                        case 'R':
                            gameState.Add(PieceType.CreatePiece(Piece.Rook, Color.White));
                            break;
                        case 'N':
                            gameState.Add(PieceType.CreatePiece(Piece.Knight, Color.White));
                            break;
                        case 'B':
                            gameState.Add(PieceType.CreatePiece(Piece.Bishop, Color.White));
                            break;
                        case 'Q':
                            gameState.Add(PieceType.CreatePiece(Piece.Queen, Color.White));
                            break;
                        case 'K':
                            gameState.Add(PieceType.CreatePiece(Piece.King, Color.White));
                            break;
                        case 'P':
                            gameState.Add(PieceType.CreatePiece(Piece.Pawn, Color.White));
                            break;
                        case '/':
                            break;
                        default:
                            throw new ArgumentException("Invalid character in FEN string");
                    }
                }
            }
        }
    }
}
