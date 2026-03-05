using PokemonSweeper.Game;
using System.Windows;
using System.Windows.Media.Imaging;

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

        public static async void ShowMessage(GameWindow window, PlayerPokemon pokemon)
        {
            var Fail = new FailMessage();
            Fail.EscapedPokemon.Source = new BitmapImage(new System.Uri(pokemon.SpriteUrl));
            Fail.Message.Text = pokemon.Pokemon.DexNum + " - " + pokemon.Pokemon.Name + " managed to escape!";
            Fail.Title = "Game over!";
            Fail.Owner = window;
            Fail.ShowDialog();
            await window.Game.NewField(window);
        }

        private void retry_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}