using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Printing;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;
using static System.Formats.Asn1.AsnWriter;

namespace myChess.Resources.Classes.AI
{
    public class AlphaBetaSearch
    {
        public static int best_move;
        public static int nodes;
        private static int ply= 0;


        public static int quiescenece(int alpha, int beta, BitGameState gameState)
        {
            //base case 
            //evaluate position 
            int eval = Evaluate.eval(gameState);

            nodes++;

            if (eval >= beta)
            {
                return beta;
            }

            if (eval > alpha)
            {
                alpha = eval;
            }



            List<int> move_list = AiMoveGen.generate_moves(gameState);

            for (int count = 0; count < move_list.Count; count++)
            {
                BitGameState clonedState = (BitGameState)gameState.Clone();
                if (MakeMove.run(move_list[count], Move_Type.only_captures, clonedState) == 0)
                {
                    continue;
                }

                ply++; // Increment ply here

                int score = -quiescenece(-beta, -alpha, clonedState);

                ply--; // Decrement ply here

                if (score >= beta)
                {
                    return beta;
                }

                if (score > alpha)
                {
                    alpha = score;
                }
            }
            return alpha;

        }

        public static int Negamax(int alpha, int beta, int depth, BitGameState gameState)
        {
            if (depth == 0)
            {
                return quiescenece(alpha, beta,gameState);
            }

            nodes++;

            //is king in check ?
            int in_check = MakeMove.is_square_attacked((gameState.SideToMove == Side.White) ?
                BitBoard.get_lsb_index(gameState.PieceList[(int)CombinedPiece.WhiteKing]) :
                BitBoard.get_lsb_index(gameState.PieceList[(int)CombinedPiece.BlackKing]), 
                (gameState.SideToMove == Side.White) ? Side.Black : Side.White, gameState);


            //legal moves counter 
            int legal_moves = 0;

            int best_sofar = 0;
            int old_alpha = alpha;

            List<int> move_list = AiMoveGen.generate_moves(gameState);

            for (int count = 0; count < move_list.Count; count++)
            {
                BitGameState clonedState = (BitGameState)gameState.Clone();
                if (MakeMove.run(move_list[count], Move_Type.all_moves, clonedState) == 0)
                {
                    continue;
                }

                //Increment legal moves
                legal_moves++;

                ply++; // Increment ply here

                int score = -Negamax(-beta, -alpha, depth - 1, clonedState);

                ply--; // Decrement ply here

                if (score >= beta)
                {
                    return beta;
                }

                if (score > alpha)
                {
                    alpha = score;
                    if (ply == 0)
                    {
                        best_sofar = move_list[count];
                    }
                }
            }

            //we don't have any legal moves to make in the current position 
            if (legal_moves == 0)
            {
                //When king is in check 
                if (in_check == 1)
                {
                    return -49000 + ply;
                }


                //When king is not in check

                else
                {
                    //Return stalemate score 
                    return 0;
                }


            }


            if (old_alpha != alpha)
            {
                best_move = best_sofar;
            }

            return alpha;
        }
    }
}
