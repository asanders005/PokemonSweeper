using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PokemonSweeper.Data;
using PokemonSweeper.Game.PokemonModels;

namespace PokemonSweeper;

public partial class PokemonPC : Window
{
    public DAL Dal { get; set; }
    private List<PlayerPokemon> PCPokemon { get; set; } = new();
    
    public PokemonPC(DAL dal)
    {
        InitializeComponent();
        Dal = dal;
    }

    private async void WindowLoaded(object sender, RoutedEventArgs e)
    {
        
        int pid = 1;
        PlayerPokemon p;
        do
        {
            p = await Dal.GetPlayerPokemonAsync(pid);
            if (p == null) break;
            PCPokemon.Add(p);
            pid++;
        } while (true);

        for (int i = 0; i < 3; i++)
        {
            PokemonPCGrid.ColumnDefinitions.Add(new ColumnDefinition());
        }
        for (int i = 0; i < int.Max(1,(int)Math.Ceiling(PCPokemon.Count/3f)); i++)
        {
            PokemonPCGrid.RowDefinitions.Add(new RowDefinition());
        }

        int j = 0;
        foreach (PlayerPokemon pokemon in PCPokemon)
        {
            DockPanel grid = new();
            Button button = new()
            {
                Content = grid
            };
            Label label = new Label()
            {
                Content = pokemon.Pokemon.Name,
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
    
    private void BackClick(object sender, RoutedEventArgs e)
    {
        Close();
    }
}