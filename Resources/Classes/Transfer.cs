using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace myChess.Resources.Classes
{
    public class Transfer
    {
        public List<Position> NormalSquares = new List<Position>();
        public List<Position> EnpSquares = new List<Position>();
        public List<Position> CastlingSquares = new List<Position>();
        public List<Position> PromotionSquares =  new List<Position>();
        public List<Position> DoublePawnSquares = new List<Position>();

        public bool is_Empty()
        {
            if (NormalSquares.Count == 0 && EnpSquares.Count == 0 && CastlingSquares.Count == 0 && PromotionSquares.Count == 0 && DoublePawnSquares.Count == 0) return true;
            return false;
        }
    }
}
