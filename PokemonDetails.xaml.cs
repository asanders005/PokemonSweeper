#nullable enable
using System.Windows;
using System.Windows.Media.Imaging;
using PokemonSweeper.Data;
using PokemonSweeper.Game;
using PokemonSweeper.Game.PokemonModels;

namespace PokemonSweeper;

public partial class PokemonDetails : Window
{
    public PlayerPokemon? CurrentPlayerPokemon { get; set; }
    public IDal Dal { get; set; }
    private PokemonPC? PCWindow { get; set; }
    private bool editMode;
    public PokemonDetails(IDal dal, PlayerPokemon? pokemon = null, PokemonPC? pcwindow = null)
    {
        InitializeComponent();
        Dal = dal;
        if (pokemon != null) CurrentPlayerPokemon = pokemon;
        if (pcwindow != null) PCWindow = pcwindow;
    }

    private async void SetPokemon()
    {
        CurrentPlayerPokemon ??= await PlayerPokemon.CreateWithRandomStats(Dal);
        var p = CurrentPlayerPokemon.Pokemon;
        
        PokemonName.Content = char.ToUpper(p.Name[0]) + p.Name[1..];
        
        PokemonImage.Source = new BitmapImage(new System.Uri(p.DefaultSprite));
        
        PokemonLevel.Content = "Level "+CurrentPlayerPokemon.Level;
        
        PokemonType.Content = p.PrimaryType+" Type";
        if (p.SecondaryType != null) PokemonType.Content += ", "+p.SecondaryType+" Type";

        PokemonHP.Content = "HP: " + p.BaseStats[PokemonStatsType.HP];
        PokemonAttack.Content = "Attack: " + p.BaseStats[PokemonStatsType.Attack];
        PokemonDefense.Content = "Defense: " + p.BaseStats[PokemonStatsType.Defense];
        PokemonSpAttack.Content = "Sp. Attack: " + p.BaseStats[PokemonStatsType.SpecialAttack];
        PokemonSpDefense.Content = "Sp. Defense: " + p.BaseStats[PokemonStatsType.SpecialDefense];
        PokemonSpeed.Content = "Speed: " + p.BaseStats[PokemonStatsType.Speed];
    }

    private void EditButton(object sender, RoutedEventArgs e)
    {
        var p = CurrentPlayerPokemon?.Pokemon ?? null;
        if (p == null)
            return;
        
        if (!editMode)
        {
            editMode = true;
            EditPokemonName.Content = "Save";
            PokemonNameEntry.Text = char.ToUpper(p.Name[0]) + p.Name[1..];
            PokemonNameEntry.Visibility =  Visibility.Visible;
            PokemonName.Visibility = Visibility.Hidden;
        }
        else
        {
            editMode = false;
            EditPokemonName.Content = "Edit";
            p.Name = PokemonNameEntry.Text;
            PokemonName.Content = char.ToUpper(p.Name[0]) + p.Name[1..];
            PokemonNameEntry.Visibility =  Visibility.Visible;
            PokemonName.Visibility = Visibility.Hidden;
        }
    }

    private async void BackClick(object sender, RoutedEventArgs e)
    {
        await Dal.SavePlayerPokemonAsync(CurrentPlayerPokemon);
        if (PCWindow != null) await PCWindow.Refresh();
        Close();
    }
    
    private void WindowLoaded(object sender, RoutedEventArgs e)
    {
        SetPokemon();
    }
}