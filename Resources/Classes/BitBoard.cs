using System.Diagnostics;
using System.Numerics;
namespace myChess.Resources.Classes
{

    public enum Square
    {
        a8 = 0, b8 = 1, c8 = 2, d8 = 3, e8 = 4, f8 = 5, g8 = 6, h8 = 7,
        a7 = 8, b7 = 9, c7 = 10, d7 = 11, e7 = 12, f7 = 13, g7 = 14, h7 = 15,
        a6 = 16, b6 = 17, c6 = 18, d6 = 19, e6 = 20, f6 = 21, g6 = 22, h6 = 23,
        a5 = 24, b5 = 25, c5 = 26, d5 = 27, e5 = 28, f5 = 29, g5 = 30, h5 = 31,
        a4 = 32, b4 = 33, c4 = 34, d4 = 35, e4 = 36, f4 = 37, g4 = 38, h4 = 39,
        a3 = 40, b3 = 41, c3 = 42, d3 = 43, e3 = 44, f3 = 45, g3 = 46, h3 = 47,
        a2 = 48, b2 = 49, c2 = 50, d2 = 51, e2 = 52, f2 = 53, g2 = 54, h2 = 55,
        a1 = 56, b1 = 57, c1 = 58, d1 = 59, e1 = 60, f1 = 61, g1 = 62, h1 = 63, no_sq = 64,
    }
    public enum Side
    {
        White = 0,
        Black = 1,
        Both = 2
    }

    public enum Slider
    {
        Rook = 0,
        Bishop = 1,
    }

    public enum castle
    {
        wk = 1, wq = 2,
        bk = 4, bq = 8,
    }

    public class BitBoard
    {
        public BitBoard()
        {

        }
        //Has pop, get, set functions


        private void testing_bitBoards()
        {
            ulong example = 0x0000000000000081;
            print_bitboard(example);
            set_bit(ref example, Square.e4);
            set_bit(ref example, Square.d7);
            set_bit(ref example, Square.e1);
            print_bitboard(example);
            pop_bit(ref example, Square.d7);
            pop_bit(ref example, Square.d7);
            print_bitboard(example);
            pop_bit(ref example, Square.d7);
            print_bitboard(example);
        }

        public static int count_bits(ulong bitboard)
        {
            return BitOperations.PopCount(bitboard);

            //Myimplementation for the count bits
            //int count = 0;
            //while (bitboard > 0)
            //{
            //    count++;
            //    bitboard &= (bitboard-1)
            //}
            //return count;

        }

        public static int get_lsb_index(ulong bitboard)
        {
            if (bitboard == 0) return -1;
            return BitOperations.TrailingZeroCount(bitboard);

            //Myimplementation for the lsbbits
            //int count = 0;
            //ulong start = 1UL;
            //while ((start & bitboard) == 0)
            //{
            //    count++;
            //    start <<= 1;
            //}
            //return count;

        }


        public static void print_bitboard(ulong bitboard)
        {
            //loop over board ranks 
            for (int rank = 0; rank < 8; rank++)
            {
                Debug.Write(8 - rank + "  ");


                for (int file = 0; file < 8; file++)
                {
                    //convert file & rank into square index
                    int square = rank * 8 + file;



                    Debug.Write(get_bit(bitboard, square) + " ");

                }
                Debug.WriteLine("");
            }
            Debug.WriteLine("\n   a b c d e f g h \n");

            //print bitboard as unsigned num
            Debug.WriteLine("   " + bitboard);
        }

        public static int get_bit(ulong bitboard, int square)
        {
            return ((bitboard & (1UL << square)) == 0) ? 0 : 1;
        }

        public static int get_bit(ulong bitboard, Square square)
        {
            return ((bitboard & (1UL << (int)square)) == 0) ? 0 : 1;
        }

        public static void set_bit(ref ulong bitboard, Square sq)
        {
            bitboard |= (1UL << (int)sq);
        }
        public static void set_bit(ref ulong bitboard, int square)
        {
            bitboard |= (1UL << square);
        }
        public static void pop_bit(ref ulong bitboard, Square sq)
        {
            if (get_bit(bitboard, sq) == 1)
            {
                bitboard ^= (1Ul << (int)sq);
            }
        }
        public static void pop_bit(ref ulong bitboard, int square)
        {
            if (get_bit(bitboard, square) == 1)
            {
                bitboard ^= (1UL << square);
            }
        }
        public static string square_to_coordinates(int squareValue)
        {
            if (squareValue < 0 || squareValue >= 64)
            {
                return "Invalid square";
            }

            int file = squareValue % 8;
            int rank = 8 - squareValue / 8;

            char fileChar = (char)('a' + file);
            return $"{fileChar}{rank}";
        }

    }
}
