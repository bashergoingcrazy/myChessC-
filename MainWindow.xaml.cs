using myChess.Resources.Classes;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;

namespace myChess
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            PreviewKeyDown += MainWindow_PreviewKeyDown;

            SolidColorBrush lightSquareBrush = (SolidColorBrush)FindResource("lightSqareColor");
            SolidColorBrush darkSquareBrush = (SolidColorBrush)FindResource("darkSquareColor");
            Board board = new Board(ChessBoard, lightSquareBrush, darkSquareBrush);
            BoardUI ui = new BoardUI(board);
        }

        private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
                Application.Current.Shutdown();
            }

        }
        //public MainWindow()
        //{
        //    InitializeComponent();
        //    PreviewKeyDown += MainWindow_PreviewKeyDown;

        //    BitMoveGen MoveGenerator = new BitMoveGen();

        //}

        //private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.Key == Key.Escape)
        //    {
        //        Close();
        //        Application.Current.Shutdown();
        //    }

        //}
    }
}
