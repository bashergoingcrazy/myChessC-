using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Windows.Documents;
using System.Xml;

namespace myChess.Resources.Classes
{

    public class BitMoveGen
    {
        int SourceSquare,TargetSquare;
        CombinedPiece SourcePiece,TargetPiece;

        bool EnpFlag = false;
        BitGameState currState;
        AttackTables AtkTables;

        public BitMoveGen()
        {
            currState = new BitGameState();  //Don't forget to parse_fen before using this Object or elses errors will happen
            AtkTables = new AttackTables();
            //test();
        }

        public async Task<Transfer> GetLegalMovesAsync(int row, int file)
        {
            return await Task.Run(() => GetLegalMoves(row, file));
        }

        public async Task UpdateGameAsync(int targetSquare, int flag)
        {
            await Task.Run(() => UpdateGame(targetSquare, flag));
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
            SourceSquare = 8 * row + file;
            SourcePiece = currState.GetPieceAt(SourceSquare);
            if (SourcePiece == CombinedPiece.None) return result;  //Check if an invalid square is selected by mistake (Shouldn't happen normally)

            Piece pieceType = PieceType.GetPiece((int)SourcePiece);

            //Handle if king is in check to be done

            switch (pieceType)
            {
                case Piece.Pawn:
                    handle_pawn(ref result, SourcePiece);
                    break;
                case Piece.Bishop:
                    break;
                case Piece.Queen:
                    break;
                case Piece.Knight:
                    break;
                case Piece.King:
                    break;
                case Piece.Rook:
                    break;
                case Piece.Empty:
                    break;
            }

          

            return result;
        }


       
        private void handle_pawn(ref Transfer result,CombinedPiece piece)
        {
            Color pieceColor = PieceType.GetColor((int)piece);
            ulong allPiece = currState.Occupancies[(int)Side.Both];

            //Handle for pins 

            if ( pieceColor == Color.White) 
            {
                //Handle for Promotion
                if( SourceSquare>=(int)Square.a7 && SourceSquare<= (int)Square.h7)
                {
                    if ((BitBoard.get_bit(allPiece, SourceSquare - 8)==0))
                    {
                        int validSquare = SourceSquare - 8;
                        int row = validSquare / 8;
                        int col = validSquare % 8;
                        result.PromotionSquares.Add(new Position(row, col));
                    }
                    ulong attacktt = AtkTables.pawn_attacks[(int)Side.White, SourceSquare];
                    attacktt &= currState.Occupancies[(int)Side.Black];
                    while (attacktt > 0)
                    {
                        int square = BitBoard.get_lsb_index(attacktt);
                        int row = square / 8;
                        int col = square % 8;
                        result.PromotionSquares.Add(new Position(row, col));
                        BitBoard.pop_bit(ref attacktt, square);
                    }
                    return;
                }

                //Hanle for single pawn push
                
                if((BitBoard.get_bit(allPiece, SourceSquare - 8)) == 0 && ((int)Square.h7 < SourceSquare))
                {
                    int validSquare = SourceSquare - 8;
                    int row = validSquare / 8;
                    int column = validSquare % 8;
                    result.NormalSquares.Add(new Position(row, column));
                    Debug.Write("The State of enp flag: ");
                    Debug.WriteLine(EnpFlag);
                    if (EnpFlag)
                    {
                        ulong enpSquare = (1Ul << currState.Enpassant);
                        ulong att = AtkTables.pawn_attacks[(int)Side.White, SourceSquare];
                        if((enpSquare & att) != 0)
                        {
                            int ro = currState.Enpassant / 8;
                            int co = currState.Enpassant % 8;
                            result.EnpSquares.Add(new Position(ro, co));
                        }

                    }
                    
                }

                //Handle for Double pawn push
                if(BitBoard.get_bit(allPiece,SourceSquare-16)==0 && ((int)Square.a2)<=SourceSquare && ((int)Square.h2) >= SourceSquare)
                {
                    int validSquare = SourceSquare - 16;
                    int row = validSquare / 8;
                    int column = validSquare % 8;
                    result.DoublePawnSquares.Add(new Position(row, column));
                }

                //Handle for Captures
                ulong attack = AtkTables.pawn_attacks[(int)Side.White, SourceSquare];
                attack &= currState.Occupancies[(int)Side.Black];
                while (attack > 0)
                {
                    int square = BitBoard.get_lsb_index(attack);
                    int roww = square / 8;
                    int col = square % 8;
                    result.NormalSquares.Add(new Position(roww, col));
                    BitBoard.pop_bit(ref attack, square);
                }

            }
            else
            {
                //Handle for Promotion
                if (SourceSquare >= (int)Square.a2 && SourceSquare <= (int)Square.h2)
                {
                    if ((BitBoard.get_bit(allPiece, SourceSquare + 8) == 0))
                    {
                        int validSquare = SourceSquare + 8;
                        int row = validSquare / 8;
                        int col = validSquare % 8;
                        result.PromotionSquares.Add(new Position(row, col));
                        
                    }
                    ulong attacktt = AtkTables.pawn_attacks[(int)Side.Black, SourceSquare];
                    attacktt &= currState.Occupancies[(int)Side.White];
                    while (attacktt > 0)
                    {
                        int square = BitBoard.get_lsb_index(attacktt);
                        int row = square / 8;
                        int col = square % 8;
                        result.DoublePawnSquares.Add(new Position(row, col));
                        BitBoard.pop_bit(ref attacktt, square);
                    }
                    return;
                }



                //Hanle for single pawn push
                if ((BitBoard.get_bit(allPiece, SourceSquare + 8)) == 0 && ((int)Square.a2 > SourceSquare))
                {
                    int validSquare = SourceSquare + 8;
                    int row = validSquare / 8;
                    int column = validSquare % 8;
                    result.NormalSquares.Add(new Position(row, column));
                    Debug.Write("The State of enp flag: ");
                    Debug.WriteLine(EnpFlag);

                    if (EnpFlag)
                    {
                        ulong enpSquare = (1Ul << currState.Enpassant);
                        ulong att = AtkTables.pawn_attacks[(int)Side.Black, SourceSquare];
                        if ((enpSquare & att) != 0)
                        {
                            int ro = currState.Enpassant / 8;
                            int co = currState.Enpassant % 8;
                            result.EnpSquares.Add(new Position(ro, co));
                        }

                    }
                }

                //Handle for Double pawn push
                if (BitBoard.get_bit(allPiece, SourceSquare + 16) == 0 && ((int)Square.a7) <= SourceSquare && ((int)Square.h7) >= SourceSquare)
                {
                    int validSquare = SourceSquare + 16;
                    int row = validSquare / 8;
                    int column = validSquare % 8;
                    result.DoublePawnSquares.Add(new Position(row, column));
                }
                //Handle for Captures

                ulong attack = AtkTables.pawn_attacks[(int)Side.Black, SourceSquare];
                attack &= currState.Occupancies[(int)Side.White];
                while (attack > 0)
                {
                    int square = BitBoard.get_lsb_index(attack);
                    int row = square / 8;
                    int col = square % 8;
                    result.NormalSquares.Add(new Position(row, col));
                    BitBoard.pop_bit(ref attack, square);
                }
            }





        }

        

        public void UpdateGame(int targetSquare, int flag)
        {
            if (EnpFlag)
            {
                EnpFlag = false;
            }
            if (flag == 10)
            {
                HandleEnp(targetSquare);
            }

            

            TargetSquare = targetSquare;
            currState.UpdateGameState(SourceSquare, TargetSquare, flag);
            Debug.WriteLine("");
            currState.print_board();
            Debug.WriteLine("White");
            BitBoard.print_bitboard(currState.Occupancies[(int)Side.White]);
            Debug.WriteLine("Black");
            BitBoard.print_bitboard(currState.Occupancies[(int)Side.Black]);
            Debug.WriteLine("Both");
            BitBoard.print_bitboard(currState.Occupancies[(int)Side.Both]);
        }

        public Color PieceColor()
        {
            return PieceType.GetColor((int)SourcePiece);
        }

        private void HandleEnp(int sq)
        {
            EnpFlag = true;
            Color colo = PieceType.GetColor((int)SourcePiece);
            if (colo == Color.White)
            {
                currState.Enpassant = sq + 8;
            }
            else
            {
                currState.Enpassant = sq - 8;
            }

        }



        public void StartNewGame()
        {
            currState.parse_fen(Constants.START_POSITION);
            currState.print_board();
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
