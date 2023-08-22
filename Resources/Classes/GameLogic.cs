using System.Collections.Generic;

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

        public Color GetPieceColor(int row, int column)
        {
            int newIndex = 8 * row + column;
            return PieceType.GetColor(_gameState.gameState[newIndex]);
        }

        public void UpdateState(Position sourcePosition, Position targetPosition)
        {
            int sourceIndex = 8 * sourcePosition.X + sourcePosition.Y;
            int targetIndex = 8 * targetPosition.X + targetPosition.Y;

            _gameState.gameState[targetIndex] = _gameState.gameState[sourceIndex];
            _gameState.gameState[sourceIndex] = PieceType.CreatePiece(Piece.Empty, Color.NoCol);
        }
    }
}
