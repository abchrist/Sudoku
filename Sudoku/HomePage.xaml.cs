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
using Microsoft.Win32;

namespace Sudoku
{
    /// <summary>
    /// Interaction logic for HomePage.xaml
    /// </summary>
    public partial class HomePage : Page
    {
        public HomePage()
        {
            InitializeComponent();
        }

        private void NewGame_Button_Click(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show("You clicked New Game!");
            Page myMainGame = new MainGame("New");
            Window.GetWindow(this).Content = myMainGame;
        }

        private void Load_Button_Click(object sender, RoutedEventArgs e) //TODO: Implement Load button
        {
            //MessageBox.Show("You clicked New Game!");
            Page myMainGame = new MainGame("Load");
            Window.GetWindow(this).Content = myMainGame;
        }
    }
}
