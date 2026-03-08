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
        
        PokemonImage.Source = new BitmapImage(new System.Uri(p.DefaultSprite));;
        
        PokemonType.Content = p.PrimaryType+" Type";
        if (p.SecondaryType != null) PokemonType.Content += ", "+p.SecondaryType+" Type";

        PokemonHP.Content = "HP: " + p.BaseStats[PokemonStatsType.HP];
        PokemonAttack.Content = "Attack: " + p.BaseStats[PokemonStatsType.Attack];
        PokemonDefense.Content = "Defense: " + p.BaseStats[PokemonStatsType.Defense];
        PokemonSpAttack.Content = "Sp. Attack: " + p.BaseStats[PokemonStatsType.SpecialAttack];
        PokemonSpDefense.Content = "Sp. Defense: " + p.BaseStats[PokemonStatsType.SpecialDefense];
        PokemonSpeed.Content = "Speed: " + p.BaseStats[PokemonStatsType.Speed];
    }

    private void BackClick(object sender, RoutedEventArgs e)
    {
        Close();
    }
    
    private void WindowLoaded(object sender, RoutedEventArgs e)
    {
        SetPokemon();
    }
}