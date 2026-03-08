using PokemonSweeper.Game.PokemonModels;
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
            Fail.Message.Text = $"A wild {pokemon.Pokemon.Name} Appeared! \n The {pokemon.Pokemon.Name} defeated your Pokemon! \n You whited out!";
            Fail.Title = "Game over!";
            Fail.Owner = window;
            Fail.ShowDialog();
            await window.Game.NewField(window);
        }

        private void retry_Click(object sender, RoutedEventArgs e)
        {
            // Calling Code automatically resets the game, so we just need to close the message box
            Close();
        }

        private void MainMenu_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Navigate back to the main menu
            // might need to make the method async
            Close();
        }
    }
}