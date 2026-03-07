using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PokemonSweeper.Game.Messages;
using System.Windows.Documents;
using System.Collections.Generic;
using PokemonSweeper.Game.PokemonModels;
using PokemonSweeper.Services;
using System.Threading.Tasks;

namespace PokemonSweeper
{
    public class Square : Button
    {
        public enum SquareStatus
        {
            Cleared,
            Pokemon,
            Open,
            Flagged,
            Question
        }

        public Square(Field field, int rows, int columns, int row, int column, PokemonTeamService pokemonTeamService)
        {
            Field = field;
            NrOfRows = rows;
            NrOfColumns = columns;
            Row = row;
            _pokemonTeamService = pokemonTeamService;
            Column = column;
            Pokemon = null;
            Status = SquareStatus.Open;
        }

        public SquareStatus Status { get; set; }
        public int Row { get; set; }
        public int Column { get; set; }
        public int NrOfRows { get; set; }

        public int NrOfColumns
        {
            get { return NrOfRows; }
            set { NrOfRows = value; }
        }

        public Field Field { get; set; }
        public PlayerPokemon Pokemon { get; set; }

        private PokemonTeamService _pokemonTeamService;

        public int Mines
        {
            get
            {
                var mines = 0;
                foreach (var Square in (Field.Squares.Where
                    (s => (s.Row >= Row - 1) && (s.Row <= Row + 1) &&
                          (s.Column >= Column - 1) && (s.Column <= Column + 1))
                    .ToList()))
                {
                    if (Square.Pokemon != null) mines++;
                }
                return mines;
            }
        }

        public void RightButton(GameWindow sender)
        {
            List<Square> FlaggedSquares;
            if (Status == SquareStatus.Open)
            {
                Status = SquareStatus.Flagged;
                Content = new Image {Source = new BitmapImage(new Uri("pack://application:,,,/images/pokeball.png"))};
                FlaggedSquares = Field.Squares.Where( square => square.Status == SquareStatus.Flagged ).ToList();
                sender.MinesLeftLabel( sender.Game.FieldLevels[sender.Game.Level].Pokemon - FlaggedSquares.Count() );
                if (FlaggedSquares.Count() == sender.Game.FieldLevels[sender.Game.Level].Pokemon)
                {
                    var win = true;
                    foreach (var flaggedSquare in FlaggedSquares)
                    {
                        if (flaggedSquare.Pokemon == null)
                        {
                            win = false;
                        }
                    }
                    if (win) Score.ShowScore(sender, Field);
                }
            }
            else if (Status == SquareStatus.Flagged)
            {
                Status = SquareStatus.Question;
                Content = "?";
                Foreground = Brushes.Blue;
                FontWeight = FontWeights.Bold;
                FlaggedSquares = Field.Squares.Where( square => square.Status == SquareStatus.Flagged ).ToList();
                sender.MinesLeftLabel( sender.Game.FieldLevels[sender.Game.Level].Pokemon - FlaggedSquares.Count() );
            }
            else
            {
                Status = SquareStatus.Open;
                Content = "";
                Foreground = Brushes.Gray;
                FontWeight = FontWeights.Normal;
            }
            
        }

        public async void LeftButton(GameWindow window)
        {
            if (Status == SquareStatus.Open)
            {
                await SwipeSquare(window);
                if (Field.ClearedSquares + window.Game.FieldLevels[window.Game.Level].Pokemon ==
                    window.Game.FieldLevels[window.Game.Level].Dimention) Score.ShowScore(window, Field);
            }
        }

        public async Task SwipeSquare(GameWindow window)
        {
            Field.NrOfClicks++;
            if (Pokemon != null)
            {
                Content = new Image {Source = new BitmapImage(new Uri(Pokemon.SpriteUrl))};
                Status = SquareStatus.Pokemon;
                Background = Brushes.Red;
                BorderBrush = Brushes.Red;
                IsEnabled = false;

                var battleResult = await _pokemonTeamService.CurrentTeam.Battle(Pokemon);

                // If player wins, show battle result, otherwise show fail message
                if (battleResult.Item1)
                    BattleMessage.ShowMessage(window, Pokemon, battleResult.Item2);
                else
                    FailMessage.ShowMessage(window, Pokemon);
            }
            else if (Mines ==1)
            {
                Content = Mines;
                Status = SquareStatus.Cleared;
                Background = Brushes.White;
                Foreground = Brushes.Blue;
                BorderBrush = Brushes.White;
                IsEnabled = false;
            }
            else if (Mines == 2)
            {
                Content = Mines;
                Status = SquareStatus.Cleared;
                Background = Brushes.White;
                Foreground = Brushes.Green;
                BorderBrush = Brushes.White;
                IsEnabled = false;
            }
            else if (Mines == 3)
            {
                Content = Mines;
                Status = SquareStatus.Cleared;
                Background = Brushes.White;
                Foreground = Brushes.Red;
                BorderBrush = Brushes.White;
                IsEnabled = false;
            }
            else if (Mines == 4)
            {
                Content = Mines;
                Status = SquareStatus.Cleared;
                Background = Brushes.White;
                Foreground= Brushes.DarkBlue;
                BorderBrush = Brushes.White;
                IsEnabled = false;
            }
            else if (Mines == 5)
            {
                Content = Mines;
                Status = SquareStatus.Cleared;
                Background = Brushes.White;
                Foreground = Brushes.DarkRed;
                BorderBrush = Brushes.White;
                IsEnabled = false;
            }
            else if (Mines == 6)
            {
                Content = Mines;
                Status = SquareStatus.Cleared;
                Background = Brushes.White;
                Foreground = Brushes.Teal;
                BorderBrush = Brushes.White;
                IsEnabled = false;
            }
            else if (Mines == 7)
            {
                Content = Mines;
                Status = SquareStatus.Cleared;
                Background = Brushes.White;
                BorderBrush = Brushes.White;
                IsEnabled = false;
            }
            else if (Mines == 8)
            {
                Content = Mines;
                Status = SquareStatus.Cleared;
                Background = Brushes.White;
                Foreground = Brushes.Gray;
                BorderBrush = Brushes.White;
                IsEnabled = false;
            }
            else
            {
                Background = Brushes.White;
                BorderBrush = Brushes.White;
                Status = SquareStatus.Cleared;
                IsEnabled = false;
                foreach (var OtherSquare in (Field.Squares.Where
                    (s => (s.Row >= Row - 1) && (s.Row <= Row + 1) &&
                          (s.Column >= Column - 1) && (s.Column <= Column + 1) && (s.Status == SquareStatus.Open))
                    .ToList()))
                    OtherSquare.SwipeSquare(window);
            }
        }
    }
}