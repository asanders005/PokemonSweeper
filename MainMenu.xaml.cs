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
using System.Windows.Shapes;
using PokemonSweeper.Data;
using PokemonSweeper.Services;

namespace PokemonSweeper.Game.Field
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class MainMenu : Window
    {
        private readonly IDal dal;
        private readonly PokemonTeamService _pokemonTeamService;
        public MainMenu(IDal dal, PokemonTeamService pokemonTeamService)
        {
            InitializeComponent();
            this.dal = dal;
            _pokemonTeamService = pokemonTeamService;
        }

        private void Easy_Click(object sender, RoutedEventArgs e)
        {

            GameWindow game = new GameWindow(dal, _pokemonTeamService, "easy");
            game.Show();
            this.Close();
        }

        private void Medium_Click(object sender, RoutedEventArgs e)
        {
            GameWindow game = new GameWindow(dal, _pokemonTeamService, "intermediate");
            game.Show();
            this.Close();
        }

        private void Hard_Click(object sender, RoutedEventArgs e)
        {
            GameWindow game = new GameWindow(dal, _pokemonTeamService, "hard");
            game.Show();
            this.Close();
        }

        private void Box_Click(object sender, RoutedEventArgs e)
        {
            PokemonPC pc = new PokemonPC(dal, _pokemonTeamService);
            pc.Show();
        }
    }
}

