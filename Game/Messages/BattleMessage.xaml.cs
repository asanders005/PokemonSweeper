using PokemonSweeper.Game.PokemonModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace PokemonSweeper.Game.Messages
{
    public partial class BattleMessage : Window
    {
        public BattleMessage()
        {
            InitializeComponent();
        }

        public static void ShowMessage(GameWindow window, PlayerPokemon wildPokemon, List<PlayerPokemon> faintedPokemon)
        {
            var result = new BattleMessage();
            result.encounter.Text = $"A wild {wildPokemon.Pokemon.Name} appeared!";
            result.battle.Text = $"Your team defeated the wild {wildPokemon.Pokemon.Name}! \n Pokemon fainted during battle:";
            foreach (var pokemon in faintedPokemon)
            {
                result.ListBoxPokemon.Items.Add(pokemon);
            }

            result.Owner = window;
            result.ShowDialog();
        }

        private void Menu_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
