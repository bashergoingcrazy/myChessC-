using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace myChess.Resources.Classes
{
    public static class Codec
    {
        public static int encode_move(int source, int target, int piece, int promoted, int capturef, int dbpushf, int enpf, int castlingf)
        {
            int move = (source) | (target << 6) | (piece << 12) | (promoted << 17) | (capturef << 22) | (dbpushf << 23) | (enpf << 24) | (castlingf << 25);
            return move;
        }

        public static int get_move_source(int move)
        {
            return move & 0x3f;
        }

        public static int get_move_target(int move)
        {
            int res = move & 0xfc0;
            return (res >> 6);
        }

        public static CombinedPiece get_move_piece(int move)
        {
            int res = move & 0x1f000;
            int pp = (res >> 12);
            return (CombinedPiece)pp;
        }

        public static CombinedPiece get_move_promoted(int move)
        {
            int res = move & 0x3e0000;
            int pp = (res >> 17);
            return (CombinedPiece)pp;
        }

        public static int get_move_capture(int move)
        {
            int res = move & 0x400000;
            return (res >> 22);
        }

        public static int get_move_double(int move)
        {
            int res = move & 0x800000;
            return (res >> 23);
        }

        public static int get_move_enpassant(int move)
        {
            int res = move & 0x1000000;
            return (res >> 24);
        }

        public static int get_move_castle(int move)
        {
            int res = move & 0x2000000;
            return (res >> 25);
        }

        //              Binary move bits representation                               Hexidecimal 
        /* source square            0000 0000 0000 0000 0011 1111               0x3f
         * target square            0000 0000 0000 1111 1100 0000               0xfc0
         * piece                    0000 0001 1111 0000 0000 0000               0x1f000
         * promoted piece           0011 1110 0000 0000 0000 0000               0x3e0000
         * capture flag             0100 0000 0000 0000 0000 0000               0x400000
         * double pawn push         1000 0000 0000 0000 0000 0000               0x800000
         * enpassant flag         1 0000 0000 0000 0000 0000 0000               0x1000000
         * castling flag         10 0000 0000 0000 0000 0000 0000               0x2000000
         */
    }
}
