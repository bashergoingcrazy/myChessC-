using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using myChess.Resources.Classes;

using System.Windows.Media.Imaging;
using System.Media;
using System.Linq;
//using static System.Net.Mime.MediaTypeNames;

namespace myChess.Resources.Classes
{
    public class BoardUI
    {

        private Board _board;
        private Image _draggedPiece;
        private Point _offset;
        private bool _isDragging = false;
        public Point initialCursor;
        public int Inrow, Incol;
        private BitMoveGen _moveGen;
        private Transfer HighlightingSquare;
        private Position CheckSquares;
        private bool isInCheck;
        private MediaPlayer captureAudioPlayer;
        private MediaPlayer moveAudioPlayer;
        bool wantToRotate = true;
        bool isBoardRotated = false;

        public BoardUI(Board board)
        {
            _board = board;
            _moveGen = new BitMoveGen();
            Initialize();
            captureAudioPlayer = new MediaPlayer();
            captureAudioPlayer.Open(new Uri("pack://siteoforigin:,,,/capture.mp3"));

            moveAudioPlayer = new MediaPlayer();
            moveAudioPlayer.Open(new Uri("pack://siteoforigin:,,,/move-self.mp3"));
        }

        public void Initialize()
        {
            Type gridType = _board._grid.GetType();
            Debug.WriteLine($"_board._grid is of type: {gridType}");
            _moveGen.StartNewGame();
            _board.InitializeBoard();
            int imageCounter = 1;


            foreach (UIElement child in _board._grid.Children)
            {
                if (child is Image image)
                {
                    image.Name = $"PieceImage_{imageCounter++}";

                    image.MouseDown += HandleMouseDown;

                    image.MouseMove += HandleMouseMove;

                    image.MouseUp += HandleMouseUp;
                    //Mouse Up mapped over the mouse move only 
                }
            }
        }


        private void HandleMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                _offset = e.GetPosition(sender as Image);
                _isDragging = true;
                Mouse.Capture(sender as Image);
                Point mousePosition = e.GetPosition(_board._grid);
                initialCursor.X = mousePosition.X - _offset.X;
                initialCursor.Y = mousePosition.Y - _offset.Y;


                Image clickedImage = sender as Image;

                Inrow = Grid.GetRow(clickedImage);
                Incol = Grid.GetColumn(clickedImage);
                if (clickedImage.Source != null)
                {
                    HandleHighlighting(clickedImage);
                }
            }
        }

        private void HandleMouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging)
            {
                Image image = sender as Image;
                Point mousePosition = e.GetPosition(_board._grid);
                double left = mousePosition.X - _offset.X;
                double top = mousePosition.Y - _offset.Y;

                var transform = new TranslateTransform(left - initialCursor.X, top - initialCursor.Y);
                image.RenderTransform = transform;



                int row = (int)Math.Floor(top + (75) / 2) / 75;
                int col = (int)Math.Floor(left + (75) / 2) / 75;

                // Store the row and column as tags on the image
                image.Tag = new Tuple<int, int>(row, col);
            }
        }

        private void HandleMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDragging)
            {
                _isDragging = false;
                Mouse.Capture(null);
                Image image = sender as Image;

                // Retrieve the stored row and column from the image's tag
                if (image.Tag is Tuple<int, int> tag)
                {
                    int row = tag.Item1;
                    int col = tag.Item2;

                    image.RenderTransform = null;


                    HandleDrop(image, row, col);
                }
            }
        }

        private void HandleDrop(Image sourceImage, int targetRow, int targetCol)
        {
           
            
            Position targetPosition = new Position(targetRow, targetCol);

            // Check if the target position is within the highlighting squares
            if (HighlightingSquare.NormalSquares.Contains(targetPosition))
            {
              
                //Color targetPieceColor = _gameLogic.GetPieceColor(targetRow, targetCol);

                // If there's an opponent's piece at the target position, remove it
                Image targetImage = FindImageAtPosition(targetRow, targetCol);
                if (targetImage != null)
                {
                    // Remove the target image from the grid
                    _board._grid.Children.Remove(targetImage);
                    PlayCaptureSound();
                }
                else
                {
                    PlayMoveSound();
                }

                // Move the source image to the target position
                Grid.SetRow(sourceImage, targetRow);
                Grid.SetColumn(sourceImage, targetCol);

                Position sourcePosition = new Position(Inrow, Incol);
                // Update the game state to reflect the move
                _moveGen.UpdateGame(Inrow * 8 + Incol, targetRow *8 + targetCol, 0);
                //_gameLogic.UpdateState(sourcePosition, targetPosition);
                isInCheck = false;
                CheckGameEnding();

            }

            if (HighlightingSquare.DoublePawnSquares.Contains(targetPosition))
            {

                //Color targetPieceColor = _gameLogic.GetPieceColor(targetRow, targetCol);

                // If there's an opponent's piece at the target position, remove it
                Image targetImage = FindImageAtPosition(targetRow, targetCol);
                if (targetImage != null)
                {
                    // Remove the target image from the grid
                    _board._grid.Children.Remove(targetImage);
                }

                // Move the source image to the target position
                Grid.SetRow(sourceImage, targetRow);
                Grid.SetColumn(sourceImage, targetCol);

                Position sourcePosition = new Position(Inrow, Incol);
                // Update the game state to reflect the move
                _moveGen.UpdateGame(Inrow*8+Incol,targetRow * 8 + targetCol, 10);
                //_gameLogic.UpdateState(sourcePosition, targetPosition);
                isInCheck = false;
                PlayMoveSound();
                CheckGameEnding();

            }

            if (HighlightingSquare.PromotionSquares.Contains(targetPosition))
            {
                //int sourceRow = Grid.GetRow(sourceImage);
                //int sourceCol = Grid.GetColumn(sourceImage);

                //Color targetPieceColor = _gameLogic.GetPieceColor(targetRow, targetCol);

                // If there's an opponent's piece at the target position, remove it
                Image targetImage = FindImageAtPosition(targetRow, targetCol);
                if (targetImage != null)
                {
                    // Remove the target image from the grid
                    _board._grid.Children.Remove(targetImage);
                }

               
                if (sourceImage != null)
                {
                    _board._grid.Children.Remove(sourceImage);
                }

                // Move the source image to the target position
                ChessPiece piece = new ChessPiece();
                Image newImage = new Image();
                if (_moveGen.PieceColor() == Color.White)
                {
                    newImage.Source = new BitmapImage(new Uri(piece.WhiteQueen, UriKind.Relative)); 
                    newImage.MouseDown += HandleMouseDown;
                    newImage.MouseMove += HandleMouseMove;

                    newImage.MouseUp += HandleMouseUp;
                }
                else
                {
                    newImage.Source = new BitmapImage(new Uri(piece.BlackQueen, UriKind.Relative));
                    newImage.MouseDown += HandleMouseDown;

                    newImage.MouseMove += HandleMouseMove;

                    newImage.MouseUp += HandleMouseUp;
                }
                PlayMoveSound();
                Grid.SetRow(newImage, targetRow);
                Grid.SetColumn(newImage, targetCol);
                _board._grid.Children.Add(newImage);
                //Position sourcePosition = new Position(Inrow, Incol);
                // Update the game state to reflect the move
                _moveGen.UpdateGame(Inrow * 8 + Incol, targetRow*8 + targetCol, 1);
                //_gameLogic.UpdateState(sourcePosition, targetPosition);
                isInCheck = false;
                CheckGameEnding();
   
            }
            if (HighlightingSquare.EnpSquares.Contains(targetPosition))
            {

                //Color targetPieceColor = _gameLogic.GetPieceColor(targetRow, targetCol);

                // If there's an opponent's piece at the target position, remove it

                if (_moveGen.PieceColor() == Color.White)
                {
                    Image targetImage = FindImageAtPosition(targetRow+1, targetCol);
                    if (targetImage != null)
                    {
                        // Remove the target image from the grid
                        _board._grid.Children.Remove(targetImage);
                    }
                }
                else
                {
                    Image targetImage = FindImageAtPosition(targetRow-1, targetCol);
                    if (targetImage != null)
                    {
                        // Remove the target image from the grid
                        _board._grid.Children.Remove(targetImage);
                    }
                }
                PlayCaptureSound();

                // Move the source image to the target position
                Grid.SetRow(sourceImage, targetRow);
                Grid.SetColumn(sourceImage, targetCol);

                Position sourcePosition = new Position(Inrow, Incol);
                // Update the game state to reflect the move
                _moveGen.UpdateGame(Inrow * 8 + Incol, targetRow * 8 + targetCol, 2);
                //_gameLogic.UpdateState(sourcePosition, targetPosition);
                isInCheck = false;
                CheckGameEnding();
            }
            if (HighlightingSquare.CastlingSquares.Contains(targetPosition))
            {
                

                
                //Handling Movemen and Switching of rooks 
                int sq = targetRow* 8 + targetCol;
                if(sq == (int)Square.g1)
                {
                    Image targetImage = FindImageAtPosition(targetRow , targetCol+1);

                    if (targetImage != null)
                    {
                        _board._grid.Children.Remove(targetImage);
                        Grid.SetRow(targetImage, targetRow);
                        Grid.SetColumn(targetImage, targetCol-1);
                        _board._grid.Children.Add(targetImage);
                    }
                }

                else if(sq == (int)Square.c1)
                {
                    Image targetImage = FindImageAtPosition(targetRow, targetCol - 2);

                    if (targetImage != null)
                    {
                        _board._grid.Children.Remove(targetImage);
                        Grid.SetRow(targetImage, targetRow);
                        Grid.SetColumn(targetImage, targetCol + 1);
                        _board._grid.Children.Add(targetImage);
                    }
                }
                else if(sq == (int)Square.g8)
                {
                    Image targetImage = FindImageAtPosition(targetRow, targetCol + 1);

                    if (targetImage != null)
                    {
                        _board._grid.Children.Remove(targetImage);
                        Grid.SetRow(targetImage, targetRow);
                        Grid.SetColumn(targetImage, targetCol - 1);
                        _board._grid.Children.Add(targetImage);
                    }
                }
                else if (sq == (int)Square.c8)
                {
                    Image targetImage = FindImageAtPosition(targetRow, targetCol - 2);

                    if (targetImage != null)
                    {
                        _board._grid.Children.Remove(targetImage);
                        Grid.SetRow(targetImage, targetRow);
                        Grid.SetColumn(targetImage, targetCol + 1);
                        _board._grid.Children.Add(targetImage);
                    }
                    
                }

                PlayMoveSound();


                // Move the source image to the target position
                //ChessPiece piece = new ChessPiece();
                //Image newImage = new Image();
                //if (_moveGen.PieceColor() == Color.White)
                //{
                //    newImage.Source = new BitmapImage(new Uri(piece.WhiteQueen, UriKind.Relative));
                //}
                //else
                //{
                //    newImage.Source = new BitmapImage(new Uri(piece.BlackQueen, UriKind.Relative));
                //}
                Grid.SetRow(sourceImage, targetRow);
                Grid.SetColumn(sourceImage, targetCol);
                //_board._grid.Children.Add(newImage);
                //Position sourcePosition = new Position(Inrow, Incol);
                // Update the game state to reflect the move
                _moveGen.UpdateGame(Inrow * 8 + Incol, targetRow * 8 + targetCol, 3);
                //_gameLogic.UpdateState(sourcePosition, targetPosition);
                isInCheck = false;
                CheckGameEnding();
            }
            RedrawSquares();
            
        }

        private void RedrawSquares()
        {
            Brush lightSquareBrush = _board._lightSquareColor;
            Brush darkSquareBrush = _board._darkSquareColor;

            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    Rectangle square = GetRectangleAtPosition(row, col);
                    if (square != null)
                    {
                        square.Fill = (row + col) % 2 == 0 ? lightSquareBrush : darkSquareBrush;
                    }
                }
            }
            if (isInCheck)
            {
                Rectangle square = GetRectangleAtPosition(CheckSquares.X, CheckSquares.Y);
                if (square != null)
                {
                    square.Fill = Brushes.Red;
                }
            }
            
        }

        private void CheckGameEnding()
        {
            Position checker =  _moveGen.CheckGameEnd();
            if (checker.X != -1)
            {
                if(checker.X == 99)
                {
                    MessageBox.Show("Stalemate Draw");
                }


                if (checker.X == 100)
                {
                    Side winner = ((int)Side.White == checker.Y) ? Side.Black : Side.White;
                    MessageBox.Show(winner + "Won");
                }


                CheckSquares = checker;
                Rectangle square = GetRectangleAtPosition(checker.X, checker.Y);
                if (square != null)
                {
                    square.Fill = Brushes.Red;
                    isInCheck = true;
                }
            }
            
        }

        public void HandleHighlighting(Image clickedImage)
        {
            int row = Grid.GetRow(clickedImage);
            int column = Grid.GetColumn(clickedImage);
            _board._grid.Children.Remove(clickedImage);
            _board._grid.Children.Add(clickedImage);

            HandleHighlightingAsync(row, column);

        }

        private async void HandleHighlightingAsync(int row, int column)
        {
            HighlightingSquare = await _moveGen.GetLegalMovesAsync(row, column);
            List<Position> combinedSquares = new List<Position>(HighlightingSquare.NormalSquares);
            combinedSquares.AddRange(HighlightingSquare.DoublePawnSquares);
            if (combinedSquares.Count != 0)
            {
                foreach (Position square in combinedSquares)
                {
                    int highlightRow = square.X;
                    int highlightCol = square.Y;
                    Rectangle cellRectangle = GetRectangleAtPosition(highlightRow, highlightCol); // Implement this method
                    if (cellRectangle != null)
                    {
                        cellRectangle.Fill = Brushes.LightBlue;
                    }
                }
            }
            if (HighlightingSquare.PromotionSquares.Count != 0)
            {
              
                foreach (Position square in HighlightingSquare.PromotionSquares)
                {
                    int highlightRow = square.X;
                    int highlightCol = square.Y;
                    Rectangle cellRectangle = GetRectangleAtPosition(highlightRow, highlightCol); // Implement this method
                    if (cellRectangle != null)
                    {
                        cellRectangle.Fill = Brushes.PeachPuff;
                    }
                }
            }
            if (HighlightingSquare.EnpSquares.Count != 0)
            {
               
                foreach (Position square in HighlightingSquare.EnpSquares)
                {
                    int highlightRow = square.X;
                    int highlightCol = square.Y;
                    Rectangle cellRectangle = GetRectangleAtPosition(highlightRow, highlightCol); // Implement this method
                    if (cellRectangle != null)
                    {
                        cellRectangle.Fill = Brushes.PeachPuff;
                    }
                }
            }
            if (HighlightingSquare.CastlingSquares.Count != 0)
            {

                foreach (Position square in HighlightingSquare.CastlingSquares)
                {
                    int highlightRow = square.X;
                    int highlightCol = square.Y;
                    Rectangle cellRectangle = GetRectangleAtPosition(highlightRow, highlightCol); // Implement this method
                    if (cellRectangle != null)
                    {
                        cellRectangle.Fill = Brushes.PeachPuff;
                    }
                }
            }

        }

        private void PlayCaptureSound()
        {
            if(captureAudioPlayer != null)
            {
                captureAudioPlayer.Stop();
                captureAudioPlayer.Position = TimeSpan.Zero;
                captureAudioPlayer.Play();
            }
        }
        private void PlayMoveSound()
        {
            if (moveAudioPlayer != null)
            {
                moveAudioPlayer.Stop();
                moveAudioPlayer.Position = TimeSpan.Zero;
                moveAudioPlayer.Play();
            }
        }

        

        private Rectangle GetRectangleAtPosition(int row, int col)
        {
            int index = row * 8 + col; // Calculate the index of the rectangle
            if (index >= 0 && index < _board._grid.Children.Count)
            {
                UIElement element = _board._grid.Children[index];
                if (element is Rectangle rectangle)
                {
                    return rectangle;
                }
            }
            return null; // No rectangle found at the specified position
        }

        private Image FindImageAtPosition(int row, int col)
        {
            foreach (UIElement child in _board._grid.Children)
            {
                if (child is Image targetImage && Grid.GetRow(targetImage) == row && Grid.GetColumn(targetImage) == col)
                {
                    return targetImage;
                    break;
                }
            }
            return null;
        }
    }
}

//int index = 8 * row + col;
//int pieceValue = _gameState.gameState[index];
//Piece pieceType = PieceType.GetPiece(pieceValue);
//Color pieceColor = PieceType.GetColor(pieceValue);

//string pieceTypeName = Enum.GetName(typeof(Piece), pieceType);
////string pieceColorName = (pieceColor == Color.White) ? "White" : "Black";
//string pieceColorName;
//if (pieceColor == Color.White)
//{
//    pieceColorName = "White";
//}
//else if (pieceColor == Color.Black)
//{
//    pieceColorName = "Black";
//}
//else
//{
//    pieceColorName = "NoCol";
//}

//Debug.WriteLine($"Piece at row {row}, column {col}: {pieceColorName} {pieceTypeName}");



//int sourceIndex = 8 * Inrow + Incol;
//int targetIndex = 8 * row + col;

//int sourcePiece = _gameState.gameState[sourceIndex];
//int targetPiece = _gameState.gameState[targetIndex];

//Color sourcePieceColor = PieceType.GetColor(sourcePiece);
//Color targetPieceColor = PieceType.GetColor(targetPiece);

//if (sourcePieceColor != targetPieceColor && targetPieceColor != Color.NoCol)
//{
//    // Handle capturing here
//    // You can update the gameState to remove the captured piece


//    foreach (UIElement child in _board._grid.Children)
//    {
//        if (child is Image targetImage && Grid.GetRow(targetImage) == row && Grid.GetColumn(targetImage) == col)
//        {
//            _board._grid.Children.Remove(targetImage);
//            break;
//        }
//    }
//}