using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Xml;

namespace myChess.Resources.Classes
{

    public class BitMoveGen
    {
        int SourceSquare,TargetSquare;
        CombinedPiece SourcePiece,TargetPiece;


        BitGameState currState;
        AttackTables AtkTables;

        public BitMoveGen()
        {
            currState = new BitGameState();  //Don't forget to parse_fen before using this Object or elses errors will happen
            AtkTables = new AttackTables();
            test();
        }

        private void test()
        {
            currState.parse_fen("rnbqkbnr/pppppppp/8/8/4q2q/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");
            currState.print_board();
            Debug.Write("Is White King in check ");
            Debug.WriteLine(is_King_in_Check(Side.White));
            SourceSquare = (int)Square.f2;
            Debug.Write("Is this white piece pinned ");
            Debug.WriteLine(is_Pinned(Side.White));
            SourceSquare = (int)Square.e2;
            BitBoard.print_bitboard(currState.Occupancies[(int)Side.Both]);
            Debug.Write("Is this white piece pinned ");
            Debug.WriteLine(is_Pinned(Side.White));

            BitBoard.print_bitboard(currState.Occupancies[(int)Side.Both]);

            //print all attacked squares on the chess board;
        }
        public Transfer GetLegalMoves(int row, int file)
        {
            Transfer result = new Transfer();

            //init squares 
            int SourceSquare = 8 * row + file;
            CombinedPiece piece = GetPieceAt(SourceSquare);
            if (piece == CombinedPiece.None) return result;  //Check if an invalid square is selected by mistake (Shouldn't happen normally)
            
            //Handle if king is in check to be done










            return result;
        }


       







        private CombinedPiece GetPieceAt(int square)
        {
            ulong allPiece = currState.Occupancies[(int)Side.Both];
            foreach(CombinedPiece piece in Enum.GetValues(typeof(CombinedPiece)))
            {
                if (BitBoard.get_bit(currState.PieceList[(int)piece],square)!=0)
                {
                    return piece;
                }
            }

            return CombinedPiece.None;
        }






        private int is_Pinned(Side side)
        {
            int result;
            ulong allPiece = currState.Occupancies[(int)Side.Both];
            BitBoard.pop_bit(ref allPiece, SourceSquare);
            currState.Occupancies[(int)Side.Both] = allPiece;
            result = is_King_in_Check(side);
            BitBoard.set_bit(ref allPiece, SourceSquare);
            currState.Occupancies[(int)Side.Both] = allPiece;

            return result;
        }

        private int is_King_in_Check(Side side)
        {
            if (side == Side.White)
            {
                ulong currentKingState = currState.PieceList[(int)CombinedPiece.WhiteKing];
                int square = BitBoard.get_lsb_index(currentKingState);
                return is_square_attacked(square, (int)Side.Black);
            }
            else
            {
                ulong currentKingState = currState.PieceList[(int)CombinedPiece.BlackKing];
                int square = BitBoard.get_lsb_index(currentKingState);
                return is_square_attacked(square, (int)Side.White);
            }
        }

        private int is_square_attacked(int square, int side)
        {
            //Is attacked by white pawns 
            if ((side == (int)Side.White) && ((AtkTables.pawn_attacks[(int)Side.Black, square] & currState.PieceList[(int)CombinedPiece.WhitePawn]) != 0)) return 1;

            //Is attacked by black pawns 
            if ((side == (int)Side.Black) && ((AtkTables.pawn_attacks[(int)Side.White, square] & currState.PieceList[(int)CombinedPiece.BlackPawn]) != 0)) return 1;

            if(side == (int)Side.White)
            {
                if ((AtkTables.knight_attacks[square] & currState.PieceList[(int)CombinedPiece.WhiteKnight]) != 0) return 1;
                if ((AtkTables.king_attacks[square] & currState.PieceList[(int)CombinedPiece.WhiteKing]) != 0) return 1;
                if ((AtkTables.get_rook_attacks(square, currState.Occupancies[(int)Side.Both]) & currState.PieceList[(int)CombinedPiece.WhiteRook]) != 0) return 1;
                if ((AtkTables.get_bishop_attacks(square, currState.Occupancies[(int)Side.Both]) & currState.PieceList[(int)CombinedPiece.WhiteBishop]) != 0) return 1;
                if ((AtkTables.get_queen_attacks(square, currState.Occupancies[(int)Side.Both]) & currState.PieceList[(int)CombinedPiece.WhiteQueen]) != 0) return 1;
            }
            else
            {
                if ((AtkTables.knight_attacks[square] & currState.PieceList[(int)CombinedPiece.BlackKnight]) != 0) return 1;
                if ((AtkTables.king_attacks[square] & currState.PieceList[(int)CombinedPiece.BlackKing]) != 0) return 1;
                if ((AtkTables.get_rook_attacks(square, currState.Occupancies[(int)Side.Both]) & currState.PieceList[(int)CombinedPiece.BlackRook]) != 0) return 1;
                if ((AtkTables.get_bishop_attacks(square, currState.Occupancies[(int)Side.Both]) & currState.PieceList[(int)CombinedPiece.BlackBishop]) != 0) return 1;
                if ((AtkTables.get_queen_attacks(square, currState.Occupancies[(int)Side.Both]) & currState.PieceList[(int)CombinedPiece.BlackQueen]) != 0) return 1;

            }

            return 0;
        }

        private void print_attacked_squares(int side)
        {
            for (int rank = 0; rank < 8; rank++)
            {
                Debug.Write((8 - rank)+ "   ");
                for (int file = 0; file < 8; file++)
                {
                    int square = 8 * rank + file;

                    //check whether current squared is attacked or not 
                    Debug.Write((is_square_attacked(square, side) == 1) ? 1+" " : 0+ " ");
                }
                Debug.WriteLine("");
            }
            Debug.WriteLine("\n    a b c d e f g h ");
        }
    }
}
