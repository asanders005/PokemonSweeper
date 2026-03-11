using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PokemonSweeper.Data;
using PokemonSweeper.Game.PokemonModels;
using PokemonSweeper.Services;

namespace PokemonSweeper;

public partial class PokemonPC : Window
{
    public DAL Dal { get; set; }
    private List<PlayerPokemon> PCPokemon { get; set; } = new();
    private PokemonTeamService _pokemonTeamService;

    public PokemonPC(DAL dal, PokemonTeamService pokemonTeamService)
    {
        InitializeComponent();
        Dal = dal;
        _pokemonTeamService = pokemonTeamService;
    }

    private async void WindowLoaded(object sender, RoutedEventArgs e)
    {
        _pokemonTeamService.CurrentTeam ??= await Dal.LoadPokemonTeamAsync();

        LoadTeamPokemon();
        
        LoadPcPokemon();
    }

    private async void LoadTeamPokemon()
    {
        PokemonTeamGrid.Children.Clear();
        
        for (int i = 0; i < 6; i++)
        {
            var pokemon = _pokemonTeamService.CurrentTeam.Pokemon[i];
            DockPanel grid = new();
            Button button = new()
            {
                Content = grid
            };
            string pokemonname = pokemon != null ? char.ToUpper(pokemon.Pokemon.Name[0]) + pokemon.Pokemon.Name[1..] : "None";
            ImageSource pokemonimage = pokemon != null ?  new BitmapImage(new Uri(pokemon.Pokemon.DefaultSprite)) : null;

            Label label = new()
            {
                Content = pokemonname,
                HorizontalAlignment = HorizontalAlignment.Right,
                FontSize = 20,
                VerticalAlignment = VerticalAlignment.Center
            };
            Image image = new()
            {
                Source = pokemonimage,
                HorizontalAlignment = HorizontalAlignment.Left,
                Width = 50
            };

            void DetailsOpen()
            {
                PokemonDetails details = new PokemonDetails(Dal)
                {
                    CurrentPlayerPokemon = pokemon
                };
                RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.NearestNeighbor);
                details.Show();
            }

            var i1 = i;
            void AddToBox()
            {
                _pokemonTeamService.CurrentTeam.Pokemon[i1] = new PlayerPokemon();
                LoadTeamPokemon();
                LoadPcPokemon();
            }

            if (pokemon != null) button.Click += (_, _) => DetailsOpen();
            if (pokemon != null) button.MouseRightButtonDown += (_, _) => AddToBox();
            
            Grid.SetColumn(button, i%3);
            Grid.SetRow(button, i/3);
            
            grid.Children.Add(image);
            DockPanel.SetDock(image,Dock.Left);
            grid.Children.Add(label);
            DockPanel.SetDock(label,Dock.Right);
            PokemonTeamGrid.Children.Add(button);
        }
    }
    private async void LoadPcPokemon()
    {
        PokemonPCGrid.ColumnDefinitions.Clear();
        PokemonPCGrid.RowDefinitions.Clear();
        PokemonPCGrid.Children.Clear();

        if (PCPokemon.Count == 0)
        {
            int pid = 1;
            do
            {
                var p = await Dal.GetPlayerPokemonAsync(pid);
                if (p == null) break;
                PCPokemon.Add(p);
                pid++;
            } while (true);
        }
        
        for (int i = 0; i < 3; i++)
        {
            PokemonPCGrid.ColumnDefinitions.Add(new ColumnDefinition());
        }

        for (int i = 0; i < int.Max(1, (int)Math.Ceiling(PCPokemon.Count / 3f)); i++)
        {
            PokemonPCGrid.RowDefinitions.Add(new RowDefinition());
        }

        int j = 0;
        foreach (PlayerPokemon pokemon in PCPokemon)
        {
            if (_pokemonTeamService.CurrentTeam.Pokemon.Contains(pokemon)) continue;
            
            DockPanel grid = new();
            Button button = new()
            {
                Content = grid
            };
            Label label = new Label()
            {
                Content = char.ToUpper(pokemon.Pokemon.Name[0]) + pokemon.Pokemon.Name[1..],
                HorizontalAlignment = HorizontalAlignment.Right,
                FontSize = 20,
                VerticalAlignment = VerticalAlignment.Center
            };
            Image image = new Image()
            {
                Source = new BitmapImage(new Uri(pokemon.Pokemon.DefaultSprite)),
                HorizontalAlignment = HorizontalAlignment.Left,
                Width = 50
            };

            void DetailsOpen()
            {
                PokemonDetails details = new PokemonDetails(Dal)
                {
                    CurrentPlayerPokemon = pokemon
                };
                RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.NearestNeighbor);
                details.Show();
            }
            
            void AddToTeam()
            {
                for (int i = 0; i < _pokemonTeamService.CurrentTeam.Pokemon.Length; i++)
                {
                    ref var p = ref _pokemonTeamService.CurrentTeam.Pokemon[i];
                    if (p != null) continue;
                    p = pokemon;
                    LoadTeamPokemon();
                    LoadPcPokemon();
                    break;
                }
            }

            button.Click += (_, _) => DetailsOpen();
            button.MouseRightButtonDown += (_, _) => AddToTeam();
            
            Grid.SetColumn(button, j%3);
            Grid.SetRow(button, j/3);
            
            grid.Children.Add(image);
            DockPanel.SetDock(image,Dock.Left);
            grid.Children.Add(label);
            DockPanel.SetDock(label,Dock.Right);
            PokemonPCGrid.Children.Add(button);

            j++;
        }
    }
    
    private async void BackClick(object sender, RoutedEventArgs e)
    {
        try
        {
            await Dal.SavePokemonTeamAsync(_pokemonTeamService.CurrentTeam);
        }
        catch (NullReferenceException error)
        {
            await Console.Error.WriteLineAsync("Warning: team is incomplete and will not be written to disk!");
        }

        Close();
    }
}