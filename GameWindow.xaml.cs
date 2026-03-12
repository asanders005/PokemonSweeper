using PokemonSweeper.Data;
using PokemonSweeper.Game;
using PokemonSweeper.Game.Field;
using PokemonSweeper.Services;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace PokemonSweeper
{
    public partial class GameWindow : Window
    {
        private readonly IDal _dal;
        private readonly PokemonTeamService _pokemonTeamService;

        public GameWindow(IDal dal, PokemonTeamService teamService, string value)
        {
            InitializeComponent();
            _dal = dal;
            _pokemonTeamService = teamService;

            Game = new PokeSweepGame(dal, teamService, value);
        }

        public PokeSweepGame Game { get; set; }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await Game.NewField(this);
        }

        public void MineSquare_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            ((Square)sender).RightButton(this);
        }

        public void MineSquare_Click(object sender, RoutedEventArgs e)
        {
            ((Square)sender).LeftButton(this);
        }

        public void MinesLeftLabel(int count)
        {
            MinesLeft.Content = "Pokeballs: " + count;
        }

        private void MenuButton_Click(object sender, RoutedEventArgs e)
        {
            MainMenu main = new MainMenu(_dal, _pokemonTeamService);
            main.Show();

            Close();
        }
    }
}