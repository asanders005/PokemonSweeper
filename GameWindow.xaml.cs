using System.Windows;
using System.Windows.Input;
using PokemonSweeper.Data;
using PokemonSweeper.Game;

namespace PokemonSweeper
{
    public partial class GameWindow : Window
    {
        public GameWindow(DAL dal)
        {
            InitializeComponent();
            _dal = dal;
            Game = new PokeSweepGame(_dal);
        }

        public PokeSweepGame Game { get; set; }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Game.NewField(this);
        }

        public void MineSquare_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            ((Square) sender).RightButton(this);
        }

        public void MineSquare_Click(object sender, RoutedEventArgs e)
        {
            ((Square) sender).LeftButton(this);
        }

        public void MinesLeftLabel(int count)
        {
            MinesLeft.Content = "Pokeballs: " + count;
        }

        private readonly DAL _dal;
    }
}