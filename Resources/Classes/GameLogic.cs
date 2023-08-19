using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace myChess.Resources.Classes
{
    public class GameLogic
    {
        private GameState _gameState;
        private MoveGenerator _moveGenerator;

        public GameLogic()
        {
            _gameState = new GameState();
            _moveGenerator = new MoveGenerator(_gameState);
        }

        public void StartNewGame()
        {
            _gameState.LoadStartingState();
        }

        public List<Position> GetLegalMoves(int row, int column)
        {
            _moveGenerator.SetRowCol(row, column);
            return _moveGenerator.GetLegalMoves();
        }


    }
}
