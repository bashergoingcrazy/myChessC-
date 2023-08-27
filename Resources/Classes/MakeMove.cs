using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Animation;

namespace myChess.Resources.Classes
{
    public enum Move_Type
    {
        all_moves = 0,
        only_captures = 1,
    }

    public static class MakeMove  
    {
        public static int[] castling_rights = new int[64]
        {
            7,15,15,15,3,15,15,11,
            15,15,15,15,15,15,15,15,
            15,15,15,15,15,15,15,15,
            15,15,15,15,15,15,15,15,
            15,15,15,15,15,15,15,15,
            15,15,15,15,15,15,15,15,
            15,15,15,15,15,15,15,15,
            13,15,15,15,12,15,15,14,
        };
    
        private static AttackTables attks = new AttackTables();



        public static int run(int move, Move_Type move_flag, BitGameState GameState)
        {
            if(move_flag == Move_Type.all_moves)
            {
                int source_square = Codec.get_move_source(move);
                int target_square = Codec.get_move_target(move);
                CombinedPiece piece = Codec.get_move_piece(move);
                CombinedPiece promoted = Codec.get_move_promoted(move);
                int capture = Codec.get_move_capture(move);
                int double_pawn = Codec.get_move_double(move);
                int enpass = Codec.get_move_enpassant(move);
                int castling = Codec.get_move_castle(move);
                Side side = (PieceType.GetColor((int)piece) == Color.White) ? Side.White : Side.Black;
                

                //move quiet moves 
                GameState.popthebit(piece,source_square);
                

                //handle capture moves 
                if (capture == 1)
                {
                    CombinedPiece target_piece = GameState.GetPieceAt(target_square);
                    GameState.popthebit(target_piece, target_square);
                }
                GameState.setthebit(piece, target_square);
                //handle pawn promotions
                if (promoted != CombinedPiece.None)
                {
                    //remove the pawn from the target position
                    GameState.popthebit(piece, target_square);

                    //set up the promoted piece on the chess board
                    GameState.setthebit(promoted, target_square);
                }

                //handle enpassant captures 
                if (enpass == 1)
                {
                    if (side == Side.White)
                    {
                        GameState.popthebit(CombinedPiece.BlackPawn, target_square + 8);
                    }
                    else
                    {
                        GameState.popthebit(CombinedPiece.WhitePawn, target_square - 8);
                    }
                }

                //reset the enpassant square 
                GameState.Enpassant = (int)Square.no_sq;

                //handle the double pawn push
                if (double_pawn == 1)
                {
                    if (side == Side.White)
                    {
                        GameState.Enpassant = target_square + 8;
                    }
                    else
                    {
                        GameState.Enpassant = target_square - 8;
                    }
                }

                //handle the castling moves 
                if (castling == 1)
                {
                    switch (target_square)
                    {
                        //white castles king side
                        case ((int)Square.g1):
                            GameState.popthebit(CombinedPiece.WhiteRook, (int)Square.h1);
                            GameState.setthebit(CombinedPiece.WhiteRook, (int)Square.f1);
                            break;
                        case ((int)Square.c1):
                            GameState.popthebit(CombinedPiece.WhiteRook, (int)Square.a1);
                            GameState.setthebit(CombinedPiece.WhiteRook, (int)Square.d1);
                            break;

                        case ((int)Square.g8):
                            GameState.popthebit(CombinedPiece.BlackRook, (int)Square.h8);
                            GameState.setthebit(CombinedPiece.BlackRook, (int)Square.f8);
                            break;

                        
                        case ((int)Square.c8):
                            GameState.popthebit(CombinedPiece.BlackRook, (int)Square.a8);
                            GameState.setthebit(CombinedPiece.BlackRook, (int)Square.d8);
                            break;                    
                    }              
                }

                //Update castling rights 
                GameState.CastlingRights &= castling_rights[source_square];
                GameState.CastlingRights &= castling_rights[target_square];

                //update Occupancies 
                ulong whitePieceOcc = 0UL;
                ulong blackPieceOcc = 0UL;
                foreach (CombinedPiece pp in Enum.GetValues(typeof(CombinedPiece)))
                {
                    if (pp == CombinedPiece.None) continue;
                    if (Color.White == PieceType.GetColor((int)pp))
                    {
                        whitePieceOcc |= GameState.PieceList[(int)pp];
                    }
                    else
                    {
                        blackPieceOcc |= GameState.PieceList[(int)pp];
                    }
                }
                GameState.Occupancies[(int)Side.White] = whitePieceOcc;
                GameState.Occupancies[(int)Side.Black] = blackPieceOcc;
                GameState.Occupancies[(int)Side.Both] = whitePieceOcc | blackPieceOcc;

                GameState.SideToMove = (GameState.SideToMove == Side.White) ? Side.Black : Side.White;
                int kingSquare;

                if(GameState.SideToMove == Side.White)
                {
                    ulong blackKing = GameState.PieceList[(int)CombinedPiece.BlackKing];
                    kingSquare = BitBoard.get_lsb_index(blackKing);
                }
                else
                {
                    ulong whiteKing = GameState.PieceList[(int)CombinedPiece.WhiteKing];
                    kingSquare = BitBoard.get_lsb_index(whiteKing);
                }


    
                if (is_square_attacked(kingSquare , GameState.SideToMove, GameState) == 1)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            else
            {
                if (Codec.get_move_capture(move) == 1)
                {
                    return run(move, Move_Type.all_moves, GameState);
                }
                else
                {
                    return 0;
                }
            }

        }


        public static int safe_run(int move, Move_Type move_flag, BitGameState GameState)
        {
            if (move_flag == Move_Type.all_moves)
            {
                BitGameState clState = (BitGameState)GameState.Clone();
                int source_square = Codec.get_move_source(move);
                int target_square = Codec.get_move_target(move);
                CombinedPiece piece = Codec.get_move_piece(move);
                CombinedPiece promoted = Codec.get_move_promoted(move);
                int capture = Codec.get_move_capture(move);
                int double_pawn = Codec.get_move_double(move);
                int enpass = Codec.get_move_enpassant(move);
                int castling = Codec.get_move_castle(move);
                Side side = (PieceType.GetColor((int)piece) == Color.White) ? Side.White : Side.Black;


                //move quiet moves 
                clState.popthebit(piece, source_square);


                //handle capture moves 
                if (capture == 1)
                {
                    CombinedPiece target_piece = clState.GetPieceAt(target_square);
                    clState.popthebit(target_piece, target_square);
                }
                clState.setthebit(piece, target_square);
                //handle pawn promotions
                if (promoted != CombinedPiece.None)
                {
                    //remove the pawn from the target position
                    clState.popthebit(piece, target_square);

                    //set up the promoted piece on the chess board
                    clState.setthebit(promoted, target_square);
                }

                //handle enpassant captures 
                if (enpass == 1)
                {
                    if (side == Side.White)
                    {
                        clState.popthebit(CombinedPiece.BlackPawn, target_square + 8);
                    }
                    else
                    {
                        clState.popthebit(CombinedPiece.WhitePawn, target_square - 8);
                    }
                }

                //reset the enpassant square 
                clState.Enpassant = (int)Square.no_sq;

                //handle the double pawn push
                if (double_pawn == 1)
                {
                    if (side == Side.White)
                    {
                        clState.Enpassant = target_square + 8;
                    }
                    else
                    {
                        clState.Enpassant = target_square - 8;
                    }
                }

                //handle the castling moves 
                if (castling == 1)
                {
                    switch (target_square)
                    {
                        //white castles king side
                        case ((int)Square.g1):
                            clState.popthebit(CombinedPiece.WhiteRook, (int)Square.h1);
                            clState.setthebit(CombinedPiece.WhiteRook, (int)Square.f1);
                            break;
                        case ((int)Square.c1):
                            clState.popthebit(CombinedPiece.WhiteRook, (int)Square.a1);
                            clState.setthebit(CombinedPiece.WhiteRook, (int)Square.d1);
                            break;

                        case ((int)Square.g8):
                            clState.popthebit(CombinedPiece.BlackRook, (int)Square.h8);
                            clState.setthebit(CombinedPiece.BlackRook, (int)Square.f8);
                            break;


                        case ((int)Square.c8):
                            clState.popthebit(CombinedPiece.BlackRook, (int)Square.a8);
                            clState.setthebit(CombinedPiece.BlackRook, (int)Square.d8);
                            break;
                    }
                }

                //Update castling rights 
                clState.CastlingRights &= castling_rights[source_square];
                clState.CastlingRights &= castling_rights[target_square];

                //update Occupancies 
                ulong whitePieceOcc = 0UL;
                ulong blackPieceOcc = 0UL;
                foreach (CombinedPiece pp in Enum.GetValues(typeof(CombinedPiece)))
                {
                    if (pp == CombinedPiece.None) continue;
                    if (Color.White == PieceType.GetColor((int)pp))
                    {
                        whitePieceOcc |= clState.PieceList[(int)pp];
                    }
                    else
                    {
                        blackPieceOcc |= clState.PieceList[(int)pp];
                    }
                }
                clState.Occupancies[(int)Side.White] = whitePieceOcc;
                clState.Occupancies[(int)Side.Black] = blackPieceOcc;
                clState.Occupancies[(int)Side.Both] = whitePieceOcc | blackPieceOcc;

                clState.SideToMove = (clState.SideToMove == Side.White) ? Side.Black : Side.White;

                if (is_square_attacked((clState.SideToMove == Side.White) ? BitBoard.get_lsb_index(clState.PieceList[(int)CombinedPiece.BlackKing]) : BitBoard.get_lsb_index(clState.PieceList[(int)CombinedPiece.WhiteKing]), clState.SideToMove, clState) == 1)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            else
            {
                if (Codec.get_move_capture(move) == 1)
                {
                    return run(move, Move_Type.all_moves, GameState);
                }
                else
                {
                    return 0;
                }
            }

        }


        public static int is_square_attacked(int square, Side attackingSide, BitGameState gameState)
        {
            //Is attacked by white pawns 
            //gameState.print_board(); 
            if ((attackingSide == Side.White) && ((attks.pawn_attacks[(int)Side.Black, square] & gameState.PieceList[(int)CombinedPiece.WhitePawn]) != 0)) return 1;

            //Is attacked by black pawns 
            if ((attackingSide == Side.Black) && ((attks.pawn_attacks[(int)Side.White, square] & gameState.PieceList[(int)CombinedPiece.BlackPawn]) != 0)) return 1;

            if (attackingSide == Side.White)
            {
                if ((attks.knight_attacks[square] & gameState.PieceList[(int)CombinedPiece.WhiteKnight]) != 0) return 1;
                if ((attks.king_attacks[square] & gameState.PieceList[(int)CombinedPiece.WhiteKing]) != 0) return 1;
                if ((attks.get_rook_attacks(square, gameState.Occupancies[(int)Side.Both]) & gameState.PieceList[(int)CombinedPiece.WhiteRook]) != 0) return 1;
                if ((attks.get_bishop_attacks(square, gameState.Occupancies[(int)Side.Both]) & gameState.PieceList[(int)CombinedPiece.WhiteBishop]) != 0) return 1;
                if ((attks.get_queen_attacks(square, gameState.Occupancies[(int)Side.Both]) & gameState.PieceList[(int)CombinedPiece.WhiteQueen]) != 0) return 1;
            }
            else
            {
                if ((attks.knight_attacks[square] & gameState.PieceList[(int)CombinedPiece.BlackKnight]) != 0) return 1;
                if ((attks.king_attacks[square] & gameState.PieceList[(int)CombinedPiece.BlackKing]) != 0) return 1;
                if ((attks.get_rook_attacks(square, gameState.Occupancies[(int)Side.Both]) & gameState.PieceList[(int)CombinedPiece.BlackRook]) != 0) return 1;
                if ((attks.get_bishop_attacks(square, gameState.Occupancies[(int)Side.Both]) & gameState.PieceList[(int)CombinedPiece.BlackBishop]) != 0) return 1;
                if ((attks.get_queen_attacks(square, gameState.Occupancies[(int)Side.Both]) & gameState.PieceList[(int)CombinedPiece.BlackQueen]) != 0) return 1;

            }

            return 0;
        }

    }
}
