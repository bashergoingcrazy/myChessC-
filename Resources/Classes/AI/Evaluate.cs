using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Media.Media3D;

namespace myChess.Resources.Classes.AI
{
    public class Evaluate
    {
        public static Dictionary<CombinedPiece, int> material_score = new Dictionary<CombinedPiece, int>
        {
            {CombinedPiece.WhitePawn,100 }, {CombinedPiece.BlackPawn, -100 },
            {CombinedPiece.WhiteKnight, 300}, {CombinedPiece.BlackKnight, -300},
            {CombinedPiece.WhiteBishop, 350 },{CombinedPiece.BlackBishop, -350},
            {CombinedPiece.WhiteRook, 500 }, {CombinedPiece.BlackRook, -500},
            {CombinedPiece.WhiteQueen, 1000 }, {CombinedPiece.BlackQueen, -1000},
            {CombinedPiece.WhiteKing, 10000 },{CombinedPiece.BlackKing, -10000},
        };


        private static int[] pawn_score = new int[64]
        {
            90, 90, 90, 90, 90, 90, 90, 90,
            30, 30, 30, 40, 50, 30, 30, 30,
            20, 20, 20, 30, 30, 30, 20, 20,
            10, 10, 10, 20, 20, 10, 10, 10,
            5,  5,  10, 20, 20,  5,  5,  5,
            0,  0,   0,  5,  5,  0,  0,  0,
            0,  0,   0, -10,-10, 0,  0,  0,
            0,  0,   0,   0,  0, 0,  0,  0,
        };


        private static int[] knight_score = new int[64]
        {
             -5,   0,   0,   0,   0,   0,   0,  -5,
             -5,   0,   0,  10,  10,   0,   0,  -5,
             -5,   5,  20,  20,  20,  20,   5,  -5,
             -5,  10,  20,  30,  30,  20,  10,  -5,
             -5,  10,  20,  30,  30,  20,  10,  -5,
             -5,   5,  20,  10,  10,  20,   5,  -5,
             -5,   0,   0,   0,   0,   0,   0,  -5,
             -5, -10,   0,   0,   0,   0, -10,  -5
        };

        private static int[] bishop_score = new int[64]
        {
            -5,   0,   0,   0,   0,   0,   0,  -5,
            -5,   0,   0,  10,  10,   0,   0,  -5,
            -5,   5,  20,  20,  20,  20,   5,  -5,
            -5,  10,  20,  30,  30,  20,  10,  -5,
            -5,  10,  20,  30,  30,  20,  10,  -5,
            -5,   5,  20,  10,  10,  20,   5,  -5,
            -5,   0,   0,   0,   0,   0,   0,  -5,
            -5, -10,   0,   0,   0,   0, -10,  -5
        };

        private static int[] rook_score = new int[64]
        {
            50,  50,  50,  50,  50,  50,  50,  50,
            50,  50,  50,  50,  50,  50,  50,  50,
             0,   0,  10,  20,  20,  10,   0,   0,
             0,   0,  10,  20,  20,  10,   0,   0,
             0,   0,  10,  20,  20,  10,   0,   0,
             0,   0,  10,  20,  20,  10,   0,   0,
             0,   0,  10,  20,  20,  10,   0,   0,
             0,   0,   0,  20,  20,   0,   0,   0
        };

        private static int[] king_score = new int[64]
        {
             0,   0,   0,   0,   0,   0,   0,   0,
             0,   0,   5,   5,   5,   5,   0,   0,
             0,   5,   5,  10,  10,   5,   5,   0,
             0,   5,  10,  20,  20,  10,   5,   0,
             0,   5,  10,  20,  20,  10,   5,   0,
             0,   0,   5,  10,  10,   5,   0,   0,
             0,   5,   5,  -5,  -5,   0,   5,   0,
             0,   0,   5,   0, -15,   0,  10,   0
        };


        private static int[] mirror_score = new int[64]
        {
            (int)Square.a1, (int)Square.b1, (int)Square.c1, (int)Square.d1, (int)Square.e1, (int)Square.f1, (int)Square.g1, (int)Square.h1,
            (int)Square.a2, (int)Square.b2, (int)Square.c2, (int)Square.d2, (int)Square.e2, (int)Square.f2, (int)Square.g2, (int)Square.h2,
            (int)Square.a3, (int)Square.b3, (int)Square.c3, (int)Square.d3, (int)Square.e3, (int)Square.f3, (int)Square.g3, (int)Square.h3,
            (int)Square.a4, (int)Square.b4, (int)Square.c4, (int)Square.d4, (int)Square.e4, (int)Square.f4, (int)Square.g4, (int)Square.h4,
            (int)Square.a5, (int)Square.b5, (int)Square.c5, (int)Square.d5, (int)Square.e5, (int)Square.f5, (int)Square.g5, (int)Square.h5,
            (int)Square.a6, (int)Square.b6, (int)Square.c6, (int)Square.d6, (int)Square.e6, (int)Square.f6, (int)Square.g6, (int)Square.h6,
            (int)Square.a7, (int)Square.b7, (int)Square.c7, (int)Square.d7, (int)Square.e7, (int)Square.f7, (int)Square.g7, (int)Square.h7,
            (int)Square.a8, (int)Square.b8, (int)Square.c8, (int)Square.d8, (int)Square.e8, (int)Square.f8, (int)Square.g8, (int)Square.h8
        };






        //evaluate position 
        public static int eval(BitGameState GameState)
        {
            int score = 0;

            //current piece bitboard copy 
            ulong bitboard = 0;

            //init square 
            int square;

            foreach (CombinedPiece piece in Enum.GetValues(typeof(CombinedPiece)))
            {
                if (piece == CombinedPiece.None) continue;
                bitboard = GameState.PieceList[(int)piece];

                //loop over piece within the bit board
                while (bitboard > 0)
                {
                    square = BitBoard.get_lsb_index(bitboard);

                    score += material_score[piece];

                    switch (piece)
                    {
                        //evaluate white pieces (except queen)
                        case CombinedPiece.WhitePawn: score += pawn_score[square]; break;
                        case CombinedPiece.WhiteKnight: score += knight_score[square]; break;
                        case CombinedPiece.WhiteBishop: score += bishop_score[square]; break;
                        case CombinedPiece.WhiteRook: score += rook_score[square]; break;
                        case CombinedPiece.WhiteKing: score += king_score[square]; break;

                        //evaluate black piece (except queen)
                        case CombinedPiece.BlackPawn: score -= pawn_score[mirror_score[square]]; break;
                        case CombinedPiece.BlackKnight: score -= knight_score[mirror_score[square]]; break;
                        case CombinedPiece.BlackBishop: score -= bishop_score[mirror_score[square]]; break;
                        case CombinedPiece.BlackRook: score -= rook_score[mirror_score[square]]; break;
                        case CombinedPiece.BlackKing: score -= king_score[mirror_score[square]]; break;
                    }

                    //pop ls1b
                    BitBoard.pop_bit(ref bitboard, square);
                }


            }

            //return final evaluation based on side 
            return (GameState.SideToMove == Side.White)? score: -score;
            
        }

    }
}
