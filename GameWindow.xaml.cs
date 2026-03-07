using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using PokemonSweeper.Data;
using PokemonSweeper.Game;
using PokemonSweeper.Services;

namespace PokemonSweeper
{
    public partial class GameWindow : Window
    {
        public GameWindow(DAL dal, PokemonTeamService teamService)
        {
            InitializeComponent();
            Game = new PokeSweepGame(dal, teamService);
        }

        public PokeSweepGame Game { get; set; }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await Game.NewField(this);
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
    }
}