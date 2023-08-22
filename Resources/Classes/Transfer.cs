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
        public List<Position> CastleSquares = new List<Position>();
        public List<Position> PromotionSquares =  new List<Position>();
        public List<Position> DoublePawnSquares = new List<Position>();

        
    }
}
