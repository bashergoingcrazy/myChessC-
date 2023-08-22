using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace myChess.Resources.Classes
{
    public enum Castle
    {
        wk = 1, wq = 2,
        bk = 4, bq = 8,
    }
    public class BitGameState
    {
        public Dictionary<int, ulong> PieceList = new Dictionary<int, ulong>();
        public Dictionary<int, ulong> Occupancies = new Dictionary<int, ulong>();
        public Dictionary<int, Char> Ascii_Pieces = new Dictionary<int, Char>();
        public int SideToMove = (int)Side.White;
        public int Enpassant = (int)Square.no_sq;
        public int CastlingRights = 0;



        public BitGameState()
        {
            init_Ascii_Pieces();
            //test();
        }

        private void test()
        {
            //set white pawns
            parse_fen("rnbqkb1r/pp1p1pPp/8/2p1pP2/1P1P4/3P3P/P1P1P3/RNBQKBNR w Kk e6 0 1");

            print_board();
            foreach(Side side in Enum.GetValues(typeof(Side))){
                Debug.WriteLine(side);
                BitBoard.print_bitboard(Occupancies[(int)side]);
            }
        }


        private void Set_Bit(CombinedPiece piece, int square)
        {
            ulong pieceValue = PieceList[(int)piece];
            BitBoard.set_bit(ref pieceValue, square);
            PieceList[(int)piece] = pieceValue;
        }

        private void Set_Bit(int piece, int square)
        {
            Color col = PieceType.GetColor(piece);
            if (col == Color.White)
            {
                ulong pv = Occupancies[(int)Side.White];
                BitBoard.set_bit(ref pv, square);
                Occupancies[(int)Side.White] = pv;
            }
            else
            {
                ulong pv = Occupancies[(int)Side.Black];
                BitBoard.set_bit(ref pv, square);
                Occupancies[(int)Side.Black] = pv;
            }

            ulong pieceValue = PieceList[piece];
            BitBoard.set_bit(ref pieceValue, square);
            PieceList[piece] = pieceValue;
        }

        private void Pop_Bit(CombinedPiece piece, int square) 
        {
            ulong pieceValue = PieceList[(int)piece];
            BitBoard.pop_bit(ref pieceValue, square);
            PieceList[(int)piece] = pieceValue;
        }

        private void InitializeGameState()
        {
            init_PieceList();
            init_Occupancies();
            SideToMove = (int)Side.White;
            Enpassant = (int)Square.no_sq;
            CastlingRights = 0;
        }


        private void init_PieceList()
        {
            foreach (CombinedPiece piece in Enum.GetValues(typeof(CombinedPiece)))
            {
                PieceList[(int)piece] = 0UL; // Initialize each piece's bitboard to 0
            }
        }

        private void init_Occupancies()
        {
            foreach(Side side in Enum.GetValues(typeof(Side)))
            {
                Occupancies[(int)side] = 0Ul;
            }
        }


        private void init_Ascii_Pieces()
        {
            Ascii_Pieces[(int)CombinedPiece.WhitePawn] = 'P';
            Ascii_Pieces[(int)CombinedPiece.WhiteRook] = 'R';
            Ascii_Pieces[(int)CombinedPiece.WhiteBishop] = 'B';
            Ascii_Pieces[(int)CombinedPiece.WhiteKnight] = 'N';
            Ascii_Pieces[(int)CombinedPiece.WhiteKing] = 'K';
            Ascii_Pieces[(int)CombinedPiece.WhiteQueen] = 'Q';

            Ascii_Pieces[(int)CombinedPiece.BlackPawn] = 'p';
            Ascii_Pieces[(int)CombinedPiece.BlackRook] = 'r';
            Ascii_Pieces[(int)CombinedPiece.BlackBishop] = 'b';
            Ascii_Pieces[(int)CombinedPiece.BlackKnight] = 'n';
            Ascii_Pieces[(int)CombinedPiece.BlackKing] = 'k';
            Ascii_Pieces[(int)CombinedPiece.BlackQueen] = 'q';
        }

        public void print_board()
        {
            //loop over the board ranks and file 
            
            for (int rank = 0; rank < 8; rank++)
            {
                Debug.Write(8 - rank + "  ");
                for (int file = 0; file < 8; file++)
                {
                    // initalize the square
                    int square = 8 * rank + file;

                    //define piece variable 
                    int piece = -1;

                    foreach (CombinedPiece pie in Enum.GetValues(typeof(CombinedPiece)))
                    {
                        // Initialize each piece's bitboard to 0

                        if ((BitBoard.get_bit(PieceList[(int)pie], square)) != 0)
                        {
                            piece = (int)pie;
                        }

                    }



                    Debug.Write((piece == -1) ? ". " : Ascii_Pieces[piece] + " ");
                }
                //printnewline everyrank
                Debug.WriteLine("");
            }
            Debug.WriteLine("\n   a b c d e f g h");

            Debug.Write("    Side :");
            Debug.WriteLine((SideToMove == (int)Side.White) ? "White" : "Black");
            Debug.Write("    Enpass :");
            Debug.WriteLine((Enpassant != (int)Square.no_sq) ? BitBoard.square_to_coordinates(Enpassant) : "No");
            Debug.Write("    Castle :");
            Debug.Write(
                 ((CastlingRights & (int)Castle.wk) != 0) ? "K" : "-");
            Debug.Write(
                ((CastlingRights & (int)Castle.wq) != 0) ? "Q" : "-");
            Debug.Write(
                ((CastlingRights & (int)Castle.bk) != 0) ? "k" : "-");
            Debug.WriteLine(
                ((CastlingRights & (int)Castle.bq) != 0) ? "q" : "-");


            Debug.Write("\n\n");
        }

        private void ClearSquare(int square)
        {
            foreach(CombinedPiece piece in Enum.GetValues(typeof(CombinedPiece)))
            {
                Pop_Bit(piece, square);
            }
        }


        public void parse_fen(string fen)
        {
            InitializeGameState();
            Debug.WriteLine(fen);

            string[] fenParts = fen.Split(' ');

            string boardPart = fenParts[0];
            int rank = 0, file = 0;
           
            foreach (char fenChar in boardPart)
            {
                
                if(fenChar == '/')
                {
                    file=0;
                    rank++;
                    continue;
                }
                if (char.IsDigit(fenChar))
                {
                    int emptySquares = int.Parse(fenChar.ToString());
                    file += emptySquares;
                    continue;
                }
                else
                {
                    int square = rank * 8 + file;
                    int key = Ascii_Pieces.FirstOrDefault(x => x.Value == fenChar).Key;
                    Set_Bit(key, square);
                }

                file++;
            }



            // Parse other FEN parts (side to move, castling rights, en passant square, etc.)
            // fenParts[1] contains side to move ('w' for White, 'b' for Black)
       
            foreach(char fenChar in fenParts[1])
            {
             
                if (fenChar == 'w')
                {
                    SideToMove = (int)Side.White; break;
                }
                else
                {
                    SideToMove= (int)Side.Black; break;
                }
                
            }


            // fenParts[2] contains castling rights
            if (fenParts[2] != "-")
            {
                
                foreach(char fenChar in fenParts[2])
                {
                    switch (fenChar)
                    {
                        case 'K':
                            CastlingRights |= (int)Castle.wk;
                            break;
                        case 'Q':
                            CastlingRights |= (int)Castle.wq;
                            break;
                        case 'k':
                            CastlingRights |= (int)Castle.bk;
                            break;
                        case 'q':
                            CastlingRights |= (int)Castle.bq;
                            break;
                        case '-':
                            break;
                    }
                }
            }

            // fenParts[3] contains en passant square or '-'

            if (fenParts[3] != "-")
            {
                string input = fenParts[3];
                Square squareValue = (Square)Enum.Parse(typeof(Square), input);

                Enpassant = (int)squareValue;  
            }
            else
            {
                Enpassant = (int)Square.no_sq;
            }
            // ... parse the remaining FEN parts and update the game state

            Occupancies[(int)Side.Both] = Occupancies[(int)Side.White] | Occupancies[(int)Side.Black];
        }

    }
}
