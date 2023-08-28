using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using System.Windows.Shapes;
using myChess.Resources.Classes;

using System.Windows.Media.Imaging;
using System.Media;
using System.Linq;
using myChess.Resources.UserControls;
using System.Windows.Media.Media3D;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Animation;
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
        bool wantToRotate = false;
        bool isBoardRotated = false;
        private promotions _whitePromotionControls;
        private Transform _draggingTransform;
        private Popup _promotionPopup;
        private promotions _whitePromotions;
        private Position _promotion_TargetSqauare;
        private blackPromotions _blackPromotions;
        private AiMoveGen _aiMoveGen;


        public BoardUI(Board board)
        {
            _board = board;
            _moveGen = new BitMoveGen();
            Initialize();
            captureAudioPlayer = new MediaPlayer();
            captureAudioPlayer.Open(new Uri("pack://siteoforigin:,,,/capture.mp3"));

            moveAudioPlayer = new MediaPlayer();
            moveAudioPlayer.Open(new Uri("pack://siteoforigin:,,,/move-self.mp3"));

            _aiMoveGen = new AiMoveGen();
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

                

                if (isBoardRotated)
                {
                    // Adjust translation for rotated board
                    double centerX = _board._grid.ActualWidth / 8;
                    double centerY = _board._grid.ActualHeight / 8;
                    double offsetX = left - initialCursor.X + centerX;
                    double offsetY = top - initialCursor.Y + centerY;

                    _draggingTransform = new TransformGroup
                    {
                        Children =
                {
                    new RotateTransform(180), // Add 180-degree rotation to the image
                    new TranslateTransform(offsetX, offsetY) // Add the adjusted translation
                }
                    };
                }
                else
                {
                    _draggingTransform = new TranslateTransform(left - initialCursor.X, top - initialCursor.Y);
                }

                image.RenderTransform = _draggingTransform;


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
                image.RenderTransform = null;
                if (!(wantToRotate)&& (isBoardRotated))
                {
                    
                    image.UpdateLayout();
                    Debug.WriteLine("Herer");
                    Debug.WriteLine("Before transform"+ image.RenderTransform);
                    RotateTransform tp = new RotateTransform(180);
                    TranslateTransform translateTransform = new TranslateTransform(75, 75);

                    TransformGroup transformGroup = new TransformGroup();
                    transformGroup.Children.Add(tp);
                    transformGroup.Children.Add(translateTransform);
                    image.RenderTransform = transformGroup;
                    Debug.WriteLine("After transform" + image.RenderTransform);
                }

                // Retrieve the stored row and column from the image's tag
                if (image.Tag is Tuple<int, int> tag)
                {
                    int row = tag.Item1;
                    int col = tag.Item2;

                    //image.RenderTransform = null;


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
                HandleNormalMove(sourceImage, targetPosition);
  
            }

            if (HighlightingSquare.DoublePawnSquares.Contains(targetPosition))
            {
                HandleDoublePawnMove(sourceImage, targetPosition);
            }

            if (HighlightingSquare.PromotionSquares.Contains(targetPosition))
            {
                HandlePromotion(sourceImage, targetPosition);    
            }
            if (HighlightingSquare.EnpSquares.Contains(targetPosition))
            {
                HandleEnPassantCapture(sourceImage, targetPosition);             
            }
            if (HighlightingSquare.CastlingSquares.Contains(targetPosition))
            {
                HandleCastling(sourceImage, targetPosition);
            }
            RedrawSquares();
        }

        private void HandleNormalMove(Image sourceImage , Position targetPosition)
        {
            // Handle normal move logic
            //Color targetPieceColor = _gameLogic.GetPieceColor(targetRow, targetCol);

            // If there's an opponent's piece at the target position, remove it
            Image targetImage = FindImageAtPosition(targetPosition.X, targetPosition.Y);
            int captured = 0;
            if (targetImage != null)
            {
                // Remove the target image from the grid
                _board._grid.Children.Remove(targetImage);
                captured = 8;
                PlayCaptureSound();
            }
            else
            {
                PlayMoveSound();
            }

            // Move the source image to the target position
            Grid.SetRow(sourceImage, targetPosition.X);
            Grid.SetColumn(sourceImage, targetPosition.Y);

            Position sourcePosition = new Position(Inrow, Incol);
            // Update the game state to reflect the move
            _moveGen.UpdateGame(Inrow * 8 + Incol, targetPosition.X * 8 + targetPosition.Y, 0|captured);
            //_gameLogic.UpdateState(sourcePosition, targetPosition);
            isInCheck = false;
            CheckGameEnding();
            RotateBoard();
            MakeMoveAi();
        }

        private void HandleDoublePawnMove(Image sourceImage, Position targetPosition)
        {
            
            
            // Move the source image to the target position
            Grid.SetRow(sourceImage, targetPosition.X);
            Grid.SetColumn(sourceImage, targetPosition.Y);

            Position sourcePosition = new Position(Inrow, Incol);
            // Update the game state to reflect the move
            _moveGen.UpdateGame(Inrow * 8 + Incol, targetPosition.X * 8 + targetPosition.Y, 4);
            //_gameLogic.UpdateState(sourcePosition, targetPosition);
            isInCheck = false;
            PlayMoveSound();
            CheckGameEnding();
            RotateBoard();

            MakeMoveAi();

        }
        private bool _promotion_Capture = false;
        private void HandlePromotion(Image sourceImage, Position targetPosition)
        {
            // Handle promotion logic
            Image targetImage = FindImageAtPosition(targetPosition.X, targetPosition.Y);
            int Captured = 0;
            if (targetImage != null)
            {
                // Remove the target image from the grid
                Captured = 8;
                _board._grid.Children.Remove(targetImage);
            }
            _promotion_TargetSqauare = targetPosition;

            if (sourceImage != null)
            {
                _board._grid.Children.Remove(sourceImage);
                _promotion_Capture = true;
            }

            if(_moveGen.SideToMove() == Side.White)
            {
                _promotionPopup = new Popup
                {
                    Placement = PlacementMode.Mouse,
                    PlacementTarget = _board._grid,
                    StaysOpen = false,
                    Child = new promotions(),
                };
                _whitePromotions = _promotionPopup.Child as promotions;
                _whitePromotions.PieceSelected += WhitePromotions_PieceSelected;
                _promotionPopup.IsOpen = true;
            }
            else
            {
                _promotionPopup = new Popup
                {
                    Placement = PlacementMode.Mouse,
                    PlacementTarget = _board._grid,
                    StaysOpen = false,
                    Child = new blackPromotions(),
                };
                _blackPromotions = _promotionPopup.Child as blackPromotions;
                _blackPromotions.PieceSelected += BlackPromotions_PieceSelected;
                _promotionPopup.IsOpen = true;
            }
        }
            //// Move the source image to the target position
            //ChessPiece piece = new ChessPiece();
            //Image newImage = new Image();
            //if (_moveGen.SideToMove() == Side.White)
            //{
            //    var promotionPopup = new promotions();
            //    var modalOverlay = new Grid();
            //    promotionPopup.PieceSelected += (sender, pieceType) =>
            //    {
            //        switch (pieceType)
            //        {
            //            case "queen":
            //                newImage.Source = new BitmapImage(new Uri(piece.WhiteQueen, UriKind.Relative));
            //                break;
            //            case "rook":
            //                newImage.Source = new BitmapImage(new Uri(piece.WhiteRook, UriKind.Relative));
            //                break;
            //            case "bishop":
            //                newImage.Source = new BitmapImage(new Uri(piece.WhiteBishop, UriKind.Relative));
            //                break;
            //            case "knight":
            //                newImage.Source = new BitmapImage(new Uri(piece.WhiteKnight, UriKind.Relative));
            //                break;

            //        }
            //        promotionPopup.Close();
            //        _board._grid.Children.Remove(promotionPopup);
            //        _board._grid.Children.Remove(modalOverlay); 

                    

            //    };

            //    modalOverlay.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(128, 0, 0, 0));

            //    // Add the promotion popup to the modal overlay
            //    modalOverlay.Children.Add(promotionPopup);

            //    // Add the modal overlay to your _board._grid or relevant container
            //    _board._grid.Children.Add(modalOverlay);
            //    promotionPopup.ShowDialog();
            //}
            //else
            //{
            //    newImage.Source = new BitmapImage(new Uri(piece.BlackQueen, UriKind.Relative));
                
            //}

            //newImage.MouseDown += HandleMouseDown;
            //newImage.MouseMove += HandleMouseMove;
            //newImage.MouseUp += HandleMouseUp;
            //PlayMoveSound();
            //Grid.SetRow(newImage, targetPosition.X);
            //Grid.SetColumn(newImage, targetPosition.Y);
            //_board._grid.Children.Add(newImage);
            ////Position sourcePosition = new Position(Inrow, Incol);
            //// Update the game state to reflect the move
            //_moveGen.UpdateGame(Inrow * 8 + Incol, targetPosition.X * 8 + targetPosition.Y, 1|Captured);
            ////_gameLogic.UpdateState(sourcePosition, targetPosition);
            //isInCheck = false;
            //CheckGameEnding();
            //RotateBoard();
        
        private void WhitePromotions_PieceSelected(object sender, String thePiece)
        {
            
            _promotionPopup.IsOpen = false;
            ChessPiece piece = new ChessPiece();
            Image newImage = new Image();


            int flag = 1;
            switch (thePiece)
            {
                case "queen":
                    newImage.Source = new BitmapImage(new Uri(piece.WhiteQueen, UriKind.Relative));
                    break;
                case "rook":
                    newImage.Source = new BitmapImage(new Uri(piece.WhiteRook, UriKind.Relative));
                    flag = 5;
                    break;
                case "bishop":
                    newImage.Source = new BitmapImage(new Uri(piece.WhiteBishop, UriKind.Relative));
                    flag = 6;
                    break;
                case "knight":
                    newImage.Source = new BitmapImage(new Uri(piece.WhiteKnight, UriKind.Relative));
                    flag = 7;
                    break;
            }

            newImage.MouseDown += HandleMouseDown;
            newImage.MouseMove += HandleMouseMove;
            newImage.MouseUp += HandleMouseUp;
            PlayMoveSound();
            Grid.SetRow(newImage, _promotion_TargetSqauare.X);
            Grid.SetColumn(newImage, _promotion_TargetSqauare.Y);
            _board._grid.Children.Add(newImage);
            //Position sourcePosition = new Position(Inrow, Incol);
            // Update the game state to reflect the move
            _moveGen.UpdateGame(Inrow * 8 + Incol, _promotion_TargetSqauare.X * 8 + _promotion_TargetSqauare.Y, flag | ((_promotion_Capture)?8:0));
            //_gameLogic.UpdateState(sourcePosition, targetPosition);
            isInCheck = false;
            CheckGameEnding();
            RotateBoard();
            MakeMoveAi();
        }

        private void BlackPromotions_PieceSelected(object sender, String thePiece)
        {
            _promotionPopup.IsOpen = false;
            ChessPiece piece = new ChessPiece();
            Image newImage = new Image();


            int flag = 1;
            switch (thePiece)
            {
                case "queen":
                    newImage.Source = new BitmapImage(new Uri(piece.BlackQueen, UriKind.Relative));
                    break;
                case "rook":
                    newImage.Source = new BitmapImage(new Uri(piece.BlackRook, UriKind.Relative));
                    flag = 5;
                    break;
                case "bishop":
                    newImage.Source = new BitmapImage(new Uri(piece.BlackBishop, UriKind.Relative));
                    flag = 6;
                    break;
                case "knight":
                    newImage.Source = new BitmapImage(new Uri(piece.BlackKnight, UriKind.Relative));
                    flag = 7;
                    break;
            }

            newImage.MouseDown += HandleMouseDown;
            newImage.MouseMove += HandleMouseMove;
            newImage.MouseUp += HandleMouseUp;
            PlayMoveSound();
            Grid.SetRow(newImage, _promotion_TargetSqauare.X);
            Grid.SetColumn(newImage, _promotion_TargetSqauare.Y);
            _board._grid.Children.Add(newImage);
            //Position sourcePosition = new Position(Inrow, Incol);
            // Update the game state to reflect the move
            _moveGen.UpdateGame(Inrow * 8 + Incol, _promotion_TargetSqauare.X * 8 + _promotion_TargetSqauare.Y, flag | ((_promotion_Capture) ? 8 : 0));
            //_gameLogic.UpdateState(sourcePosition, targetPosition);
            isInCheck = false;
            CheckGameEnding();
            RotateBoard();
            MakeMoveAi();
        }

        private void HandleEnPassantCapture(Image sourceImage, Position targetPosition)
        {
            // Handle en passant capture logic
            //Color targetPieceColor = _gameLogic.GetPieceColor(targetRow, targetCol);

            // If there's an opponent's piece at the target position, remove it
            int targetRow = targetPosition.X, targetCol = targetPosition.Y;
            if (_moveGen.SideToMove() == Side.White)
            {
                Image targetImage = FindImageAtPosition(targetRow + 1, targetCol);
                if (targetImage != null)
                {
                    // Remove the target image from the grid
                    _board._grid.Children.Remove(targetImage);
                }
            }
            else
            {
                Image targetImage = FindImageAtPosition(targetRow - 1, targetCol);
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
            _moveGen.UpdateGame(Inrow * 8 + Incol, targetRow * 8 + targetCol, 2|8);
            //_gameLogic.UpdateState(sourcePosition, targetPosition);
            isInCheck = false;
            CheckGameEnding();
            RotateBoard();
            MakeMoveAi();
        }

        private void HandleCastling(Image sourceImage, Position targetPosition)
        {
            // Handle castling logic
            int targetRow = targetPosition.X, targetCol = targetPosition.Y;

            //Handling Movemen and Switching of rooks 
            int sq = targetRow * 8 + targetCol;
            if (sq == (int)Square.g1)
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

            else if (sq == (int)Square.c1)
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
            else if (sq == (int)Square.g8)
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


            
            Grid.SetRow(sourceImage, targetRow);
            Grid.SetColumn(sourceImage, targetCol);
            //_board._grid.Children.Add(newImage);
            //Position sourcePosition = new Position(Inrow, Incol);
            // Update the game state to reflect the move
            _moveGen.UpdateGame(Inrow * 8 + Incol, targetRow * 8 + targetCol, 3);
            //_gameLogic.UpdateState(sourcePosition, targetPosition);
            isInCheck = false;
            CheckGameEnding();
            RotateBoard();
            MakeMoveAi();
        }


        public async Task MakeMoveAi()
        {
            int move = _aiMoveGen.alphabetastuff(_moveGen.currState);
            if (move == 0) { return; }
            int sourceSquare = Codec.get_move_source(move);
            int targetSquare = Codec.get_move_target(move);

            Image sourceImage = FindImageAtPosition(sourceSquare / 8, sourceSquare % 8);
           

            // Calculate the difference in positions
            double xOffset = 75*(targetSquare%8 - sourceSquare%8);
            double yOffset = 75*(targetSquare/8 - sourceSquare/8);

            // Create TranslateTransform for animation
            TranslateTransform translateTransform = new TranslateTransform();
            sourceImage.RenderTransform = translateTransform;

            // Animate the movement
            DoubleAnimation xAnimation = new DoubleAnimation
            {
                To = xOffset,
                Duration = TimeSpan.FromSeconds(0.2), // Animation duration
                FillBehavior = FillBehavior.HoldEnd // Keep the final position
            };

            DoubleAnimation yAnimation = new DoubleAnimation
            {
                To = yOffset,
                Duration = TimeSpan.FromSeconds(0.2), // Animation duration
                FillBehavior = FillBehavior.HoldEnd // Keep the final position
            };

            translateTransform.BeginAnimation(TranslateTransform.XProperty, xAnimation);
            translateTransform.BeginAnimation(TranslateTransform.YProperty, yAnimation);

            // Wait for the animation to complete
            await Task.Delay(200); // Adjust as needed
            // Reset the transform
            sourceImage.RenderTransform = null;
            // Set the source image to the target cell
            _board._grid.Children.Remove(sourceImage);

            CombinedPiece promoted = Codec.get_move_promoted(move);


            //Handle for capturing the target piece
            if (Codec.get_move_capture(move) == 1 && Codec.get_move_enpassant(move)==1)
            {
                Image targetImage;
                if (_moveGen.SideToMove() == Side.White)
                {
                    targetImage = FindImageAtPosition((targetSquare / 8)+1, targetSquare % 8);
                }
                else
                {
                    targetImage = FindImageAtPosition((targetSquare / 8) - 1, targetSquare % 8);
                }
                if (targetImage != null)
                {
                    _board._grid.Children.Remove(targetImage);
                }
                
            }
            //Handle for just captures 
            else if (Codec.get_move_capture(move) == 1)
            {
                Image targetImage = FindImageAtPosition(targetSquare / 8, targetSquare % 8);
                if (targetImage != null)
                {
                    _board._grid.Children.Remove(targetImage);
                }
            }

            //Handle for promotion piece 
            if (promoted != CombinedPiece.None)
            {
                ChessPiece piece = new ChessPiece();
                Image newImage = new Image();

                switch (promoted)
                {
                    case CombinedPiece.WhiteQueen:
                        newImage.Source = new BitmapImage(new Uri(piece.WhiteQueen, UriKind.Relative));
                        break;
                    case CombinedPiece.BlackQueen:
                        newImage.Source = new BitmapImage(new Uri(piece.BlackQueen, UriKind.Relative));
                        break;
                    case CombinedPiece.WhiteRook:
                        newImage.Source = new BitmapImage(new Uri(piece.WhiteRook, UriKind.Relative));
                        break;
                    case CombinedPiece.BlackRook:
                        newImage.Source = new BitmapImage(new Uri(piece.BlackRook, UriKind.Relative));
                        break;
                    case CombinedPiece.WhiteBishop:
                        newImage.Source = new BitmapImage(new Uri(piece.WhiteBishop, UriKind.Relative));
                        break;
                    case CombinedPiece.BlackBishop:
                        newImage.Source = new BitmapImage(new Uri(piece.BlackBishop, UriKind.Relative));
                        break;
                    case CombinedPiece.WhiteKnight:
                        newImage.Source = new BitmapImage(new Uri(piece.WhiteKnight, UriKind.Relative));
                        break;
                    case CombinedPiece.BlackKnight:
                        newImage.Source = new BitmapImage(new Uri(piece.BlackKnight, UriKind.Relative));
                        break;
                }
                newImage.MouseDown += HandleMouseDown;
                newImage.MouseMove += HandleMouseMove;
                newImage.MouseUp += HandleMouseUp;
                //PlayMoveSound();
                Grid.SetRow(newImage, targetSquare/8);
                Grid.SetColumn(newImage, targetSquare%8);
                _board._grid.Children.Add(newImage);
            }
            else
            {
                _board._grid.Children.Add(sourceImage);
                Grid.SetRow(sourceImage, targetSquare / 8);
                Grid.SetColumn(sourceImage, targetSquare % 8);
            }

            if (Codec.get_move_castle(move) == 1)
            {
                Debug.Write("Correctly reached here but still not working ");
                Image RookImage = new Image();
                switch (targetSquare)
                {
                    case ((int)Square.g1):
                        RookImage = FindImageAtPosition((int)Square.h1 / 8,(int)Square.h1 % 8);
                        _board._grid.Children.Remove(RookImage);
                        _board._grid.Children.Add(RookImage);
                        Grid.SetRow(RookImage, (int)Square.f1 / 8);
                        Grid.SetColumn(RookImage, (int)Square.f1 % 8);
                        _board._grid.Children.Add(RookImage);
                        break;
                    case ((int)Square.c1):
                        RookImage = FindImageAtPosition((int)Square.a1 / 8,(int)Square.a1 % 8);
                        _board._grid.Children.Remove(RookImage);
                        _board._grid.Children.Add(RookImage);
                        Grid.SetRow(RookImage, (int)Square.d1 / 8);
                        Grid.SetColumn(RookImage, (int)Square.d1 % 8);
                        
                        break;
                    case ((int)Square.g8):
                        RookImage = FindImageAtPosition((int)Square.h8 / 8,(int)Square.h8 % 8);
                        _board._grid.Children.Remove(RookImage);
                        _board._grid.Children.Add(RookImage);
                        Grid.SetRow(RookImage, (int)Square.f8 / 8);
                        Grid.SetColumn(RookImage, (int)Square.f8 % 8);
                        
                        break;
                    case ((int)Square.c8):
                        RookImage = FindImageAtPosition((int)Square.a8 / 8,(int)Square.a8 % 8);
                        _board._grid.Children.Remove(RookImage);
                        _board._grid.Children.Add(RookImage);
                        Grid.SetRow(RookImage, (int)Square.f1 / 8);
                        Grid.SetColumn(RookImage, (int)Square.f1 % 8);
                        break;
                }

            }

            //first update the move in the backend 
            _moveGen.UpdateGame(move,0, 32);
            //then redraw squares after checkgameend of course
            isInCheck = false;
            CheckGameEnding();
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

        private void RotateBoard()
        {
            if (wantToRotate)
            {   
                RotateTransform rotateTransform = new RotateTransform((isBoardRotated)?0:180);
                _board._grid.RenderTransform = rotateTransform;

                _board._grid.RenderTransformOrigin = new Point(0.5, 0.5);
                isBoardRotated = !isBoardRotated;

                foreach (UIElement child in _board._grid.Children)
                {
                    if (child is Image image)
                    {
                        double offsetX = 0;
                        double offsetY = 0;

                        if (isBoardRotated)
                        {
                            // Adjust the position of the image when rotated
                            double centerX = 75;
                            double centerY = 75;
                            offsetX = centerX;
                            offsetY = centerY;
                        }

                        TranslateTransform translateTransform = new TranslateTransform(offsetX, offsetY);

                        TransformGroup transformGroup = new TransformGroup();
                        transformGroup.Children.Add(rotateTransform);
                        transformGroup.Children.Add(translateTransform);
                        //_draggingTransform = new TransformGroup();
                        //_draggingTransform = transformGroup;
                        image.RenderTransform = transformGroup;
                    }
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
                    return;
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

        private async Task PlaySoundForDurationAsync(MediaPlayer player, TimeSpan duration)
        {
            if (player != null)
            {
                player.Play();

                await Task.Delay(duration);

                player.Pause();
                player.Position = TimeSpan.Zero; // Reset position to the beginning
            }
        }


        private async void PlayCaptureSound()
        {
            await PlaySoundForDurationAsync(captureAudioPlayer, TimeSpan.FromSeconds(1));
        }

        private async void PlayMoveSound()
        {
            await PlaySoundForDurationAsync(moveAudioPlayer, TimeSpan.FromSeconds(1));
        }



        private void ReloadCaptureSound()
        {
            if (captureAudioPlayer != null)
            {
                captureAudioPlayer.Stop();
                captureAudioPlayer.Position = TimeSpan.Zero;
            }
        }

        private void ReloadMoveSound()
        {
            if (moveAudioPlayer != null)
            {
                moveAudioPlayer.Stop();
                moveAudioPlayer.Position = TimeSpan.Zero;
                
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