using System.Collections.Generic;

namespace myChess.Resources.Classes
{
    public class MoveGenerator
    {
        private GameState _currentState;
        private int _row;
        private int _col;

        public MoveGenerator(GameState gameState)
        {
            _currentState = gameState;
        }

        public void SetRowCol(int Row, int Col)
        {
            _row = Row;
            _col = Col;
        }

        public List<Position> GetLegalMoves()
        {
            int index = 8 * _row + _col;
            int pieceValue = _currentState.gameState[index];
            Piece pieceType = PieceType.GetPiece(pieceValue);
            Color pieceColor = PieceType.GetColor(pieceValue);

            List<Position> finalList = new List<Position> { };

            HandlePiece(pieceType, pieceColor, finalList);

            return finalList;
        }

        private void HandlePiece(Piece pieceType, Color pieceColor, List<Position> finalList)
        {
            switch (pieceType)
            {
                case Piece.Pawn:

                    AddPawnMoves(pieceColor, finalList);
                    break;
                case Piece.Knight:
                    AddKnightMoves(pieceColor, finalList);
                    break;
                case Piece.Bishop:
                    AddBishopMoves(pieceColor, finalList);
                    break;
                case Piece.Rook:
                    AddRookMoves(pieceColor, finalList);
                    break;
                case Piece.King:
                    AddKingMoves(pieceColor, finalList);
                    break;
                case Piece.Queen:
                    AddQueenMoves(pieceColor, finalList);
                    break;
                default:
                    break;
            }
        }

        private void AddPawnMoves(Color pieceColor, List<Position> finalList)
        {
            if (pieceColor == Color.White)
            {
                //Handle the normal movement of a pawn
                int ind = 8 * (_row - 1) + _col;
                if (_row != 0 && PieceType.GetPiece(_currentState.gameState[ind]) == Piece.Empty)
                {
                    finalList.Add(new Position(_row - 1, _col));
                    ind = 8 * (_row - 2) + _col;
                    if (_row == 6 && PieceType.GetPiece(_currentState.gameState[ind]) == Piece.Empty)
                    {
                        finalList.Add(new Position(_row - 2, _col));
                    }
                }

                //Handle the case when we have to capture 
                int ind1 = 8 * (_row - 1) + _col - 1;
                int ind2 = 8 * (_row - 1) + _col + 1;
                if (_row != 0 && _col > 0 && _col < 7)
                {
                    if (PieceType.GetColor(_currentState.gameState[ind1]) == Color.Black)
                    {
                        finalList.Add(new Position(_row - 1, _col - 1));
                    }
                    if (PieceType.GetColor(_currentState.gameState[ind2]) == Color.Black)
                    {
                        finalList.Add(new Position(_row - 1, _col + 1));
                    }
                }
                else if (_row != 0 && _col == 0)
                {
                    if (PieceType.GetColor(_currentState.gameState[ind2]) == Color.Black)
                    {
                        finalList.Add(new Position(_row - 1, _col + 1));
                    }
                }
                else if (_row != 0 && _col == 7)
                {
                    if (PieceType.GetColor(_currentState.gameState[ind1]) == Color.Black)
                    {
                        finalList.Add(new Position(_row - 1, _col - 1));
                    }
                }
            }
            else
            {
                //Handle the normal movement of a pawn
                int ind = 8 * (_row + 1) + _col;
                if (_row != 7 && PieceType.GetPiece(_currentState.gameState[ind]) == Piece.Empty)
                {
                    finalList.Add(new Position(_row + 1, _col));
                    ind = 8 * (_row + 2) + _col;
                    if (_row == 1 && PieceType.GetPiece(_currentState.gameState[ind]) == Piece.Empty)
                    {
                        finalList.Add(new Position(_row + 2, _col));
                    }
                }

                //Handle the case when we have to capture 
                int ind1 = 8 * (_row + 1) + _col - 1;
                int ind2 = 8 * (_row + 1) + _col + 1;
                if (_row != 7 && _col > 0 && _col < 7)
                {
                    if (PieceType.GetColor(_currentState.gameState[ind1]) == Color.White)
                    {
                        finalList.Add(new Position(_row + 1, _col - 1));
                    }
                    if (PieceType.GetColor(_currentState.gameState[ind2]) == Color.White)
                    {
                        finalList.Add(new Position(_row + 1, _col + 1));
                    }
                }
                else if (_row != 7 && _col == 0)
                {
                    if (PieceType.GetColor(_currentState.gameState[ind2]) == Color.White)
                    {
                        finalList.Add(new Position(_row + 1, _col + 1));
                    }
                }
                else if (_row != 7 && _col == 7)
                {
                    if (PieceType.GetColor(_currentState.gameState[ind1]) == Color.White)
                    {
                        finalList.Add(new Position(_row + 1, _col - 1));
                    }
                }
            }

        }

        private void AddKnightMoves(Color pieceColor, List<Position> finalList)
        {
            Color thisColor = (pieceColor == Color.White) ? Color.White : Color.Black;
            int[] rowOffsets = { -2, -2, -1, -1, 1, 1, 2, 2 };
            int[] colOffsets = { -1, 1, -2, 2, -2, 2, -1, 1 };

            for (int i = 0; i < 8; i++)
            {
                int newRow = _row + rowOffsets[i];
                int newCol = _col + colOffsets[i];

                if (newRow >= 0 && newRow < 8 && newCol >= 0 && newCol < 8)
                {
                    int newIndex = newRow * 8 + newCol;
                    if (PieceType.GetColor(_currentState.gameState[newIndex]) != thisColor)
                    {
                        finalList.Add(new Position(newRow, newCol));
                    }
                }
            }
        }
        private void AddBishopMoves(Color pieceColor, List<Position> finalList)
        {
            Color thisColor = (pieceColor == Color.White) ? Color.White : Color.Black;

            int[] rowOffsets = { -1, -1, 1, 1 };
            int[] colOffsets = { -1, 1, -1, 1 };

            for (int i = 0; i < 4; i++)
            {
                int newRow = _row + rowOffsets[i];
                int newCol = _col + colOffsets[i];

                while (newRow >= 0 && newRow < 8 && newCol >= 0 && newCol < 8)
                {
                    int newIndex = newRow * 8 + newCol;

                    if (PieceType.GetColor(_currentState.gameState[newIndex]) != thisColor)
                    {
                        finalList.Add(new Position(newRow, newCol));

                        if (PieceType.GetColor(_currentState.gameState[newIndex]) != Color.NoCol)
                        {
                            // Stop searching in this direction if we encounter a piece
                            break;
                        }
                    }
                    else
                    {
                        // Stop searching in this direction if we encounter a piece of the same color
                        break;
                    }

                    newRow += rowOffsets[i];
                    newCol += colOffsets[i];
                }
            }
        }
        private void AddRookMoves(Color pieceColor, List<Position> finalList)
        {
            Color thisColor = (pieceColor == Color.White) ? Color.White : Color.Black;

            int[] rowOffsets = { 0, 0, 1, -1 };
            int[] colOffsets = { -1, 1, 0, 0 };

            for (int i = 0; i < 4; i++)
            {
                int newRow = _row + rowOffsets[i];
                int newCol = _col + colOffsets[i];

                while (newRow >= 0 && newRow < 8 && newCol >= 0 && newCol < 8)
                {
                    int newIndex = newRow * 8 + newCol;

                    if (PieceType.GetColor(_currentState.gameState[newIndex]) != thisColor)
                    {
                        finalList.Add(new Position(newRow, newCol));

                        if (PieceType.GetColor(_currentState.gameState[newIndex]) != Color.NoCol)
                        {
                            // Stop searching in this direction if we encounter a piece
                            break;
                        }
                    }
                    else
                    {
                        // Stop searching in this direction if we encounter a piece of the same color
                        break;
                    }

                    newRow += rowOffsets[i];  // Update here
                    newCol += colOffsets[i];  // Update here
                }
            }
        }

        private void AddKingMoves(Color pieceColor, List<Position> finalList)
        {
            Color thisColor = (pieceColor == Color.White) ? Color.White : Color.Black;

            int[] rowOffsets = { 1, 1, 1, -1, -1, -1, 1, 0, -1, 1, 0, -1 };
            int[] colOffsets = { -1, 0, 1, -1, 0, 1, +1, +1, +1, -1, -1, -1 };

            for (int i = 0; i < 12; i++)
            {
                int newRow = _row + rowOffsets[i];
                int newCol = _col + colOffsets[i];

                if (newRow >= 0 && newRow < 8 && newCol >= 0 && newCol < 8)
                {
                    int newIndex = newRow * 8 + newCol;
                    if (PieceType.GetColor(_currentState.gameState[newIndex]) != thisColor)
                    {
                        finalList.Add(new Position(newRow, newCol));
                    }
                }
            }
        }
        private void AddQueenMoves(Color pieceColor, List<Position> finalList)
        {
            AddBishopMoves(pieceColor, finalList);
            AddRookMoves(pieceColor, finalList);
        }
    }
}
