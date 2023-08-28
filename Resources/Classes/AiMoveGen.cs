using myChess.Resources.Classes.AI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Printing;
using System.Printing.IndexedProperties;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace myChess.Resources.Classes
{
    public class AiMoveGen
    {
        BitGameState crSt;
        static AttackTables attks; 
        Side side; // remember to set after calling the parse 
        List<int> Moves = new List<int>();
        long nodes=0;

        public AiMoveGen(Side MoveGenSide)
        {
            crSt = new BitGameState();
            attks = new AttackTables();
            side = MoveGenSide;

        }
        public AiMoveGen()
        {
            crSt = new BitGameState();
            attks = new AttackTables();
            side = crSt.SideToMove;

            test();
        }

        private void test()
        {
            crSt.parse_fen(Constants.TRICKY_POSITION);
            side = crSt.SideToMove;
            crSt.print_board();

            int alpha = -50000;
            int beta = 50000;
            int depth = 1;

            int eval = AlphaBetaSearch.Negamax(alpha, beta, depth, crSt);
            Debug.WriteLine("\n\n "+eval+"\n");

            Debug.Write("\n");
            print_move(AlphaBetaSearch.best_move);
            
            Debug.Write("Nodes calculated: " + AlphaBetaSearch.nodes + "\n");
            Debug.Write("\n");

            //nodes = 0;

            //Stopwatch stopwatch = new Stopwatch();
            //stopwatch.Start();
            //perft_test(4,crSt);

            //stopwatch.Stop();
            //Debug.WriteLine("Time taken: " + stopwatch.ElapsedMilliseconds + "ms");
            //Debug.WriteLine("Nodes Spotted: " + nodes);
        }

        // leaf nodes (number of positions reached during testing of the move generator at a given depth)

        //perft driver 
        void perft_driver(int depth, BitGameState gameState, ref long localNodes)
        {
            // recursion escape condition 
            if (depth == 0)
            {
                // increment nodes count (count reached positions)
                localNodes++;
                return;
            }

            // generate moves
            List<int> movesList = generate_moves(gameState);

            // loop over all the generated pseudo-legal moves
            for (int i = 0; i < movesList.Count; i++)
            {
                BitGameState clonedState = (BitGameState)gameState.Clone();

                // make move on the clone 
                if (MakeMove.run(movesList[i], Move_Type.all_moves, clonedState) != 1)
                {
                    continue;
                }

                // call perft driver recursively 
                perft_driver(depth - 1, clonedState, ref localNodes);
            }
        }

        void perft_test(int depth, BitGameState gameState)
        {
            Debug.WriteLine("\n\n   Performance Test \n\n");
            // generate moves 
            List<int> movesList = generate_moves(gameState);
            long totalNodes = 0;
            // loop over all the generated pseudo-legal moves
            for (int i = 0; i < movesList.Count; i++)
            {
                BitGameState clonedState = (BitGameState)gameState.Clone();

                // make move on the clone 
                if (MakeMove.run(movesList[i], Move_Type.all_moves, clonedState) != 1)
                {
                    continue;
                }

                long localNodes = 0; // Local node count for this move

                // call perft driver recursively 
                perft_driver(depth - 1, clonedState, ref localNodes);

                print_move(movesList[i]);
                Debug.Write("   Nodes: " + localNodes + "\n");
                totalNodes += localNodes;
            }

            Debug.WriteLine("\n Depth " + depth);
            Debug.WriteLine("\n Total Nodes encountered " + totalNodes);
        }

        public int alphabetastuff(BitGameState gg)
        {
            crSt = gg;
            //side = crSt.SideToMove;
            //crSt.print_board();

            int alpha = -50000;
            int beta = 50000;
            int depth = 6;

            int eval = AlphaBetaSearch.Negamax(alpha, beta, depth, crSt);
            Debug.WriteLine("\n\n " + eval);

            
            print_move(AlphaBetaSearch.best_move);
            Debug.Write("\n");
            Debug.Write("Nodes calculated: " + AlphaBetaSearch.nodes +"\n");
            return AlphaBetaSearch.best_move;
        }



        public int GenerateRandomMove(BitGameState gg)
        {
            crSt = gg;
            side = crSt.SideToMove;
            //crSt.print_board();
            List<int> RandomMovesList = new List<int>();
            List<int>movesList  = generate_moves(gg);
            for(int i = 0; i < movesList.Count; i++)
            {
                int move = Moves[i];

                BitGameState clonedState = (BitGameState)gg.Clone();

                if(MakeMove.run(move, Move_Type.all_moves, clonedState) == 1)
                {
                    RandomMovesList.Add(move);
                    
                }
                else
                {
                    
                }
            }
            int randomMove;
            if (RandomMovesList.Count > 0)
            {
                int randomIndex = new Random().Next(0, RandomMovesList.Count);

                randomMove = RandomMovesList[randomIndex];
            }
            else
            {
                randomMove = 0;
                
            }
            return randomMove;
        }




        public void print_move_list()
        {
            if (Moves.Count == 0)
            {
                Debug.WriteLine("No Moves in the list dammnn!!!!!");
            }


            Debug.WriteLine("\nMove      Piece    PromotedPiece  Capture      DoubleP      Enp    Castling    ");
            for(int i = 0; i<Moves.Count; i++)
            {
                Debug.Write(BitBoard.square_to_coordinates(Codec.get_move_source(Moves[i])));
                Debug.Write(BitBoard.square_to_coordinates(Codec.get_move_target(Moves[i])));
                Debug.Write("\t\t" + crSt.Ascii_Pieces[(int)
                    Codec.get_move_piece(Moves[i])]);
                Debug.Write("\t\t  " + crSt.Ascii_Pieces[(int)
                    Codec.get_move_promoted(Moves[i])]);
                Debug.Write("\t\t\t\t"+  Codec.get_move_capture(Moves[i]));
                Debug.Write("\t\t\t  " + Codec.get_move_double(Moves[i]));
                Debug.Write("\t\t\t" + Codec.get_move_enpassant(Moves[i]));
                Debug.Write("\t\t" + Codec.get_move_castle(Moves[i]));
                Debug.Write("\n");
            }
            Debug.WriteLine("\nTotal Number of moves found = " + Moves.Count);
        }

        public void print_move(int move)
        {
            Debug.Write(BitBoard.square_to_coordinates(Codec.get_move_source(move)));
            Debug.Write(BitBoard.square_to_coordinates(Codec.get_move_target(move)));
            Debug.Write(crSt.Ascii_Pieces[(int)Codec.get_move_piece(move)].ToString());
        }

        public static List<int> generate_moves(BitGameState gameState)
        {
            int source_square, target_square;

            List<int> PMoves = new List<int> ();

            ulong bitboard, attacks;

          
            foreach(CombinedPiece piece in Enum.GetValues(typeof(CombinedPiece)))
            {
                if (piece == CombinedPiece.None) continue;

                //init pieec bitboard copy
                bitboard = gameState.PieceList[(int)piece];

                //generate white pawns and white's king castling moves
                if (gameState.SideToMove == Side.White)
                {

                    if(piece == CombinedPiece.WhitePawn)
                    {
                        //loop over white pawns bitboard index 
                        while (bitboard > 0)
                        {
                            //init squares 
                            source_square = BitBoard.get_lsb_index(bitboard);
                            target_square = source_square - 8;


                            //generate quite pawn moves
                            if(!(target_square< 0) && BitBoard.get_bit(gameState.Occupancies[(int)Side.Both], target_square) == 0)
                            {
                                // pawn promotion
                                if (source_square >= (int)Square.a7 && source_square <=(int)Square.h7)
                                {
                                    // add move into the list
                                    PMoves.Add(Codec.encode_move(source_square, target_square, (int)piece, (int)CombinedPiece.WhiteQueen, 0, 0, 0, 0));
                                    PMoves.Add(Codec.encode_move(source_square, target_square, (int)piece, (int)CombinedPiece.WhiteRook, 0, 0, 0, 0));
                                    PMoves.Add(Codec.encode_move(source_square, target_square, (int)piece, (int)CombinedPiece.WhiteBishop, 0, 0, 0, 0));
                                    PMoves.Add(Codec.encode_move(source_square, target_square, (int)piece, (int)CombinedPiece.WhiteKnight, 0, 0, 0, 0));
                                   
                                }
                                else
                                {
                                    //one square ahead pawn moves
                                    
                                    PMoves.Add(Codec.encode_move(source_square, target_square, (int)piece,0, 0, 0, 0, 0));

                                    //two squares ahead pawn moves 
                                    if ((source_square >= (int)Square.a2) && (source_square<=(int)Square.h2) && (BitBoard.get_bit(gameState.Occupancies[(int)Side.Both], target_square - 8) == 0))
                                    {
                                        
                                        PMoves.Add(Codec.encode_move(source_square, target_square-8, (int)piece, 0, 0, 1, 0, 0));
                                    }

                                }


                            }
                            //init pawn attacks bitboard 
                            attacks = attks.pawn_attacks[(int)gameState.SideToMove, source_square] & gameState.Occupancies[(int)Side.Black];

                            //generate pawn captures 
                            while (attacks>0)
                            {
                                // init target square 
                                target_square = BitBoard.get_lsb_index(attacks);

                                if (source_square >= (int)Square.a7 && source_square <= (int)Square.h7)
                                {
                                    // add move into the list
                                  
                                    PMoves.Add(Codec.encode_move(source_square, target_square, (int)piece, (int)CombinedPiece.WhiteQueen, 1, 0, 0, 0));
                                    PMoves.Add(Codec.encode_move(source_square, target_square, (int)piece, (int)CombinedPiece.WhiteRook, 1, 0, 0, 0));
                                    PMoves.Add(Codec.encode_move(source_square, target_square, (int)piece, (int)CombinedPiece.WhiteBishop, 1, 0, 0, 0));
                                    PMoves.Add(Codec.encode_move(source_square, target_square, (int)piece, (int)CombinedPiece.WhiteKnight, 1, 0, 0, 0));

                                }
                                else
                                {
                                    PMoves.Add(Codec.encode_move(source_square, target_square, (int)piece, 0, 1, 0, 0, 0));
                                }
                                //pop lisb
                                BitBoard.pop_bit(ref attacks,target_square);
                            }
                            // generate enpassant captures 
                            if (gameState.Enpassant != (int)Square.no_sq)
                            {
                                //lookup pawn attacks and bitwise AND with enpassant square (bit)              
                                ulong enp_attacks = attks.pawn_attacks[(int)gameState.SideToMove, source_square] & (1Ul << gameState.Enpassant);

                                //make sure enpassant capture available 
                                if (enp_attacks > 0)
                                {
                                    //init enpassant capture target square 
                                    int target_enpassant = BitBoard.get_lsb_index(enp_attacks);

                                    PMoves.Add(Codec.encode_move(source_square, target_enpassant, (int)piece, 0, 1, 0, 1, 0));
                                   
                                }
                            }

                            //pop the bit 
                            BitBoard.pop_bit(ref bitboard, source_square);
                        }

                    }

                    //Castling moves
                    if(piece == CombinedPiece.WhiteKing)
                    {
                        //king side castling is available 
                        if((gameState.CastlingRights & (int)Castle.wk) != 0)
                        {
                            //make sure the squares between king and corresponding rook are emty
                            if ((BitBoard.get_bit(gameState.Occupancies[(int)Side.Both], (int)Square.f1))==0 && (BitBoard.get_bit(gameState.Occupancies[(int)Side.Both], (int)Square.g1)) == 0)
                            {
                                //make sure the king and the f1 square are not in in check
                                if((MakeMove.is_square_attacked((int)Square.e1,Side.Black,gameState))==0 && (MakeMove.is_square_attacked((int)Square.f1, Side.Black, gameState)) == 0)
                                {
                                   

                                    PMoves.Add(Codec.encode_move((int)Square.e1, (int)Square.g1, (int)piece, 0, 0, 0, 0, 1));
                                }
                            }
                        }

                        //queen side castlin available 
                        //king side castling is available 
                        if ((gameState.CastlingRights & (int)Castle.wq) != 0)
                        {
                            //make sure the squares between king and corresponding rook are emty
                            if ((BitBoard.get_bit(gameState.Occupancies[(int)Side.Both], (int)Square.d1)) == 0 && (BitBoard.get_bit(gameState.Occupancies[(int)Side.Both], (int)Square.c1)) == 0 && (BitBoard.get_bit(gameState.Occupancies[(int)Side.Both], (int)Square.b1)) == 0)
                            {
                                //make sure the king and the f1 square are not in in check
                                if ((MakeMove.is_square_attacked((int)Square.e1, Side.Black, gameState)) == 0 && (MakeMove.is_square_attacked((int)Square.d1, Side.Black, gameState)) == 0)
                                {
                                   

                                    PMoves.Add(Codec.encode_move((int)Square.e1, (int)Square.c1, (int)piece, 0, 0, 0, 0, 1));
                                }
                            }
                        }
                    }

                   
                }

                //generate black pawns and black king castling moves 
                else
                {
                    if (piece == CombinedPiece.BlackPawn)
                    {
                        //loop over white pawns bitboard index 
                        while (bitboard > 0)
                        {
                            //init squares 
                            source_square = BitBoard.get_lsb_index(bitboard);
                            target_square = source_square + 8;


                            //generate quite pawn moves
                            if (!(target_square >63) && BitBoard.get_bit(gameState.Occupancies[(int)Side.Both], target_square) == 0)
                            {
                                // pawn promotion
                                if (source_square >= (int)Square.a2 && source_square <= (int)Square.h2)
                                {
                                   
                                    // add move into the list
                                    PMoves.Add(Codec.encode_move(source_square, target_square, (int)piece, (int)CombinedPiece.BlackQueen, 0, 0, 0, 0));
                                    PMoves.Add(Codec.encode_move(source_square, target_square, (int)piece, (int)CombinedPiece.BlackRook, 0, 0, 0, 0));
                                    PMoves.Add(Codec.encode_move(source_square, target_square, (int)piece, (int)CombinedPiece.BlackBishop, 0, 0, 0, 0));
                                    PMoves.Add(Codec.encode_move(source_square, target_square, (int)piece, (int)CombinedPiece.BlackKnight, 0, 0, 0, 0));

                                }
                                else
                                {
                                    //one square ahead pawn moves
                                   
                                    PMoves.Add(Codec.encode_move(source_square, target_square, (int)piece, 0, 0, 0, 0, 0));

                                    //two squares ahead pawn moves 
                                    if ((source_square >= (int)Square.a7) && (source_square <= (int)Square.h7) && (BitBoard.get_bit(gameState.Occupancies[(int)Side.Both], target_square + 8) == 0))
                                    {
                                        
                                        PMoves.Add(Codec.encode_move(source_square, target_square+8, (int)piece, 0, 0, 1, 0, 0));
                                    }

                                }


                            }
                            //init pawn attacks bitboard 
                            attacks = attks.pawn_attacks[(int)gameState.SideToMove, source_square] & gameState.Occupancies[(int)Side.White];

                            //generate pawn captures 
                            while (attacks > 0)
                            {
                                // init target square 
                                target_square = BitBoard.get_lsb_index(attacks);

                                if (source_square >= (int)Square.a2 && source_square <= (int)Square.h2)
                                {
                                    // add move into the list
                                   
                                    PMoves.Add(Codec.encode_move(source_square, target_square, (int)piece, (int)CombinedPiece.BlackQueen, 1, 0, 0, 0));
                                    PMoves.Add(Codec.encode_move(source_square, target_square, (int)piece, (int)CombinedPiece.BlackRook, 1, 0, 0, 0));
                                    PMoves.Add(Codec.encode_move(source_square, target_square, (int)piece, (int)CombinedPiece.BlackBishop, 1, 0, 0, 0));
                                    PMoves.Add(Codec.encode_move(source_square, target_square, (int)piece, (int)CombinedPiece.BlackKnight, 1, 0, 0, 0));
                                }
                                else
                                {
                                   
                                    PMoves.Add(Codec.encode_move(source_square, target_square, (int)piece, 0, 1, 0, 0, 0));

                                }
                                //pop lisb
                                BitBoard.pop_bit(ref attacks, target_square);
                            }
                            // generate enpassant captures 
                            if (gameState.Enpassant != (int)Square.no_sq)
                            {
                                //lookup pawn attacks and bitwise AND with enpassant square (bit)
                                ulong enp_attacks = attks.pawn_attacks[(int)gameState.SideToMove, source_square] & (1Ul << gameState.Enpassant);
                      

                                //make sure enpassant capture available 
                                if (enp_attacks > 0)
                                {
                                    //init enpassant capture target square 
                                    int target_enpassant = BitBoard.get_lsb_index(enp_attacks);
                                    PMoves.Add(Codec.encode_move(source_square, target_enpassant, (int)piece, 0, 1, 0, 1, 0));
                                    
                                }
                            }

                            //pop the bit 
                            BitBoard.pop_bit(ref bitboard, source_square);
                        }

                    }
                    //Castling moves
                    if (piece == CombinedPiece.BlackKing)
                    {
                        //king side castling is available 
                        if ((gameState.CastlingRights & (int)Castle.bk) != 0)
                        {
                            //make sure the squares between king and corresponding rook are emty
                            if ((BitBoard.get_bit(gameState.Occupancies[(int)Side.Both], (int)Square.f8)) == 0 && (BitBoard.get_bit(gameState.Occupancies[(int)Side.Both], (int)Square.g8)) == 0)
                            {
                                //make sure the king and the f1 square are not in in check
                                if ((MakeMove.is_square_attacked((int)Square.e8, Side.White, gameState)) == 0 && (MakeMove.is_square_attacked((int)Square.f8, Side.White, gameState)) == 0)
                                {
                                   
                                    PMoves.Add(Codec.encode_move((int)Square.e8, (int)Square.g8, (int)piece, 0, 0, 0, 0, 1));
                                }
                            }
                        }

                        //queen side castlin available 
                        //king side castling is available 
                        if ((gameState.CastlingRights & (int)Castle.bq) != 0)
                        {
                            //make sure the squares between king and corresponding rook are emty
                            if ((BitBoard.get_bit(gameState.Occupancies[(int)Side.Both], (int)Square.d8)) == 0 && (BitBoard.get_bit(gameState.Occupancies[(int)Side.Both], (int)Square.c8)) == 0 && (BitBoard.get_bit(gameState.Occupancies[(int)Side.Both], (int)Square.b8)) == 0)
                            {
                                //make sure the king and the f1 square are not in in check
                                if ((MakeMove.is_square_attacked((int)Square.e8, Side.White, gameState)) == 0 && (MakeMove.is_square_attacked((int)Square.d8, Side.White, gameState)) == 0)
                                {
                                   
                                    PMoves.Add(Codec.encode_move((int)Square.e8, (int)Square.c8, (int)piece, 0, 0, 0, 0, 1));
                                }
                            }
                        }
                    }
                }

                //generate knight moves 
                if((gameState.SideToMove==Side.White) ? piece==CombinedPiece.WhiteKnight: piece == CombinedPiece.BlackKnight)
                {
                    while (bitboard > 0)
                    {
                        source_square = BitBoard.get_lsb_index(bitboard);

                        //init piece attacks in order to get set of target squares 
                        attacks = attks.knight_attacks[source_square] & ((gameState.SideToMove == Side.White) ? ~gameState.Occupancies[(int)Side.White] : ~gameState.Occupancies[(int)Side.Black]);



                        //loop over target squares available from the generated attacks
                        while (attacks > 0)
                        {
                            target_square = BitBoard.get_lsb_index(attacks);


                            //quiet moves
                            if (BitBoard.get_bit((gameState.SideToMove == Side.White) ? gameState.Occupancies[(int)Side.Black] : gameState.Occupancies[(int)Side.White] , target_square) == 0)
                            {
                                
                                PMoves.Add(Codec.encode_move(source_square, target_square, (int)piece, 0, 0, 0, 0, 0));
                            }
                            //Capture moves
                            else
                            {
                                
                                PMoves.Add(Codec.encode_move(source_square, target_square, (int)piece, 0, 1, 0, 0, 0));
                            }

                            //escape
                            BitBoard.pop_bit(ref attacks, target_square);
                        }

                        //pop the bit
                        BitBoard.pop_bit(ref bitboard, source_square);
                    }
                }

                //generate bishop moves
                if ((gameState.SideToMove== Side.White) ? piece == CombinedPiece.WhiteBishop : piece == CombinedPiece.BlackBishop)
                {
                    while (bitboard > 0)
                    {
                        source_square = BitBoard.get_lsb_index(bitboard);

                        //init piece attacks in order to get set of target squares 
                        attacks = attks.get_bishop_attacks(source_square, gameState.Occupancies[(int)Side.Both]) & ((gameState.SideToMove== Side.White) ? ~gameState.Occupancies[(int)Side.White] : ~gameState.Occupancies[(int)Side.Black]);



                        //loop over target squares available from the generated attacks
                        while (attacks > 0)
                        {
                            target_square = BitBoard.get_lsb_index(attacks);


                            //quiet moves
                            if (BitBoard.get_bit((gameState.SideToMove == Side.White) ? gameState.Occupancies[(int)Side.Black] : gameState.Occupancies[(int)Side.White], target_square) == 0)
                            {
                                
                                PMoves.Add(Codec.encode_move(source_square, target_square, (int)piece, 0, 0, 0, 0, 0));
                            }
                            //Capture moves
                            else
                            {
                                
                                PMoves.Add(Codec.encode_move(source_square, target_square, (int)piece, 0, 1, 0, 0, 0));
                            }

                            //escape
                            BitBoard.pop_bit(ref attacks, target_square);
                        }

                        //pop the bit
                        BitBoard.pop_bit(ref bitboard, source_square);
                    }
                }

                //generate rook moves
                if ((gameState.SideToMove == Side.White) ? piece == CombinedPiece.WhiteRook : piece == CombinedPiece.BlackRook)
                {
                    while (bitboard > 0)
                    {
                        source_square = BitBoard.get_lsb_index(bitboard);

                        //init piece attacks in order to get set of target squares 
                        attacks = attks.get_rook_attacks(source_square, gameState.Occupancies[(int)Side.Both]) & ((gameState.SideToMove== Side.White) ? ~gameState.Occupancies[(int)Side.White] : ~gameState.Occupancies[(int)Side.Black]);



                        //loop over target squares available from the generated attacks
                        while (attacks > 0)
                        {
                            target_square = BitBoard.get_lsb_index(attacks);


                            //quiet moves
                            if (BitBoard.get_bit((gameState.SideToMove == Side.White) ? gameState.Occupancies[(int)Side.Black] : gameState.Occupancies[(int)Side.White], target_square) == 0)
                            {
                                
                                PMoves.Add(Codec.encode_move(source_square, target_square, (int)piece, 0, 0, 0, 0, 0));
                            }
                            //Capture moves
                            else
                            {
                               
                               PMoves.Add(Codec.encode_move(source_square, target_square, (int)piece, 0, 1, 0, 0, 0));
                            }

                            //escape
                            BitBoard.pop_bit(ref attacks, target_square);
                        }

                        //pop the bit
                        BitBoard.pop_bit(ref bitboard, source_square);
                    }
                }

                //generate queen moves
                if ((gameState.SideToMove == Side.White) ? piece == CombinedPiece.WhiteQueen : piece == CombinedPiece.BlackQueen)
                {
                    while (bitboard > 0)
                    {
                        source_square = BitBoard.get_lsb_index(bitboard);

                        //init piece attacks in order to get set of target squares 
                        attacks = attks.get_queen_attacks(source_square, gameState.Occupancies[(int)Side.Both]) & ((gameState.SideToMove == Side.White) ? ~gameState.Occupancies[(int)Side.White] : ~gameState.Occupancies[(int)Side.Black]);



                        //loop over target squares available from the generated attacks
                        while (attacks > 0)
                        {
                            target_square = BitBoard.get_lsb_index(attacks);


                            //quiet moves
                            if (BitBoard.get_bit((gameState.SideToMove == Side.White) ? gameState.Occupancies[(int)Side.Black] : gameState.Occupancies[(int)Side.White], target_square) == 0)
                            {
                               
                                PMoves.Add(Codec.encode_move(source_square, target_square, (int)piece, 0, 0, 0, 0, 0));
                            }
                            //Capture moves
                            else
                            {
                                
                                PMoves.Add(Codec.encode_move(source_square, target_square, (int)piece, 0, 1, 0, 0, 0));
                            }

                            //escape
                            BitBoard.pop_bit(ref attacks, target_square);
                        }

                        //pop the bit
                        BitBoard.pop_bit(ref bitboard, source_square);
                    }
                }

                //generate king moves 
                if ((gameState.SideToMove == Side.White) ? piece == CombinedPiece.WhiteKing : piece == CombinedPiece.BlackKing)
                {
                    while (bitboard > 0)
                    {
                        source_square = BitBoard.get_lsb_index(bitboard);

                        //init piece attacks in order to get set of target squares 
                        attacks = attks.king_attacks[source_square] & ((gameState.SideToMove == Side.White) ? ~gameState.Occupancies[(int)Side.White] : ~gameState.Occupancies[(int)Side.Black]);



                        //loop over target squares available from the generated attacks
                        while (attacks > 0)
                        {
                            target_square = BitBoard.get_lsb_index(attacks);


                            //quiet moves
                            if (BitBoard.get_bit((gameState.SideToMove == Side.White) ? gameState.Occupancies[(int)Side.Black] : gameState.Occupancies[(int)Side.White], target_square) == 0)
                            {
                                
                                PMoves.Add(Codec.encode_move(source_square, target_square, (int)piece, 0, 0, 0, 0, 0));
                            }
                            //Capture moves
                            else
                            {
                                
                                PMoves.Add(Codec.encode_move(source_square, target_square, (int)piece, 0, 1, 0, 0, 0));
                            }

                            //escape
                            BitBoard.pop_bit(ref attacks, target_square);
                        }

                        //pop the bit
                        BitBoard.pop_bit(ref bitboard, source_square);
                    }
                }

            }
            return PMoves;

        }
       

        
    }
}
