using PokemonSweeper.Game;
using System.Windows;

namespace PokemonSweeper.Game.Messages
{
    /// <summary>
    ///     Interaction logic for FailMessage.xaml
    /// </summary>
    public partial class FailMessage : Window
    {
        public FailMessage()
        {
            InitializeComponent();
        }

        public static void ShowMessage(GameWindow window, Pokemon pokemon)
        {
            var Fail = new FailMessage();
            //Fail.EscapedPokemon.Source = pokemon.Picture;
            Fail.Message.Text = pokemon.DexNum + " - " + pokemon.Name + " managed to escape!";
            Fail.Title = "Game over!";
            Fail.Owner = window;
            Fail.ShowDialog();
            window.Game.NewField(window);
        }

        private void retry_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}