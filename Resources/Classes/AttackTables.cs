using System;
using System.Diagnostics;

namespace myChess.Resources.Classes
{
    public class AttackTables
    {
        public ulong[,] pawn_attacks = new ulong[2, 64];
        public ulong[] knight_attacks = new ulong[64];
        public ulong[] king_attacks = new ulong[64];

        public ulong[] bishop_masks = new ulong[64];
        public ulong[] rook_masks = new ulong[64];

        public ulong[,] bishop_attacks = new ulong[64, 512];
        public ulong[,] rook_attacks = new ulong[64, 4096];


        MagicGenerator MG = new MagicGenerator();

        public AttackTables()
        {
            init_leapers_attacks();
            init_sliders_attacks((int)Slider.Bishop);
            init_sliders_attacks((int)Slider.Rook);

            //init_magic_numbers();    //Used for generating an initalizing magic numbers
            //testing();
        }

        void init_leapers_attacks()
        {
            for (int square = 0; square < 64; square++)
            {
                pawn_attacks[0, square] = MaskPawnAttacks(Side.White, square);
                pawn_attacks[1, square] = MaskPawnAttacks(Side.Black, square);

                knight_attacks[square] = MaskKnightAttacks(square);

                king_attacks[square] = MaskKingAttacks(square);

            }

        }

        private void testing()
        {
            //ulong occupancy = 0UL;

            //BitBoard.set_bit(ref occupancy, (int)Square.b6);
            //BitBoard.set_bit(ref occupancy, (int)Square.d6);
            //BitBoard.set_bit(ref occupancy, (int)Square.f6);
            //BitBoard.set_bit(ref occupancy, (int)Square.b4);
            //BitBoard.set_bit(ref occupancy, (int)Square.g4);
            //BitBoard.set_bit(ref occupancy, (int)Square.c3);
            //BitBoard.set_bit(ref occupancy, (int)Square.d3);
            //BitBoard.set_bit(ref occupancy, (int)Square.e3);
            //BitBoard.print_bitboard(occupancy);


            //BitBoard.print_bitboard(get_queen_attacks((int)Square.d4, occupancy));
            //BitBoard.print_bitboard(get_queen_attacks((int)Square.e5, occupancy));


        }

        public ulong MaskPawnAttacks(Side side, Square square)
        {
            int squareValue = (int)square; // Convert enum to integer
            return MaskPawnAttacks(side, squareValue);
        }

        public ulong MaskPawnAttacks(Side side, int square)
        {
            ulong bitboard = 0UL;
            ulong attacks = 0UL;

            // Set piece on board
            BitBoard.set_bit(ref bitboard, square);
            //handle.print_bitboard(bitboard);

            // Calculate pawn attacks here and update the 'attacks' variable ulong accordingly
            //White Pawns
            if (side == Side.White)
            {
                if (((bitboard) & Constants.NOT_H_FILE) != 0) attacks |= (bitboard >> 7);
                if (((bitboard) & Constants.NOT_A_FILE) != 0) attacks |= (bitboard >> 9);
            }
            //Black Pawns
            else
            {
                if (((bitboard) & Constants.NOT_A_FILE) != 0) attacks |= (bitboard << 7);
                if (((bitboard) & Constants.NOT_H_FILE) != 0) attacks |= (bitboard << 9);
            }

            //handle.print_bitboard(attacks);

            return attacks;
        }

        public ulong MaskKnightAttacks(int square)
        {
            ulong bitboard = 0UL;
            ulong attacks = 0UL;

            // Set piece on board
            BitBoard.set_bit(ref bitboard, square);
            //handle.print_bitboard(bitboard);

            if ((bitboard & Constants.NOT_A_FILE) != 0) attacks |= (bitboard >> 17);
            if ((bitboard & Constants.NOT_H_FILE) != 0) attacks |= (bitboard >> 15);
            if ((bitboard & Constants.NOT_AB_FILE) != 0) attacks |= (bitboard >> 10);
            if ((bitboard & Constants.NOT_HG_FILE) != 0) attacks |= (bitboard >> 6);

            if ((bitboard & Constants.NOT_H_FILE) != 0) attacks |= (bitboard << 17);
            if ((bitboard & Constants.NOT_A_FILE) != 0) attacks |= (bitboard << 15);
            if ((bitboard & Constants.NOT_HG_FILE) != 0) attacks |= (bitboard << 10);
            if ((bitboard & Constants.NOT_AB_FILE) != 0) attacks |= (bitboard << 6);


            //handle.print_bitboard(attacks);
            return attacks;
        }

        public ulong MaskKingAttacks(int square)
        {
            ulong bitboard = 0UL;
            ulong attacks = 0UL;

            // Set piece on board
            BitBoard.set_bit(ref bitboard, square);
            //handle.print_bitboard(bitboard);

            if ((bitboard & Constants.NOT_A_FILE) != 0)
            {
                attacks |= (bitboard >> 9);
                attacks |= (bitboard >> 1);
                attacks |= (bitboard << 7);
            }
            if ((bitboard & Constants.NOT_H_FILE) != 0)
            {
                attacks |= (bitboard >> 7);
                attacks |= (bitboard << 1);
                attacks |= (bitboard << 9);
            }

            if ((bitboard << 8) != 0) attacks |= (bitboard << 8);
            if ((bitboard >> 8) != 0) attacks |= (bitboard >> 8);


            //handle.print_bitboard(attacks);
            return attacks;
        }

        public ulong get_bishop_attacks(int square, ulong occupancy)
        {
            //get bishop attacks assuming current board occupancy
            occupancy &= bishop_masks[square];
            occupancy *= Constants.BISHOP_MAGIC_NUMBERS[square];
            occupancy >>= 64 - Constants.bishop_relevant_bits[square];

            return bishop_attacks[square, occupancy];

        }

        public ulong get_rook_attacks(int square, ulong occupancy)
        {
            occupancy &= rook_masks[square];
            occupancy *= Constants.ROOK_MAGIC_NUMBERS[square];
            occupancy >>= 64 - Constants.rook_relevant_bits[square];

            return rook_attacks[square, occupancy];
        }

        public ulong get_queen_attacks(int square, ulong occupancy)
        {
            //init result attacks bitboard
            ulong result = 0Ul;

            //init bishop occupancies
            ulong bishop_occupancies = occupancy;

            ulong rook_occpancies = occupancy;


            bishop_occupancies &= bishop_masks[square];
            bishop_occupancies *= Constants.BISHOP_MAGIC_NUMBERS[square];
            bishop_occupancies >>= 64 - Constants.bishop_relevant_bits[square];

            result = bishop_attacks[square, bishop_occupancies];

            rook_occpancies &= rook_masks[square];
            rook_occpancies *= Constants.ROOK_MAGIC_NUMBERS[square];
            rook_occpancies >>= 64 - Constants.rook_relevant_bits[square];

            result |= rook_attacks[square, rook_occpancies];

            return result;
        }

        public void init_sliders_attacks(int bishop)
        {
            for (int square = 0; square < 64; square++)
            {
                bishop_masks[square] = MaskBishopAttacks(square);
                rook_masks[square] = MaskRookAttacks(square);


                //init curren mask
                ulong attack_mask = (bishop == (int)Slider.Bishop) ? bishop_masks[square] : rook_masks[square];
                int relevant_bits = BitBoard.count_bits(attack_mask);

                int occupancy_indicies = (1 << relevant_bits);

                for (int index = 0; index < occupancy_indicies; index++)
                {
                    if (bishop == (int)Slider.Bishop)
                    {
                        ulong occupancy = set_occupancy(index, relevant_bits, attack_mask);

                        ulong magic_index = (occupancy * Constants.BISHOP_MAGIC_NUMBERS[square]) >> (64 - Constants.bishop_relevant_bits[square]);

                        bishop_attacks[square, magic_index] = BishopAttacksOnTheFly(square, occupancy);
                    }
                    else
                    {
                        ulong occupancy = set_occupancy(index, relevant_bits, attack_mask);

                        ulong magic_index = (occupancy * Constants.ROOK_MAGIC_NUMBERS[square]) >> (64 - Constants.rook_relevant_bits[square]);

                        rook_attacks[square, magic_index] = RookAttacksOnTheFly(square, occupancy);
                    }
                }
            }
        }


        public ulong MaskBishopAttacks(int square)
        {
            ulong attacks = 0ul;
            int r, f;

            int tr = square / 8;
            int tf = square % 8;

            for (r = tr + 1, f = tf + 1; r <= 6 && f <= 6; r++, f++) attacks |= (1ul << (r * 8 + f));
            for (r = tr - 1, f = tf - 1; r >= 1 && f >= 1; r--, f--) attacks |= (1ul << (r * 8 + f));
            for (r = tr + 1, f = tf - 1; r <= 6 && f >= 1; r++, f--) attacks |= (1ul << (r * 8 + f));
            for (r = tr - 1, f = tf + 1; r >= 1 && f <= 6; r--, f++) attacks |= (1ul << (r * 8 + f));

            //handle.print_bitboard(attacks);


            return attacks;
        }

        public ulong MaskRookAttacks(int square)
        {
            ulong attacks = 0ul;
            int r, f;

            int tr = square / 8;
            int tf = square % 8;

            for (r = tr + 1; r <= 6; r++) attacks |= (1ul << (r * 8 + tf));
            for (r = tr - 1; r >= 1; r--) attacks |= (1ul << (r * 8 + tf));
            for (f = tf + 1; f <= 6; f++) attacks |= (1ul << (tr * 8 + f));
            for (f = tf - 1; f >= 1; f--) attacks |= (1ul << (tr * 8 + f));
            //handle.print_bitboard(attacks);


            return attacks;
        }

        //generate bishop attacks on the fly 
        public ulong BishopAttacksOnTheFly(int square, ulong block)
        {
            ulong attacks = 0ul;
            int r, f;

            int tr = square / 8;
            int tf = square % 8;

            for (r = tr + 1, f = tf + 1; r <= 7 && f <= 7; r++, f++)
            {
                attacks |= (1ul << (r * 8 + f));
                if ((block & (1ul << (r * 8 + f))) != 0) break;
            }
            for (r = tr - 1, f = tf - 1; r >= 0 && f >= 0; r--, f--)
            {
                attacks |= (1ul << (r * 8 + f));
                if ((block & (1ul << (r * 8 + f))) != 0) break;
            }
            for (r = tr + 1, f = tf - 1; r <= 7 && f >= 0; r++, f--)
            {
                attacks |= (1ul << (r * 8 + f));
                if ((block & (1ul << (r * 8 + f))) != 0) break;
            }
            for (r = tr - 1, f = tf + 1; r >= 0 && f <= 7; r--, f++)
            {
                attacks |= (1ul << (r * 8 + f));
                if ((block & (1ul << (r * 8 + f))) != 0) break;
            }

            //handle.print_bitboard(attacks);

            return attacks;
        }


        public ulong RookAttacksOnTheFly(int square, ulong block)
        {
            ulong attacks = 0ul;
            int r, f;

            int tr = square / 8;
            int tf = square % 8;

            for (r = tr + 1; r <= 7; r++)
            {
                attacks |= (1ul << (r * 8 + tf));
                if ((block & (1ul << (r * 8 + tf))) != 0) break;
            }

            for (r = tr - 1; r >= 0; r--)
            {
                attacks |= (1ul << (r * 8 + tf));
                if ((block & (1ul << (r * 8 + tf))) != 0) break;
            }
            for (f = tf + 1; f <= 7; f++)
            {
                attacks |= (1ul << (tr * 8 + f));
                if ((block & (1ul << (tr * 8 + f))) != 0) break;
            }
            for (f = tf - 1; f >= 0; f--)
            {
                attacks |= (1ul << (tr * 8 + f));
                if ((block & (1ul << (tr * 8 + f))) != 0) break;
            }
            //handle.print_bitboard(attacks);


            return attacks;
        }


        public ulong set_occupancy(int index, int bits_in_mask, ulong attack_mask)
        {
            //occupancy map
            ulong occupancy = 0UL;

            //loop over the range of bits in within the attack mask
            for (int count = 0; count < bits_in_mask; count++)
            {
                int square = BitBoard.get_lsb_index(attack_mask);

                BitBoard.pop_bit(ref attack_mask, square);

                //make sure that occupancy is on board
                if ((index & (1 << count)) != 0)
                {
                    occupancy |= (1UL << square);
                }
            }

            return occupancy;
        }

        public ulong find_magic_number(int square, int relevant_bits, int bishop)
        {
            ulong[] occupancies = new ulong[4096];
            ulong[] attacks = new ulong[4096];
            ulong[] used_attacks = new ulong[4096];

            // init attack mask for the current piece
            ulong attack_mask = (bishop == (int)Slider.Bishop) ? MaskBishopAttacks(square) : MaskRookAttacks(square);

            int occupancy_indices = 1 << relevant_bits;

            for (int index = 0; index < occupancy_indices; index++)
            {
                // init occupancies
                occupancies[index] = set_occupancy(index, relevant_bits, attack_mask);

                // init attacks
                attacks[index] = (bishop == (int)Slider.Bishop) ? BishopAttacksOnTheFly(square, occupancies[index]) : RookAttacksOnTheFly(square, occupancies[index]);
            }

            // test magic numbers loop
            for (int random_count = 0; random_count < 1000000000; random_count++)
            {
                ulong magic_number = MG.generate_magicnumbers();

                // skip inappropriate magic numbers
                if (BitBoard.count_bits((attack_mask * magic_number) & 0xFF00000000000000) < 6)
                    continue;

                // init used attacks array
                Array.Clear(used_attacks, 0, used_attacks.Length);

                // init index & fail flag
                int index, fail;
                for (index = 0, fail = 0; fail == 0 && index < occupancy_indices; index++)
                {
                    // init magic index
                    int magic_index = (int)((occupancies[index] * magic_number) >> (64 - relevant_bits));

                    // On empty index available
                    if (used_attacks[magic_index] == 0UL)
                        used_attacks[magic_index] = attacks[index];
                    else if (used_attacks[magic_index] != attacks[index])
                        fail = 1;
                }

                if (fail == 0)
                    return magic_number;
            }

            Debug.WriteLine("Magic number fails!");
            return 0UL;
        }

        void init_magic_numbers()
        {
            //loop over 64 board squares
            for (int square = 0; square < 64; square++)
            {
                ulong magicNumber = find_magic_number(square, Constants.rook_relevant_bits[square], 0);
                Debug.WriteLine("0x" + magicNumber.ToString("X") + ", ");
            }

            Debug.WriteLine("\n\n");

            for (int square = 0; square < 64; square++)
            {
                ulong magicNumber = find_magic_number(square, Constants.bishop_relevant_bits[square], 1);
                Debug.WriteLine("0x" + magicNumber.ToString("X") + ",");
            }

        }







    }
    public class Constants
    {
        public const ulong NOT_A_FILE = 18374403900871474942;
        public const ulong NOT_H_FILE = 0x7F7F7F7F7F7F7F7F;
        public const ulong NOT_HG_FILE = 0x3F3F3F3F3F3F3F3F;
        public const ulong NOT_AB_FILE = 0xFCFCFCFCFCFCFCFC;
        public const string EMPTY_BOARD = "8/8/8/8/8/8/8/8 w - - ";
        public const string START_POSITION = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1 ";
        public const string TRICKY_POSITION = "r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R b KQkq - 0 1 ";
        public const string KILLER_POSITION = "rnbqkb1r/pp1p1pPp/8/2p1pP2/1P1P4/3P3P/P1P1P3/RNBQKBNR w KQkq e6 0 1";
        public const string CMK_POSITION = "r2q1rk1/ppp2ppp/2n1bn2/2b1p3/3pP3/3P1NPP/PPP1NPB1/R1BQ1RK1 b - - 0 9 ";
      


        public static readonly string[] SetOfCoordinates = new string[]
        {
            "a8", "b8", "c8", "d8", "e8", "f8", "g8", "h8",
            "a7", "b7", "c7", "d7", "e7", "f7", "g7", "h7",
            "a6", "b6", "c6", "d6", "e6", "f6", "g6", "h6",
            "a5", "b5", "c5", "d5", "e5", "f5", "g5", "h5",
            "a4", "b4", "c4", "d4", "e4", "f4", "g4", "h4",
            "a3", "b3", "c3", "d3", "e3", "f3", "g3", "h3",
            "a2", "b2", "c2", "d2", "e2", "f2", "g2", "h2",
            "a1", "b1", "c1", "d1", "e1", "f1", "g1", "h1"
        };
        public static int[] bishop_relevant_bits = new int[64]
        {
            6, 5, 5, 5, 5, 5, 5, 6,
            5, 5, 5, 5, 5, 5, 5, 5,
            5, 5, 7, 7, 7, 7, 5, 5,
            5, 5, 7, 9, 9, 7, 5, 5,
            5, 5, 7, 9, 9, 7, 5, 5,
            5, 5, 7, 7, 7, 7, 5, 5,
            5, 5, 5, 5, 5, 5, 5, 5,
            6, 5, 5, 5, 5, 5, 5, 6,
        };

        public static int[] rook_relevant_bits = new int[64]
        {
            12, 11, 11, 11, 11, 11, 11, 12,
            11, 10, 10, 10, 10, 10, 10, 11,
            11, 10, 10, 10, 10, 10, 10, 11,
            11, 10, 10, 10, 10, 10, 10, 11,
            11, 10, 10, 10, 10, 10, 10, 11,
            11, 10, 10, 10, 10, 10, 10, 11,
            11, 10, 10, 10, 10, 10, 10, 11,
            12, 11, 11, 11, 11, 11, 11, 12,
        };

        public static ulong[] ROOK_MAGIC_NUMBERS = new ulong[64]
        {
            0x8A80104000800020,
            0x140002000100040,
            0x2801880A0017001,
            0x100081001000420,
            0x200020010080420,
            0x3001C0002010008,
            0x8480008002000100,
            0x2080088004402900,
            0x800098204000,
            0x2024401000200040,
            0x100802000801000,
            0x120800800801000,
            0x208808088000400,
            0x2802200800400,
            0x2200800100020080,
            0x801000060821100,
            0x80044006422000,
            0x100808020004000,
            0x12108A0010204200,
            0x140848010000802,
            0x481828014002800,
            0x8094004002004100,
            0x4010040010010802,
            0x20008806104,
            0x100400080208000,
            0x2040002120081000,
            0x21200680100081,
            0x20100080080080,
            0x2000A00200410,
            0x20080800400,
            0x80088400100102,
            0x80004600042881,
            0x4040008040800020,
            0x440003000200801,
            0x4200011004500,
            0x188020010100100,
            0x14800401802800,
            0x2080040080800200,
            0x124080204001001,
            0x200046502000484,
            0x480400080088020,
            0x1000422010034000,
            0x30200100110040,
            0x100021010009,
            0x2002080100110004,
            0x202008004008002,
            0x20020004010100,
            0x2048440040820001,
            0x101002200408200,
            0x40802000401080,
            0x4008142004410100,
            0x2060820C0120200,
            0x1001004080100,
            0x20C020080040080,
            0x2935610830022400,
            0x44440041009200,
            0x280001040802101,
            0x2100190040002085,
            0x80C0084100102001,
            0x4024081001000421,
            0x20030A0244872,
            0x12001008414402,
            0x2006104900A0804,
            0x1004081002402,
        };

        public static ulong[] BISHOP_MAGIC_NUMBERS = new ulong[64]
        {
            0x40040844404084,
            0x2004208A004208,
            0x10190041080202,
            0x108060845042010,
            0x581104180800210,
            0x2112080446200010,
            0x1080820820060210,
            0x3C0808410220200,
            0x4050404440404,
            0x21001420088,
            0x24D0080801082102,
            0x1020A0A020400,
            0x40308200402,
            0x4011002100800,
            0x401484104104005,
            0x801010402020200,
            0x400210C3880100,
            0x404022024108200,
            0x810018200204102,
            0x4002801A02003,
            0x85040820080400,
            0x810102C808880400,
            0xE900410884800,
            0x8002020480840102,
            0x220200865090201,
            0x2010100A02021202,
            0x152048408022401,
            0x20080002081110,
            0x4001001021004000,
            0x800040400A011002,
            0xE4004081011002,
            0x1C004001012080,
            0x8004200962A00220,
            0x8422100208500202,
            0x2000402200300C08,
            0x8646020080080080,
            0x80020A0200100808,
            0x2010004880111000,
            0x623000A080011400,
            0x42008C0340209202,
            0x209188240001000,
            0x400408A884001800,
            0x110400A6080400,
            0x1840060A44020800,
            0x90080104000041,
            0x201011000808101,
            0x1A2208080504F080,
            0x8012020600211212,
            0x500861011240000,
            0x180806108200800,
            0x4000020E01040044,
            0x300000261044000A,
            0x802241102020002,
            0x20906061210001,
            0x5A84841004010310,
            0x4010801011C04,
            0xA010109502200,
            0x4A02012000,
            0x500201010098B028,
            0x8040002811040900,
            0x28000010020204,
            0x6000020202D0240,
            0x8918844842082200,
            0x4010011029020020,
        };

    }

}
