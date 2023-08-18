using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace myChess
{
    public class Board
    {
        private Grid _grid;
        private SolidColorBrush _lightSquareColor;
        private SolidColorBrush _darkSquareColor;

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
        }


    }
}
