using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using PokemonSweeper.Data;
using PokemonSweeper.Game;

namespace PokemonSweeper
{
    public class Field
    {
        private readonly Random Random = new Random();
        private readonly DAL _dal;
        public Stopwatch Timer;

        private Field(int rows, int columns, DAL dal)
        {
            Rows = rows;
            _dal = dal;
            Columns = columns;
            NrOfClicks = 0;
        }

        public static async Task<Field> CreateAsync(int rows, int columns, int nrOfPokemon, int openSquares, GameWindow window, DAL dal)
        {
            var field = new Field(rows, columns, dal);
            await field.PopulateField(nrOfPokemon, openSquares, window);
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

        private async Task PopulateField(int nrOfPokemon, int openSquares, GameWindow window)
        {
            var pokemonPlacers = new List<int>();


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
            for (var row = 0; row < Rows; row++)
            {
                for (var column = 0; column < Columns; column++)
                {
                    Squares.Add(new Square(this, Rows, Columns, row, column));
                    if (pokemonPlacers.Contains(Squares.Count - 1))
                    {
                        Squares[Squares.Count - 1].Pokemon = await PlayerPokemon.CreateWithRandomStats(_dal);
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
                Squares[openLocation].SwipeSquare(window);
            }
        }
    }
}