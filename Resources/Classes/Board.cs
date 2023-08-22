using myChess.Resources.Classes;
using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace myChess
{
    public class Board
    {
        public Grid _grid;
        public SolidColorBrush _lightSquareColor;
        public SolidColorBrush _darkSquareColor;
        public string StartingFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR";

        public Board(Grid grid, SolidColorBrush lightSquareBrush, SolidColorBrush darkColorBrush)
        {
            _grid = grid;

            _lightSquareColor = lightSquareBrush;
            _darkSquareColor = darkColorBrush;
        }

        public void InitializeBoard()
        {
            _grid.Children.Clear();
            SolidColorBrush lightSquareBrush = _lightSquareColor;
            SolidColorBrush darkSquareBrush = _darkSquareColor;

            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    Rectangle square = new Rectangle();
                    square.Fill = (row + col) % 2 == 0 ? lightSquareBrush : darkSquareBrush;

                    Grid.SetRow(square, row);
                    Grid.SetColumn(square, col);

                    square.Width = 75; // Set width to Auto
                    square.Height = 75; // Set height to Auto

                    _grid.Children.Add(square);
                }
            }

            SetStartingPos();
        }

        public void SetStartingPos()
        {
            SetPositionOnFen(StartingFen);
        }

        public void SetPositionOnFen(String FenString)
        {
            int row = 0, col = 0;

            foreach (char ch in FenString)
            {
                if (ch == '/')
                {
                    row++; // Move to the next row
                    col = 0; // Reset column index
                }
                else if (char.IsDigit(ch))
                {
                    int emptySquares = int.Parse(ch.ToString());
                    col += emptySquares; // Move forward by the number of empty squares
                }
                else
                {
                    ChessPiece piece = new ChessPiece();
                    Image image = new Image();

                    switch (ch)
                    {
                        case 'r':
                            image.Source = new BitmapImage(new Uri(piece.BlackRook, UriKind.Relative));
                            break;
                        case 'n':
                            image.Source = new BitmapImage(new Uri(piece.BlackKnight, UriKind.Relative));
                            break;
                        case 'b':
                            image.Source = new BitmapImage(new Uri(piece.BlackBishop, UriKind.Relative));
                            break;
                        case 'q':
                            image.Source = new BitmapImage(new Uri(piece.BlackQueen, UriKind.Relative));
                            break;
                        case 'k':
                            image.Source = new BitmapImage(new Uri(piece.BlackKing, UriKind.Relative));
                            break;
                        case 'p':
                            image.Source = new BitmapImage(new Uri(piece.BlackPawn, UriKind.Relative));
                            break;
                        case 'R':
                            image.Source = new BitmapImage(new Uri(piece.WhiteRook, UriKind.Relative));
                            break;
                        case 'N':
                            image.Source = new BitmapImage(new Uri(piece.WhiteKnight, UriKind.Relative));
                            break;
                        case 'B':
                            image.Source = new BitmapImage(new Uri(piece.WhiteBishop, UriKind.Relative));
                            break;
                        case 'Q':
                            image.Source = new BitmapImage(new Uri(piece.WhiteQueen, UriKind.Relative));
                            break;
                        case 'K':
                            image.Source = new BitmapImage(new Uri(piece.WhiteKing, UriKind.Relative));
                            break;
                        case 'P':
                            image.Source = new BitmapImage(new Uri(piece.WhitePawn, UriKind.Relative));
                            break;

                        default:
                            break;
                    }

                    Grid.SetRow(image, row);
                    Grid.SetColumn(image, col);

                    _grid.Children.Add(image);

                    col++; // Move to the next column
                }
            }
        }


    }
}

//Image image = new Image();
//image.Source = new BitmapImage(new Uri(piece.BlackBishop, UriKind.Relative));

//Grid.SetRow(image, row);
//Grid.SetColumn(image, col);
//_grid.Children.Add(image);