using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
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
        Color currentColor;
        bool EnpFlag = false;
        BitGameState currState;
        AttackTables AtkTables;
        Color ToMove = Color.White;

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

       

        public object Clone()
        {
            BitMoveGen clone = new BitMoveGen
            {
                SourceSquare = this.SourceSquare,
                TargetSquare = this.TargetSquare,
                SourcePiece = this.SourcePiece,
                TargetPiece = this.TargetPiece,
                currentColor = this.currentColor,
                EnpFlag = this.EnpFlag,
                currState = (BitGameState)this.currState.Clone(),
            };

            return clone;
        }

        private void test()
        {
           
        }

        public Transfer GetLegalMoves(int row, int file,int flag=0)
        {
            Transfer result = new Transfer();

            

            //init squares 
            SourceSquare = 8 * row + file;
            SourcePiece = currState.GetPieceAt(SourceSquare);
            if (SourcePiece == CombinedPiece.None) return result;  //Check if an invalid square is selected by mistake (Shouldn't happen normally)

            Piece pieceType = PieceType.GetPiece((int)SourcePiece);
            if (flag == 0)
            {
                if(ToMove != PieceType.GetColor((int)SourcePiece))
                {
                    return result;
                }
            }

            //Handle if king is in check to be done

            switch (pieceType)
            {
                case Piece.Pawn:
                    handle_pawn(ref result, SourcePiece);
                    break;
                case Piece.Bishop:
                    handle_bishops(ref result, SourcePiece);
                    break;
                case Piece.Queen:
                    handle_queens(ref result, SourcePiece);
                    break;
                case Piece.Knight:
                    handle_knights(ref result, SourcePiece);
                    break;
                case Piece.King:
                    handle_kings(ref result, SourcePiece);
                    break;
                case Piece.Rook:
                    handle_rooks(ref result, SourcePiece);
                    break;
                case Piece.Empty:
                    break;
            }

            //Debug.WriteLine("SP: " + BitBoard.square_to_coordinates(SourceSquare) + " TP: " + BitBoard.square_to_coordinates(TargetSquare));

            return result;
        }


        private void handle_kings(ref Transfer result, CombinedPiece piece)
        {
            //Handle the pin 
            //to be done 

            Debug.WriteLine(currState.CastlingRights);
            Color pieceColor = PieceType.GetColor((int)piece);
            Side toMove = (pieceColor == Color.White) ? Side.White : Side.Black;
            ulong res = AtkTables.king_attacks[SourceSquare];
            ulong occ = currState.Occupancies[(int)Side.Both];
            if (pieceColor == Color.White)
            {
                res &= ~currState.Occupancies[(int)Side.White];
                while (res > 0)
                {
                    int lsb = BitBoard.get_lsb_index(res);
                    if (is_square_attacked(lsb, (int)Side.Black,currState) == 0)
                    {
                        int row = lsb / 8;
                        int col = lsb % 8;
                        // Clone the current BitGameState object
                        BitGameState clonedState = (BitGameState)currState.Clone();

                        // Update the cloned state with the new move
                        clonedState.UpdateGameState(SourceSquare, row * 8 + col, 0);

                        // Check if the king is in check after the move
                        int kingInCheck = is_King_in_Check(toMove, clonedState);

                        if (kingInCheck == 0)
                        {
                            result.NormalSquares.Add(new Position(row, col));
                        }
                    }
                    BitBoard.pop_bit(ref res, lsb);               
                }

                //Handle castling  
                
                if ((currState.CastlingRights & 1) != 0 && is_King_in_Check(toMove,currState)==0)
                {
                    if (BitBoard.get_bit(occ, (int)Square.f1) == 0 && BitBoard.get_bit(occ, (int)Square.g1) == 0 && is_square_attacked((int)Square.f1, (int)Side.Black,currState) == 0 && is_square_attacked((int)Square.g1, (int)Side.Black,currState) == 0)
                    {
                        int square = (int)Square.g1;
                        int row = square / 8;
                        int col = square % 8;
                        result.CastlingSquares.Add(new Position(row, col));
                    }
                }
                if ((currState.CastlingRights & 2) != 0 && is_King_in_Check(toMove, currState) == 0)
                {
                    if (BitBoard.get_bit(occ, (int)Square.d1) == 0 && BitBoard.get_bit(occ, (int)Square.c1) == 0 && is_square_attacked((int)Square.d1, (int)Side.Black, currState) == 0 && is_square_attacked((int)Square.c1, (int)Side.Black, currState) == 0)
                    {
                        int square = (int)Square.c1;
                        int row = square / 8;
                        int col = square % 8;
                        result.CastlingSquares.Add(new Position(row, col));
                    }
                }
            }
            else
            {
                res &= ~currState.Occupancies[(int)Side.Black];
                while (res > 0)
                {
                    int lsb = BitBoard.get_lsb_index(res);
                    if (is_square_attacked(lsb, (int)Side.White,currState) == 0)
                    {
                        int row = lsb / 8;
                        int col = lsb % 8;
                        // Clone the current BitGameState object
                        BitGameState clonedState = (BitGameState)currState.Clone();

                        // Update the cloned state with the new move
                        clonedState.UpdateGameState(SourceSquare, row * 8 + col, 0);

                        // Check if the king is in check after the move
                        int kingInCheck = is_King_in_Check(toMove, clonedState);

                        if (kingInCheck == 0)
                        {
                            result.NormalSquares.Add(new Position(row, col));
                        }
                    }
                    BitBoard.pop_bit(ref res, lsb);
                }
                if ((currState.CastlingRights & 4) != 0 && is_King_in_Check(toMove, currState) == 0)
                {
                    if (BitBoard.get_bit(occ, (int)Square.f8) == 0 && BitBoard.get_bit(occ, (int)Square.g8) == 0 && is_square_attacked((int)Square.f8, (int)Side.White,currState) == 0 && is_square_attacked((int)Square.g8, (int)Side.White, currState) == 0)
                    {
                        int square = (int)Square.g8;
                        int row = square / 8;
                        int col = square % 8;
                        result.CastlingSquares.Add(new Position(row, col));
                    }
                }
                if ((currState.CastlingRights & 8) != 0 && is_King_in_Check(toMove, currState) == 0)
                {
                    if (BitBoard.get_bit(occ, (int)Square.d8) == 0 && BitBoard.get_bit(occ, (int)Square.c8) == 0 && is_square_attacked((int)Square.d8, (int)Side.White,currState) == 0 && is_square_attacked((int)Square.c8, (int)Side.White,currState) == 0)
                    {
                        int square = (int)Square.c8;
                        int row = square / 8;
                        int col = square % 8;
                        result.CastlingSquares.Add(new Position(row, col));
                    }
                }
            }            
        }


        private void handle_queens(ref Transfer result, CombinedPiece piece)
        {
            //Handle the pin 
            //To be done
            Color pieceColor = PieceType.GetColor((int)piece);
            ulong res = AtkTables.get_queen_attacks(SourceSquare, currState.Occupancies[(int)Side.Both]);
            Side toMove = (pieceColor == Color.White) ? Side.White : Side.Black;

            if (pieceColor == Color.White)
            {
                res &= ~currState.Occupancies[(int)Side.White];
            }
            else
            {
                res &= ~currState.Occupancies[(int)Side.Black];
            }
            while (res > 0)
            {
                int lsb = BitBoard.get_lsb_index(res);
                int row = lsb / 8;
                int col = lsb % 8;
                // Clone the current BitGameState object
                BitGameState clonedState = (BitGameState)currState.Clone();

                // Update the cloned state with the new move
                clonedState.UpdateGameState(SourceSquare, row * 8 + col, 0);

                // Check if the king is in check after the move
                int kingInCheck = is_King_in_Check(toMove, clonedState);

                if (kingInCheck == 0)
                {
                    result.NormalSquares.Add(new Position(row, col));
                }

                BitBoard.pop_bit(ref res, lsb);
            }
        }

        private void handle_rooks(ref Transfer result, CombinedPiece piece)
        {
            //Handle the pin 
            //To be done
            Color pieceColor = PieceType.GetColor((int)piece);
            ulong res = AtkTables.get_rook_attacks(SourceSquare, currState.Occupancies[(int)Side.Both]);
            Side toMove = (pieceColor == Color.White) ? Side.White : Side.Black;

            if(pieceColor == Color.White)
            {
                res &= ~currState.Occupancies[(int)Side.White];
            }
            else
            {
                res &= ~currState.Occupancies[(int)Side.Black];
            }
            while (res > 0)
            {
                int lsb = BitBoard.get_lsb_index(res);
                int row = lsb / 8;
                int col = lsb % 8;

                // Clone the current BitGameState object
                BitGameState clonedState = (BitGameState)currState.Clone();

                // Update the cloned state with the new move
                clonedState.UpdateGameState(SourceSquare, row * 8 + col, 0);

                // Check if the king is in check after the move
                int kingInCheck = is_King_in_Check(toMove,clonedState);


                if (kingInCheck==0)
                {
                    result.NormalSquares.Add(new Position(row, col));
                }


                
                BitBoard.pop_bit(ref res, lsb);
            }
        }

        private void handle_bishops(ref Transfer result, CombinedPiece piece)
        {
            //Handle the pin 
            //To be done 
            Color pieceColor = PieceType.GetColor((int)piece);
            ulong res = AtkTables.get_bishop_attacks(SourceSquare, currState.Occupancies[(int)Side.Both]);
            Side toMove = (pieceColor == Color.White) ? Side.White : Side.Black;

            if (pieceColor == Color.White)
            {
                res &= ~currState.Occupancies[(int)Side.White];
            }
            else
            {
                res &= ~currState.Occupancies[(int)Side.Black];
            }
            while (res > 0)
            {
                int lsb = BitBoard.get_lsb_index(res);
                int row = lsb / 8;
                int col = lsb % 8;
                // Clone the current BitGameState object
                BitGameState clonedState = (BitGameState)currState.Clone();

                // Update the cloned state with the new move
                clonedState.UpdateGameState(SourceSquare, row * 8 + col, 0);

                // Check if the king is in check after the move
                int kingInCheck = is_King_in_Check(toMove, clonedState);

                if (kingInCheck == 0)
                {
                    result.NormalSquares.Add(new Position(row, col));
                }
                BitBoard.pop_bit(ref res, lsb);
            }
        }


        private void handle_knights(ref Transfer result, CombinedPiece piece)
        {
            //Handle the pin 
            //To be done
            Color pieceColor = PieceType.GetColor((int)piece);
            ulong res = 0UL;
            Side toMove = (pieceColor == Color.White) ? Side.White : Side.Black;

            if (pieceColor == Color.White)
            {
                res = AtkTables.knight_attacks[SourceSquare] & ~currState.Occupancies[(int)Side.White];
            }
            else
            {
                res = AtkTables.knight_attacks[SourceSquare] & ~currState.Occupancies[(int)Side.Black];
            }
            while (res > 0)
            {
                int lsb = BitBoard.get_lsb_index(res);
                int row = lsb / 8;
                int col = lsb % 8;
                // Clone the current BitGameState object
                BitGameState clonedState = (BitGameState)currState.Clone();

                // Update the cloned state with the new move
                clonedState.UpdateGameState(SourceSquare, row * 8 + col, 0);

                // Check if the king is in check after the move
                int kingInCheck = is_King_in_Check(toMove, clonedState);

                if (kingInCheck == 0)
                {
                    result.NormalSquares.Add(new Position(row, col));
                }
                BitBoard.pop_bit(ref res, lsb);
            }
        }


        private void handle_pawn(ref Transfer result,CombinedPiece piece)
        {
            Color pieceColor = PieceType.GetColor((int)piece);
            ulong allPiece = currState.Occupancies[(int)Side.Both];
            Side toMove = (pieceColor == Color.White) ? Side.White : Side.Black;
     

            //Handle for pins
            //To be done 
            //Debug.Write("Please tell me the source square " + BitBoard.square_to_coordinates(SourceSquare));

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
                        // Clone the current BitGameState object
                        BitGameState clonedState = (BitGameState)currState.Clone();

                        // Update the cloned state with the new move
                        clonedState.UpdateGameState(SourceSquare, row * 8 + col, 1);

                        // Check if the king is in check after the move
                        int kingInCheck = is_King_in_Check(toMove, clonedState);

                        if (kingInCheck == 0)
                        {
                            result.PromotionSquares.Add(new Position(row, col));
                        }
                    }
                    ulong attacktt = AtkTables.pawn_attacks[(int)Side.White, SourceSquare];
                    attacktt &= currState.Occupancies[(int)Side.Black];
                    while (attacktt > 0)
                    {
                        int square = BitBoard.get_lsb_index(attacktt);
                        int row = square / 8;
                        int col = square % 8;
                        // Clone the current BitGameState object
                        BitGameState clonedState = (BitGameState)currState.Clone();

                        // Update the cloned state with the new move
                        clonedState.UpdateGameState(SourceSquare, row * 8 + col, 1);

                        // Check if the king is in check after the move
                        int kingInCheck = is_King_in_Check(toMove, clonedState);

                        if (kingInCheck == 0)
                        {
                            result.PromotionSquares.Add(new Position(row, col));
                        }
                        BitBoard.pop_bit(ref attacktt, square);
                    }
                    return;
                }

                //Hanle for single pawn push
                
                if((BitBoard.get_bit(allPiece, SourceSquare - 8)) == 0 && ((int)Square.h7 < SourceSquare))
                {
                    int validSquare = SourceSquare - 8;
                    int row = validSquare / 8;
                    int col = validSquare % 8;
                    // Clone the current BitGameState object
                    BitGameState clonedState = (BitGameState)currState.Clone();

                    // Update the cloned state with the new move
                    clonedState.UpdateGameState(SourceSquare, row * 8 + col, 0);

                    // Check if the king is in check after the move
                    int kingInCheck = is_King_in_Check(toMove, clonedState);

                    if (kingInCheck == 0)
                    {
                        result.NormalSquares.Add(new Position(row, col));
                    }
                }
                Debug.Write("The State of enp flag: ");
                Debug.WriteLine(EnpFlag);
                if (EnpFlag && currentColor != PieceType.GetColor((int)SourcePiece))
                {
                    ulong enpSquare = (1Ul << currState.Enpassant);
                    ulong att = AtkTables.pawn_attacks[(int)Side.White, SourceSquare];
                    if ((enpSquare & att) != 0)
                    {
                        int ro = currState.Enpassant / 8;
                        int co = currState.Enpassant % 8;
                        result.EnpSquares.Add(new Position(ro, co));
                    }

                }

                //Handle for Double pawn push
                if (BitBoard.get_bit(allPiece,SourceSquare-16)==0 && ((int)Square.a2)<=SourceSquare && ((int)Square.h2) >= SourceSquare)
                {
                    int validSquare = SourceSquare - 16;
                    int row = validSquare / 8;
                    int col = validSquare % 8;
                    // Clone the current BitGameState object
                    BitGameState clonedState = (BitGameState)currState.Clone();

                    // Update the cloned state with the new move
                    clonedState.UpdateGameState(SourceSquare, row * 8 + col, 10);

                    // Check if the king is in check after the move
                    int kingInCheck = is_King_in_Check(toMove, clonedState);

                    if (kingInCheck == 0)
                    {
                        result.DoublePawnSquares.Add(new Position(row, col));
                    }
                }

                //Handle for Captures
                ulong attack = AtkTables.pawn_attacks[(int)Side.White, SourceSquare];
                attack &= currState.Occupancies[(int)Side.Black];
                while (attack > 0)
                {
                    int square = BitBoard.get_lsb_index(attack);
                    int roww = square / 8;
                    int col = square % 8;
                    // Clone the current BitGameState object
                    BitGameState clonedState = (BitGameState)currState.Clone();

                    // Update the cloned state with the new move
                    clonedState.UpdateGameState(SourceSquare, roww * 8 + col, 0);

                    // Check if the king is in check after the move
                    int kingInCheck = is_King_in_Check(toMove, clonedState);

                    if (kingInCheck == 0)
                    {
                        result.NormalSquares.Add(new Position(roww, col));
                    }
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
                        // Clone the current BitGameState object
                        BitGameState clonedState = (BitGameState)currState.Clone();

                        // Update the cloned state with the new move
                        clonedState.UpdateGameState(SourceSquare, row * 8 + col, 1);

                        // Check if the king is in check after the move
                        int kingInCheck = is_King_in_Check(toMove, clonedState);

                        if (kingInCheck == 0)
                        {
                            result.PromotionSquares.Add(new Position(row, col));
                        }
                    }
                    ulong attacktt = AtkTables.pawn_attacks[(int)Side.Black, SourceSquare];
                    attacktt &= currState.Occupancies[(int)Side.White];
                    while (attacktt > 0)
                    {
                        int square = BitBoard.get_lsb_index(attacktt);
                        int row = square / 8;
                        int col = square % 8;
                        // Clone the current BitGameState object
                        BitGameState clonedState = (BitGameState)currState.Clone();

                        // Update the cloned state with the new move
                        clonedState.UpdateGameState(SourceSquare, row * 8 + col, 1);

                        // Check if the king is in check after the move
                        int kingInCheck = is_King_in_Check(toMove, clonedState);

                        if (kingInCheck == 0)
                        {
                            result.PromotionSquares.Add(new Position(row, col));
                        }
                        BitBoard.pop_bit(ref attacktt, square);
                        BitBoard.pop_bit(ref attacktt, square);
                    }
                    return;
                }



                //Hanle for single pawn push
                if ((BitBoard.get_bit(allPiece, SourceSquare + 8)) == 0 && ((int)Square.a2 > SourceSquare))
                {
                    int validSquare = SourceSquare + 8;
                    int row = validSquare / 8;
                    int col = validSquare % 8;
                    // Clone the current BitGameState object
                    BitGameState clonedState = (BitGameState)currState.Clone();

                    // Update the cloned state with the new move
                    clonedState.UpdateGameState(SourceSquare, row * 8 + col, 0);

                    // Check if the king is in check after the move
                    int kingInCheck = is_King_in_Check(toMove, clonedState);

                    if (kingInCheck == 0)
                    {
                        result.NormalSquares.Add(new Position(row, col));
                    }
                }
                Debug.Write("The State of enp flag: ");
                Debug.WriteLine(EnpFlag);

                if (EnpFlag && currentColor != PieceType.GetColor((int)SourcePiece))
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
                //Handle for Double pawn push
                if (BitBoard.get_bit(allPiece, SourceSquare + 16) == 0 && ((int)Square.a7) <= SourceSquare && ((int)Square.h7) >= SourceSquare)
                {
                    int validSquare = SourceSquare + 16;
                    int row = validSquare / 8;
                    int coll = validSquare % 8;
                    // Clone the current BitGameState object
                    BitGameState clonedState = (BitGameState)currState.Clone();

                    // Update the cloned state with the new move
                    clonedState.UpdateGameState(SourceSquare, row * 8 + coll, 10);

                    // Check if the king is in check after the move
                    int kingInCheck = is_King_in_Check(toMove, clonedState);

                    if (kingInCheck == 0)
                    {
                        result.DoublePawnSquares.Add(new Position(row, coll));
                    }
                }
                //Handle for Captures

                ulong attack = AtkTables.pawn_attacks[(int)Side.Black, SourceSquare];
                attack &= currState.Occupancies[(int)Side.White];
                while (attack > 0)
                {
                    int square = BitBoard.get_lsb_index(attack);
                    int row = square / 8;
                    int col = square % 8;
                    // Clone the current BitGameState object
                    BitGameState clonedState = (BitGameState)currState.Clone();

                    // Update the cloned state with the new move
                    clonedState.UpdateGameState(SourceSquare, row * 8 + col, 0);

                    // Check if the king is in check after the move
                    int kingInCheck = is_King_in_Check(toMove, clonedState);

                    if (kingInCheck == 0)
                    {
                        result.NormalSquares.Add(new Position(row, col));
                    }
                    BitBoard.pop_bit(ref attack, square);
                }
            }
        }

        
        public Position CheckGameEnd()
        {
            Color sourceColor = PieceType.GetColor((int)SourcePiece);
            Color opp = PieceType.GetOppositeColor(sourceColor);
            Side side = (opp==Color.White)? Side.White: Side.Black;
            //Handle CheckMate Or Stalemate
            if (isCheckMate(side) == 1 && is_King_in_Check(side,currState)==1)
            {
                return new Position(100, (int)side);
            }
            else if (isCheckMate(side) == 1)
            {
                return new Position(99,(int)side);
            }



            if(is_King_in_Check(side, currState) == 1)
            {
                int king = (int)opp | (int)Piece.King;
                ulong kingState = currState.PieceList[king];
                int square = BitBoard.get_lsb_index(kingState);
                int row = square / 8;
                int col = square % 8;
                return new Position(row, col);
            }


            return new Position(-1,-1);
        }

        private int isCheckMate(Side theside)
        {
            ulong occup = currState.Occupancies[(int)theside];
            //BitBoard.print_bitboard(occup);
            while (occup > 0)
            {
                int square = BitBoard.get_lsb_index(occup);
                int row = square / 8;
                int col = square % 8;
                Transfer checkLegalMoves = GetLegalMoves(row, col,1);
                if (!checkLegalMoves.is_Empty())
                {
                    return 0;
                }
                BitBoard.pop_bit(ref occup, square);
            }

            return 1;
        }

        public void UpdateGame(int sourceSquare, int targetSquare, int flag)
        {
            //Debug.WriteLine(ToMove);
            ToMove = (ToMove == Color.White) ? Color.Black : Color.White;
            if (EnpFlag)
            {
                currState.Enpassant = (int)Square.no_sq;
                EnpFlag = false;
            }
            if (flag == 10)
            {
                HandleEnp(targetSquare);
            }

            HandleCastlingRights(sourceSquare);

            TargetSquare = targetSquare;
            currState.UpdateGameState(sourceSquare, targetSquare, flag);
            //Debug.WriteLine("");
            //currState.print_board();
            //Debug.WriteLine(ToMove);
            //Debug.WriteLine("White");
            //BitBoard.print_bitboard(currState.Occupancies[(int)Side.White]);
            //Debug.WriteLine("Black");
            //BitBoard.print_bitboard(currState.Occupancies[(int)Side.Black]);
            //Debug.WriteLine("Both");
            //BitBoard.print_bitboard(currState.Occupancies[(int)Side.Both]);
            //Debug.WriteLine("SP: " + BitBoard.square_to_coordinates(sourceSquare) + " TP: " + BitBoard.square_to_coordinates(targetSquare));
        }

        public Color PieceColor()
        {
            return PieceType.GetColor((int)SourcePiece);
        }

        private void HandleEnp(int sq)
        {
            EnpFlag = true;
            currentColor = PieceType.GetColor((int)SourcePiece);
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

        private void HandleCastlingRights(int sourceSquare)
        {
            if (sourceSquare == (int)Square.h1)
            {
                currState.CastlingRights &= 0b1110;
                
            }
            if (sourceSquare == (int)Square.a1)
            {
                currState.CastlingRights &= 0b1101;
               
            }
            if (sourceSquare == (int)Square.e1)
            {
                currState.CastlingRights &= 0b1100;
        
            }
            if (sourceSquare == (int)Square.h8)
            {
                currState.CastlingRights &= 0b1011;
         
            }
            if (sourceSquare == (int)Square.a8)
            {
                currState.CastlingRights &= 0b0111;
         
            }
            if (sourceSquare == (int)Square.e8)
            {
                currState.CastlingRights &= 0b0011;
   
            }
        }



        public void StartNewGame()
        {
            currState.parse_fen(Constants.START_POSITION);
            currState.print_board();
        }

        //private int is_Pinned(Side side)
        //{
        //    int result;
        //    ulong allPiece = currState.Occupancies[(int)Side.Both];
        //    BitBoard.pop_bit(ref allPiece, SourceSquare);
        //    currState.Occupancies[(int)Side.Both] = allPiece;
        //    result = is_King_in_Check(side);
        //    BitBoard.set_bit(ref allPiece, SourceSquare);
        //    currState.Occupancies[(int)Side.Both] = allPiece;

        //    return result;
        //}

        private int is_King_in_Check(Side side, BitGameState GameState)
        {
            if (side == Side.White)
            {
                ulong currentKingState = GameState.PieceList[(int)CombinedPiece.WhiteKing];
                int square = BitBoard.get_lsb_index(currentKingState);
                return is_square_attacked(square, (int)Side.Black,GameState);
            }
            else
            {
                ulong currentKingState = GameState.PieceList[(int)CombinedPiece.BlackKing];
                int square = BitBoard.get_lsb_index(currentKingState);
                return is_square_attacked(square, (int)Side.White, GameState);
            }
        }

        private int is_square_attacked(int square, int side, BitGameState gameState)
        {
            //Is attacked by white pawns 
            if ((side == (int)Side.White) && ((AtkTables.pawn_attacks[(int)Side.Black, square] & gameState.PieceList[(int)CombinedPiece.WhitePawn]) != 0)) return 1;

            //Is attacked by black pawns 
            if ((side == (int)Side.Black) && ((AtkTables.pawn_attacks[(int)Side.White, square] & gameState.PieceList[(int)CombinedPiece.BlackPawn]) != 0)) return 1;

            if (side == (int)Side.White)
            {
                if ((AtkTables.knight_attacks[square] & gameState.PieceList[(int)CombinedPiece.WhiteKnight]) != 0) return 1;
                if ((AtkTables.king_attacks[square] & gameState.PieceList[(int)CombinedPiece.WhiteKing]) != 0) return 1;
                if ((AtkTables.get_rook_attacks(square, gameState.Occupancies[(int)Side.Both]) & gameState.PieceList[(int)CombinedPiece.WhiteRook]) != 0) return 1;
                if ((AtkTables.get_bishop_attacks(square, gameState.Occupancies[(int)Side.Both]) & gameState.PieceList[(int)CombinedPiece.WhiteBishop]) != 0) return 1;
                if ((AtkTables.get_queen_attacks(square, gameState.Occupancies[(int)Side.Both]) & gameState.PieceList[(int)CombinedPiece.WhiteQueen]) != 0) return 1;
            }
            else
            {
                if ((AtkTables.knight_attacks[square] & gameState.PieceList[(int)CombinedPiece.BlackKnight]) != 0) return 1;
                if ((AtkTables.king_attacks[square] & gameState.PieceList[(int)CombinedPiece.BlackKing]) != 0) return 1;
                if ((AtkTables.get_rook_attacks(square, gameState.Occupancies[(int)Side.Both]) & gameState.PieceList[(int)CombinedPiece.BlackRook]) != 0) return 1;
                if ((AtkTables.get_bishop_attacks(square, gameState.Occupancies[(int)Side.Both]) & gameState.PieceList[(int)CombinedPiece.BlackBishop]) != 0) return 1;
                if ((AtkTables.get_queen_attacks(square, gameState.Occupancies[(int)Side.Both]) & gameState.PieceList[(int)CombinedPiece.BlackQueen]) != 0) return 1;

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
                    Debug.Write((is_square_attacked(square, side, currState) == 1) ? 1+" " : 0+ " ");
                }
                Debug.WriteLine("");
            }
            Debug.WriteLine("\n    a b c d e f g h ");
        }
    }
}
    