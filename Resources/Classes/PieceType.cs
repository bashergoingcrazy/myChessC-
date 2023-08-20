using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace myChess.Resources.Classes
{
    public struct Position
    {
        public int X { get; }
        public int Y { get; }

        public Position(int row, int col)
        {
            X = row;
            Y = col;
        }
    }


    public enum Piece
    {
        Empty = 0,
        King = 1,
        Pawn = 2,
        Knight = 3,
        Bishop = 4,
        Rook = 5,
        Queen = 6
    }

    public enum Color
    {
        NoCol = 0,
        White = 0b00001000,
        Black = 0b00010000
    }

    public enum CombinedPiece
    {
        WhiteKing = Piece.King | Color.White,
        WhitePawn = Piece.Pawn | Color.White,
        WhiteKnight = Piece.Knight | Color.White,
        WhiteBishop = Piece.Bishop | Color.White,
        WhiteRook = Piece.Rook | Color.White,
        WhiteQueen = Piece.Queen | Color.White,

        BlackKing = Piece.King | Color.Black,
        BlackPawn = Piece.Pawn | Color.Black,
        BlackKnight = Piece.Knight | Color.Black,
        BlackBishop = Piece.Bishop | Color.Black,
        BlackRook = Piece.Rook | Color.Black,
        BlackQueen = Piece.Queen | Color.Black
    }


    public class PieceType
    {
        public static int CreatePiece(Piece piece, Color color)
        {
            return (int)piece | (int)color;
        }

        public static Piece GetPiece(int combinedValue)
        {
            return (Piece)(combinedValue & 0b00000111);
        }

        public static Color GetColor(int combinedValue)
        {
            return (Color)(combinedValue & 0b00111000);
        }
    }

}
