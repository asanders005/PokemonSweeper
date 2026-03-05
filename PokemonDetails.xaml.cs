using System.Windows;
using System.Windows.Media.Imaging;
using PokemonSweeper.Data;
using PokemonSweeper.Game;
using PokemonSweeper.Game.PokemonModels;

namespace PokemonSweeper;

public partial class PokemonDetails : Window
{
    public PlayerPokemon CurrentPlayerPokemon { get; set; }
    public DAL Dal { get; set; }
    public PokemonDetails(DAL dal)
    {
        InitializeComponent();
        Dal = dal;
    }

    private async void SetPokemon()
    {
        CurrentPlayerPokemon = await PlayerPokemon.CreateWithRandomStats(Dal);
        var p = CurrentPlayerPokemon.Pokemon;
        
        PokemonName.Content = char.ToUpper(p.Name[0]) + p.Name[1..];
        
        PokemonType.Content = CurrentPlayerPokemon.Pokemon.PrimaryType;
        if (CurrentPlayerPokemon.Pokemon.SecondaryType != null) PokemonType.Content += ", "+CurrentPlayerPokemon.Pokemon.SecondaryType;

        PokemonImage.Source = new BitmapImage(new System.Uri(p.DefaultSprite));;
    }
    
    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        SetPokemon();
    }
}