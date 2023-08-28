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
        BoardUI ui;
        //public MainWindow()
        //{
        //    InitializeComponent();
        //    PreviewKeyDown += MainWindow_PreviewKeyDown;

        //    SolidColorBrush lightSquareBrush = (SolidColorBrush)FindResource("lightSqareColor");
        //    SolidColorBrush darkSquareBrush = (SolidColorBrush)FindResource("darkSquareColor");
        //    Board board = new Board(ChessBoard, lightSquareBrush, darkSquareBrush);
        //    ui = new BoardUI(board);
        //}

        //private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.Key == Key.Escape)
        //    {
        //        Close();
        //        Application.Current.Shutdown();
        //    }
        //}
        private async void MakeMoveButton_Click(object sender, RoutedEventArgs e)
        {
            // Call the MakeMoveAi function on your BoardUi instance
            await ui.MakeMoveAi();
        }




        public MainWindow()
        {
            InitializeComponent();
            PreviewKeyDown += MainWindow_PreviewKeyDown;

            AiMoveGen MoveGenerator = new AiMoveGen();

        }

        private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
                Application.Current.Shutdown();
            }

        }
    }
}
