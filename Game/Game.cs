using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using PokemonSweeper;
using PokemonSweeper.Data;
using PokemonSweeper.Game.PokemonModels;
using PokemonSweeper.Services;

namespace PokemonSweeper.Game
{
    public class PokeSweepGame
    {
        public PokeSweepGame(DAL dal, PokemonTeamService teamService, string value)
        {
            _dal = dal;
            _pokemonTeamService = teamService;

            Level = 0;
            Score = 0;
            Pokemon = new List<PlayerPokemon>(); // make empty list of Pokemon captured
            string difficulty = value;

            FieldLevels = new List<FieldLevel>(); // Make list of Game Levels
            switch(value){
                case "easy":
                    FieldLevels.Add(new FieldLevel {Rows = 9, Columns = 9, Pokemon = 10, NextLevel = 1000});
                    break;
                case "intermediate":
                    FieldLevels.Add(new FieldLevel {Rows = 16, Columns = 16, Pokemon = 40, NextLevel = 10000});
                    break;
                case "hard": 
                    FieldLevels.Add(new FieldLevel {Rows = 16, Columns = 32, Pokemon = 99, NextLevel = 20000});
                    break;
            }
           
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
            if (_pokemonTeamService.CurrentTeam == null) _pokemonTeamService.CurrentTeam = await _dal.LoadPokemonTeamAsync();

            window.MineFieldGrid.Children.Clear();

            //for (var i = Level; Score >= FieldLevels[i].NextLevel && i <= FieldLevels.Count(); i++) Level++;
            window.MineFieldGrid.Rows = FieldLevels[Level].Rows;
            window.MineFieldGrid.Columns = FieldLevels[Level].Columns;
            window.Width = 600*FieldLevels[Level].Columns/FieldLevels[Level].Rows;
            window.MineFieldGrid.Width = 500*FieldLevels[Level].Columns/FieldLevels[Level].Rows;
            Field = await PokemonSweeper.Field.CreateAsync(
                FieldLevels[Level].Rows, 
                FieldLevels[Level].Columns,
                FieldLevels[Level].Pokemon,
                FieldLevels[Level].Open, window, 
                _dal,
                _pokemonTeamService);

            foreach (var square in Field.Squares)
            {
                square.Click += window.MineSquare_Click;
                square.MouseRightButtonDown += window.MineSquare_MouseRightButtonDown;
                window.MineFieldGrid.Children.Add(square);
            }
            window.MinesLeftLabel( FieldLevels[Level].Pokemon );
        }

        private PokemonTeamService _pokemonTeamService;
        private readonly DAL _dal;
    }
}