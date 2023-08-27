using System;
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

namespace myChess.Resources.UserControls
{
    /// <summary>
    /// Interaction logic for blackPromotions.xaml
    /// </summary>
    public partial class blackPromotions : UserControl
    {
        public event EventHandler<string> PieceSelected;
        public blackPromotions()
        {
            InitializeComponent();
        }
        private void QueenButton_Click(object sender, RoutedEventArgs e)
        {
            PieceSelected?.Invoke(this, "queen");
        }

        private void RookButton_Click(object sender, RoutedEventArgs e)
        {
            PieceSelected?.Invoke(this, "rook");
        }

        private void BishopButton_Click(object sender, RoutedEventArgs e)
        {
            PieceSelected?.Invoke(this, "bishop");
        }

        private void KnightButton_Click(object sender, RoutedEventArgs e)
        {
            PieceSelected?.Invoke(this, "knight");
        }
    }
}
