using PokemonSweeper.Data;
using PokemonSweeper.Game.Field;
using PokemonSweeper.Game.PokemonModels;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace PokemonSweeper.Game.Messages
{
    /// <summary>
    ///     Interaction logic for FailMessage.xaml
    /// </summary>
    public partial class FailMessage : Window
    {
        private DAL _dal;

        public FailMessage()
        {
            InitializeComponent();
        }

        public static async void ShowMessage(GameWindow window, PlayerPokemon pokemon, DAL dal)
        {
            var Fail = new FailMessage();
            Fail._dal = dal;
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
            MainMenu main = new MainMenu(_dal);
            main.Show();

            Owner.Close();
            Close();
        }
    }
}