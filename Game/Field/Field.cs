using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using PokemonSweeper.Data;
using PokemonSweeper.Game.PokemonModels;
using PokemonSweeper.Services;

namespace PokemonSweeper
{
    public class Field
    {
        private readonly Random Random = new Random();
        private readonly DAL _dal;
        private readonly int _level;

        public Stopwatch Timer;

        private Field(int rows, int columns, DAL dal, int level)
        {
            _level = level;
            Rows = rows;
            _dal = dal;
            Columns = columns;
            NrOfClicks = 0;
        }

        public static async Task<Field> CreateAsync(int rows, int columns, int nrOfPokemon, int openSquares, GameWindow window, int level, DAL dal, PokemonTeamService pokemonTeamService)
        {
            var field = new Field(rows, columns, dal, level);
            await field.PopulateField(nrOfPokemon, openSquares, window, pokemonTeamService);
            field.Timer = Stopwatch.StartNew();
            return field;
        }

        public int Columns { get; set; }
        public int Rows { get; set; }
        public List<Square> Squares { get; set; }

        public int ClearedSquares
        {
            get { return Squares.Where(Square => Square.Status == Square.SquareStatus.Cleared).Count(); }
        }

        public int NrOfClicks { get; set; }

        private async Task PopulateField(int nrOfPokemon, int openSquares, GameWindow window, PokemonTeamService teamService)
        {
            var pokemonPlacers = new List<int>();

            int averageLevel = teamService.CurrentTeam.AverageLevel;

            int pokemonLocation;
            for (var i = 0; i < nrOfPokemon; i++)
            {
                do
                {
                    pokemonLocation = Random.Next(Rows*Columns);
                } while (pokemonPlacers.Contains(pokemonLocation));
                pokemonPlacers.Add(pokemonLocation);
            }
            Squares = new List<Square>();

            int minBst = _level switch
            {
                0 => 0,
                1 => 350,
                2 => 500,
                _ => throw new ArgumentOutOfRangeException(nameof(_level), "Level must be between 0 and 2.")
            };

            int maxBst = _level switch
            {
                0 => 375,
                1 => 525,
                2 => int.MaxValue,
                _ => throw new ArgumentOutOfRangeException(nameof(_level), "Level must be between 0 and 2.")
            };

            for (var row = 0; row < Rows; row++)
            {
                for (var column = 0; column < Columns; column++)
                {
                    Squares.Add(new Square(this, Rows, Columns, row, column, teamService, _dal));
                    if (pokemonPlacers.Contains(Squares.Count - 1))
                    {
                        Squares[Squares.Count - 1].Pokemon = await PlayerPokemon.CreateRandomFromBST(_dal, minBst, maxBst, averageLevel);
                    }
                }
            }
            for (var i = 0; i < openSquares; i++)
            {
                int openLocation;
                do
                {
                    openLocation = Random.Next(Rows*Columns);
                } while (pokemonPlacers.Contains(openLocation) ||
                         Squares[openLocation].Status == Square.SquareStatus.Cleared);
                Squares[openLocation].Status = Square.SquareStatus.Cleared;
                await Squares[openLocation].SwipeSquare(window);
            }
        }
    }
}