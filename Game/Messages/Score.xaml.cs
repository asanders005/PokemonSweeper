using PokemonSweeper.Data;
using PokemonSweeper.Game.Field;
using PokemonSweeper.Game.PokemonModels;
using PokemonSweeper.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace PokemonSweeper.Game.Messages
{
    /// <summary>
    ///     Interaction logic for Score.xaml
    /// </summary>
    public partial class Score : Window
    {
        private int _maxSelectablePokemon = 2;
        private DAL _dal;
        private PokemonTeam _team;

        public Score(PokemonTeam pokemonTeam, DAL dal)
        {
            _dal = dal;
            _team = pokemonTeam;
            InitializeComponent();
        }

        public static async void ShowScore(GameWindow sender, PokemonSweeper.Field Field, PokemonTeam pokemonTeam, DAL dal)
        {
            Field.Timer.Stop();
            var PokeList = new List<PlayerPokemon>();
            var Winner = new Score(pokemonTeam, dal);

            foreach (var square in Field.Squares.Where(s => s.Pokemon != null && !s.Pokemon.IsFainted))
            {
                Winner.ListBoxPokemon.Items.Add(square.Pokemon);
                PokeList.Add(square.Pokemon);
            }

            int expGain = pokemonTeam.AwardExpToTeam(PokeList);

            await pokemonTeam.SaveTeam();

            var newScore = sender.Game.CalculateNewScore(Field.Timer, Field.NrOfClicks, PokeList);
            Winner.score.Text = "Good job! You caught all the Pokemon!! Your Non-fainted Pokemon have earned " + expGain + " experience points each! Your score is " + newScore;

            Winner._maxSelectablePokemon = sender.Game.Level switch
            {
                0 => 2,
                1 => 3,
                2 => 4,
                _ => Winner._maxSelectablePokemon
            };
            Winner.SelectionInfo.Text = $"Select up to {Winner._maxSelectablePokemon} Pokemon to save to your PC.";

            Winner.Owner = sender;
            Winner.ShowDialog();
        }

        private void ListBoxPokemon_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (ListBoxPokemon.SelectedItems.Count > _maxSelectablePokemon)
            {
                foreach (var item in e.AddedItems)
                {
                    ListBoxPokemon.SelectedItems.Remove(item);
                }

                MessageBox.Show($"You can only select {_maxSelectablePokemon} Pokemon to save!", "Selection limit", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            SelectionInfo.Text = $"Selected {ListBoxPokemon.SelectedItems.Count} / {_maxSelectablePokemon} Pokemon.";
        }

        private async void Retry_Click(object sender, RoutedEventArgs e)
        {
            await SavePokemon();

            var OwnerWindow = Owner as GameWindow;
            await OwnerWindow.Game.NewField(OwnerWindow);

            Close();
        }

        private async void Next_Click(object sender, RoutedEventArgs e)
        {
            await SavePokemon();
            MainMenu main=new MainMenu(_dal,_team);
            main.Show();
            Close();
        }

        private async Task SavePokemon()
        {
            var selectedPokemon = ListBoxPokemon.SelectedItems.Cast<PlayerPokemon>().ToList();
            await _dal.SavePlayerPokemonAsync(selectedPokemon);
        }
    }
}