﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
            board.InitializeBoard();
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
