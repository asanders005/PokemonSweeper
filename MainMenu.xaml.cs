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
        Data.DAL dal= new Data.DAL();
        Game.PokemonModels.PokemonTeam team = null;
        public MainMenu()
        {
            InitializeComponent();
        }
        public MainMenu(DAL dal, Game.PokemonModels.PokemonTeam team)
        {
            InitializeComponent();
            this.dal = dal;
            this.team = team;
        }

        private void Easy_Click(object sender, RoutedEventArgs e)
            {
            
            GameWindow game =new GameWindow(dal, new Services.PokemonTeamService(dal),"easy");
            game.Show();
            this.Close();
            }

            private void Meduim_Click(object sender, RoutedEventArgs e)
            {
            Data.DAL dal = new Data.DAL();
            GameWindow game = new GameWindow(dal, new Services.PokemonTeamService(dal), "intermediate");
            game.Show();
            this.Close();
            }

        private void Hard_Click(object sender, RoutedEventArgs e)
        {
            Data.DAL dal = new Data.DAL();
            GameWindow game = new GameWindow(dal, new Services.PokemonTeamService(dal), "hard");
            game.Show();
            this.Close();
        }

        private void Box_Click(object sender, RoutedEventArgs e)
        {
            PokemonPC pc = new PokemonPC(dal);
            pc.Show();
        }
    }
    }

