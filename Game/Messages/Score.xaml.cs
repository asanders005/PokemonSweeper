using PokemonSweeper.Data;
using PokemonSweeper.Game.PokemonModels;
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

        public Score(DAL dal)
        {
            _dal = dal;
            InitializeComponent();
        }

        public static void ShowScore(GameWindow sender, PokemonSweeper.Field Field, DAL dal)
        {
            Field.Timer.Stop();
            var PokeList = new List<PlayerPokemon>();
            var Winner = new Score(dal);

            foreach (var square in Field.Squares.Where(s => s.Pokemon != null))
            {
                Winner.ListBoxPokemon.Items.Add(square.Pokemon);
                PokeList.Add(square.Pokemon);
            }
            var newScore = sender.Game.CalculateNewScore(Field.Timer, Field.NrOfClicks, PokeList);
            Winner.score.Text = "Good job! You caught all the Pokemon!! Your score is " + newScore;

            Winner._maxSelectablePokemon = sender.Game.Level switch
            {
                1 => 2,
                2 => 3,
                3 => 4,
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

            // TODO: Return to main menu
            Close();
        }

        private async Task SavePokemon()
        {
            var selectedPokemon = ListBoxPokemon.SelectedItems.Cast<PlayerPokemon>().ToList();
            await _dal.SavePlayerPokemonAsync(selectedPokemon);
        }
    }
}