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
        public PokeSweepGame(IDal dal, PokemonTeamService teamService, string value)
        {
            _dal = dal;
            _pokemonTeamService = teamService;

            Level = 0;
            Score = 0;
            Pokemon = new List<PlayerPokemon>(); // make empty list of Pokemon captured
            string difficulty = value;

            switch(value){
                case "easy":
                    FieldLevel = new FieldLevel {Rows = 9, Columns = 9, Pokemon = 10, NextLevel = 1000};
                    Level = 0;
                    break;
                case "intermediate":
                    FieldLevel = new FieldLevel {Rows = 16, Columns = 16, Pokemon = 40, NextLevel = 10000};
                    Level = 1;
                    break;
                case "hard": 
                    FieldLevel = new FieldLevel {Rows = 16, Columns = 32, Pokemon = 99, NextLevel = 20000};
                    Level = 2;
                    break;
            }
           
        }

        public FieldLevel FieldLevel { get; set; }
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
            /* Temp functionality for testing
            //if (_pokemonTeamService.CurrentTeam == null)
            //{
            //    PlayerPokemon[] playerPokemons = new PlayerPokemon[]
            //    {
            //        await _dal.GetPlayerPokemonAsync(6),
            //        await _dal.GetPlayerPokemonAsync(7),
            //        await _dal.GetPlayerPokemonAsync(2),
            //        await _dal.GetPlayerPokemonAsync(5),
            //        null,
            //        null
            //    };

            //    _pokemonTeamService.CurrentTeam = new PokemonTeam(_dal)
            //    {
            //        Pokemon = playerPokemons
            //    };
            //} */

            // Actual functionality for loading the team, if it is not already loaded. This should be the case when starting a new game, but not when going to the next level.
            if (_pokemonTeamService.CurrentTeam == null) _pokemonTeamService.CurrentTeam = await _dal.LoadPokemonTeamAsync();

            _pokemonTeamService.CurrentTeam.RestTeam();

            window.MineFieldGrid.Children.Clear();

            //for (var i = Level; Score >= FieldLevels[i].NextLevel && i <= FieldLevels.Count(); i++) Level++;
            window.MineFieldGrid.Rows = FieldLevel.Rows;
            window.MineFieldGrid.Columns = FieldLevel.Columns;
            window.Width = 600*FieldLevel.Columns/FieldLevel.Rows;
            window.MineFieldGrid.Width = 500*FieldLevel.Columns/FieldLevel.Rows;
            Field = await PokemonSweeper.Field.CreateAsync(
                FieldLevel.Rows, 
                FieldLevel.Columns,
                FieldLevel.Pokemon,
                FieldLevel.Open, window, 
                Level,
                _dal,
                _pokemonTeamService);

            foreach (var square in Field.Squares)
            {
                square.Click += window.MineSquare_Click;
                square.MouseRightButtonDown += window.MineSquare_MouseRightButtonDown;
                window.MineFieldGrid.Children.Add(square);
            }
            window.MinesLeftLabel( FieldLevel.Pokemon );
        }

        private PokemonTeamService _pokemonTeamService;
        private readonly IDal _dal;
    }
}