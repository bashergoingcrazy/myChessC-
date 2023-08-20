using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;

namespace myChess.Resources.Classes
{
    public class MagicGenerator
    {
        uint random_state = 1804289383;
        //generate 32-bit pseudo legal numbers


        public MagicGenerator()
        {
            //test();
        }

        public void test()
        {
            BitBoard.print_bitboard((ulong)get_random_number());
            BitBoard.print_bitboard((ulong)(get_random_number() & 0xFFFF));
            BitBoard.print_bitboard(get_random_U64_numbers());
            BitBoard.print_bitboard(get_random_U64_numbers() & get_random_U64_numbers() & get_random_U64_numbers());
        }



        






        public ulong get_random_U64_numbers()
        {
            ulong n1, n2, n3, n4;
            n1 = ((ulong) get_random_number()) & 0xFFFF;
            n2 = ((ulong) get_random_number()) & 0xFFFF;
            n3 = ((ulong) get_random_number()) & 0xFFFF;
            n4 = ((ulong) get_random_number()) & 0xFFFF; 

            return n1 | (n2 << 16) | (n3 << 32) | (n4 << 48);
        }

        public ulong generate_magicnumbers()
        {
            return get_random_U64_numbers() & get_random_U64_numbers() & get_random_U64_numbers();
        }
        public uint get_random_number()
        {
            uint number = random_state;

            //XOR shift algo
            number ^= number << 13;
            number ^= number >> 17;
            number ^= number << 5;

            random_state = number;

            return number;
        }

    }
}
