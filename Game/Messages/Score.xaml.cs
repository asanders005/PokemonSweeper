using PokemonSweeper.Game;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace PokemonSweeper.Game.Messages
{
    /// <summary>
    ///     Interaction logic for Score.xaml
    /// </summary>
    public partial class Score : Window
    {
        public Score()
        {
            InitializeComponent();
        }

        public static void ShowScore(GameWindow sender, PokemonSweeper.Field Field)
        {
            Field.Timer.Stop();
            var PokeList = new List<PlayerPokemon>();
            var Winner = new Score();

            foreach (var square in Field.Squares.Where(s => s.Pokemon != null))
            {
                Winner.ListBoxPokemon.Items.Add(square.Pokemon);
                PokeList.Add(square.Pokemon);
            }
            var newScore = sender.Game.CalculateNewScore(Field.Timer, Field.NrOfClicks, PokeList);
            Winner.score.Text = "Good job! You caught all the Pokemon!! Your score is " + newScore;
            Winner.Owner = sender;
            Winner.ShowDialog();
        }

        private async void Next_Click(object sender, RoutedEventArgs e)
        {
            var OwnerWindow = ((GameWindow) Owner);
            await OwnerWindow.Game.NewField(OwnerWindow);
            Close();
        }
    }
}