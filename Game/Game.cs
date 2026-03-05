using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using PokemonSweeper;
using PokemonSweeper.Data;
using PokemonSweeper.Game.PokemonModels;

namespace PokemonSweeper.Game
{
    public class PokeSweepGame
    {
        public PokeSweepGame(DAL dal)
        {
            _dal = dal;
            Level = 0;
            Score = 0;
            Pokemon = new List<PlayerPokemon>(); // make empty list of Pokemon captured

            FieldLevels = new List<FieldLevel>(); // Make list of Game Levels
            // ------------------------
            FieldLevels.Add(new FieldLevel {Rows = 9, Columns = 9, Pokemon = 10, NextLevel = 1000});
            // Beginner standard minesweeper
            FieldLevels.Add(new FieldLevel {Rows = 16, Columns = 16, Pokemon = 40, NextLevel = 10000});
            // Intermediate standard minesweeper
            FieldLevels.Add(new FieldLevel {Rows = 16, Columns = 32, Pokemon = 99, NextLevel = 20000});
            // Expert standard minesweeper
            FieldLevels.Add(new FieldLevel {Rows = 16, Columns = 16, Pokemon = 70, NextLevel = 99999999, Open = 50});
        }

        public List<FieldLevel> FieldLevels { get; set; }
        public int Score { get; set; }
        public List<PlayerPokemon> Pokemon { get; set; }
        public int Level { get; set; }
        public PokemonSweeper.Field Field { get; set; }
        // Calculate the score gained after finishing a field.
        public int CalculateNewScore(Stopwatch timer, int clicks, List<PlayerPokemon> pokemon)
        {
            // count the number of new (never before) catched pokemon.
            var newPokemon = 0;
            foreach (var monster in pokemon.Where(m => !Pokemon.Contains(m))) newPokemon++;

            // Calculate the score and add it to the old score
            var newScore = (int) ((newPokemon*100 + (100 - clicks)/(timer.Elapsed.TotalSeconds/2)));
            Score += newScore;
            // Return the field-score
            return newScore;
        }

        public async Task NewField(GameWindow window)
        {
            window.MineFieldGrid.Children.Clear();

            for (var i = Level; Score >= FieldLevels[i].NextLevel && i <= FieldLevels.Count(); i++) Level++;
            window.MineFieldGrid.Rows = FieldLevels[Level].Rows;
            window.MineFieldGrid.Columns = FieldLevels[Level].Columns;
            window.Width = 600*FieldLevels[Level].Columns/FieldLevels[Level].Rows;
            window.MineFieldGrid.Width = 500*FieldLevels[Level].Columns/FieldLevels[Level].Rows;
            Field = await PokemonSweeper.Field.CreateAsync(
                FieldLevels[Level].Rows, 
                FieldLevels[Level].Columns,
                FieldLevels[Level].Pokemon,
                FieldLevels[Level].Open, window, 
                _dal);

            foreach (var square in Field.Squares)
            {
                square.Click += window.MineSquare_Click;
                square.MouseRightButtonDown += window.MineSquare_MouseRightButtonDown;
                window.MineFieldGrid.Children.Add(square);
            }
            window.MinesLeftLabel( FieldLevels[Level].Pokemon );
        }

        private readonly DAL _dal;
    }
}