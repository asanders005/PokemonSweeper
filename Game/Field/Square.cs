using PokemonSweeper.Data;
using PokemonSweeper.Game.Messages;
using PokemonSweeper.Game.PokemonModels;
using PokemonSweeper.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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

        public Square(Field field, int rows, int columns, int row, int column, PokemonTeamService pokemonTeamService, IDal dal)
        {
            Field = field;
            NrOfRows = rows;
            NrOfColumns = columns;
            Row = row;
            _pokemonTeamService = pokemonTeamService;
            _d = dal;
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
        private readonly IDal _d;

        private static readonly Brush[] NumberColors = {
            Brushes.Blue,
            Brushes.Green,
            Brushes.Red,
            Brushes.DarkBlue,
            Brushes.Brown,
            Brushes.Cyan,
            Brushes.Magenta,
            Brushes.Orange
        };

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
            if (Status == SquareStatus.Open)
            {
                Status = SquareStatus.Flagged;
                Content = new Image { Source = new BitmapImage(new Uri("pack://application:,,,/images/pokeball.png")) };
                UpdateMineText(sender);
            }
            else if (Status == SquareStatus.Flagged)
            {
                Status = SquareStatus.Question;
                Content = "?";
                Foreground = Brushes.Blue;
                FontWeight = FontWeights.Bold;
                UpdateMineText(sender);
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
                if (Field.ClearedSquares + window.Game.FieldLevel.Pokemon ==
                    window.Game.FieldLevel.Dimention)
                {
                    Score.ShowScore(window, Field, _pokemonTeamService, _d);
                }
            }
        }

        public async Task SwipeSquare(GameWindow window)
        {
            if (Field.NrOfClicks < 1)
            {
                Field.NrOfClicks++;

                Pokemon = null;
                var squaresList = Field.Squares.Where
                    (s => (s.Row >= Row - 1) && (s.Row <= Row + 1) &&
                          (s.Column >= Column - 1) && (s.Column <= Column + 1))
                    .ToList();
                foreach (var Square in squaresList)
                {
                    if (Square.Pokemon != null)
                    {
                        Square.Pokemon = null;
                        window.Game.FieldLevel.Pokemon--;
                        UpdateMineText(window);
                    }
                }
                foreach (var Square in squaresList)
                {
                    await Square.SwipeSquare(window);
                }
                return;
            }

            if (Pokemon != null)
            {
                Content = new Image { Source = new BitmapImage(new Uri(Pokemon.SpriteUrl)) };
                Status = SquareStatus.Pokemon;
                Background = Brushes.Red;
                BorderBrush = Brushes.Red;
                IsEnabled = false;

                var battleResult = await _pokemonTeamService.CurrentTeam.Battle(Pokemon);

                // If player wins, show battle result, otherwise show fail message
                if (battleResult.Item1)
                {
                    BattleMessage.ShowMessage(window, Pokemon, battleResult.Item2);

                    UpdateMineText(window);
                }
                else
                    FailMessage.ShowMessage(window, Pokemon, _d, _pokemonTeamService);
            }
            else if (Mines > 0)
            {
                Content = Mines;
                Status = SquareStatus.Cleared;
                Background = Brushes.White;
                Foreground = NumberColors[Mines - 1];
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
                    await OtherSquare.SwipeSquare(window);
            }
        }

        private void UpdateMineText(GameWindow window)
        {
            var FlaggedSquares = Field.Squares.Where(square => square.Status == SquareStatus.Flagged || (square.Status == SquareStatus.Pokemon)).ToList();
            window.MinesLeftLabel(window.Game.FieldLevel.Pokemon - FlaggedSquares.Count());

            if (FlaggedSquares.Count() == window.Game.FieldLevel.Pokemon)
            {
                var win = true;
                foreach (var flaggedSquare in FlaggedSquares)
                {
                    if (flaggedSquare.Pokemon == null)
                    {
                        win = false;
                    }
                }
                if (win) Score.ShowScore(window, Field, _pokemonTeamService, _d);
            }
        }
    }
}