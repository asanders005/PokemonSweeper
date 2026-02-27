using MongoDB.Bson;
using System;
using System.Linq;
using System.Windows.Media.Imaging;

namespace PokemonSweeper.Game
{
    public enum PokemonType
    {
        Normal,
        Fire,
        Water,
        Electric,
        Grass,
        Ice,
        Fighting,
        Poison,
        Ground,
        Flying,
        Psychic,
        Bug,
        Rock,
        Ghost,
        Dragon,
        Dark,
        Steel,
        Fairy
    }

    public class Pokemon
    {
        public int DexNum { get; set; }

        public string Name { get; set; } = "";
        public string DefaultSprite { get; set; } = "";
        public string ShinySprite { get; set; } = "";
        public bool IsShiny { get; set; } = false;

        public PokemonType PrimaryType { get; set; }
        public PokemonType? SecondaryType { get; set; } = null;
        public PokemonStats Stats { get; set; }

        public static Pokemon CreateFromBson(BsonDocument bsonDoc)
        {
            var types = bsonDoc["types"].AsBsonArray;
            int[] stats = bsonDoc["stats"].AsBsonArray.Select(s => s.AsInt32).ToArray();

            return new Pokemon
            {
                DexNum = bsonDoc["dex_num"].AsInt32,
                Name = bsonDoc["name"].AsString,
                DefaultSprite = bsonDoc["sprites"]["default"].AsString,
                ShinySprite = bsonDoc["sprites"]["shiny"].AsString,
                PrimaryType = (PokemonType)Enum.Parse(typeof(PokemonType), types[0].AsString),
                SecondaryType = types.Count > 1 ? (PokemonType?)Enum.Parse(typeof(PokemonType), types[1].AsString) : null,
                Stats = new PokemonStats(stats)
            };
        }

        public BsonDocument ToBson()
        {
            return new BsonDocument
            {
                { "dex_num", DexNum },
                { "name", Name },
                { "sprites", new BsonDocument
                    {
                        { "default", DefaultSprite },
                        { "shiny", ShinySprite }
                    }
                },
                { "types", new BsonArray
                    {
                        PrimaryType.ToString(),
                        SecondaryType?.ToString() ?? ""
                    }
                },
                { "stats", new BsonArray(Stats.AsArray) }
            };
        }
    }

    public class PokemonStats
    {
        public int HP { get; set; }
        public int Attack { get; set; }
        public int Defense { get; set; }
        public int SpecialAttack { get; set; }
        public int SpecialDefense { get; set; }
        public int Speed { get; set; }

        public int BST => HP + Attack + Defense + SpecialAttack + SpecialDefense + Speed;

        public int[] AsArray => new[] {HP, Attack, Defense, SpecialAttack, SpecialDefense, Speed};

        public PokemonStats() {}
        public PokemonStats(int[] stats)
        {
            if (stats.Length != 6)
                throw new ArgumentException("Stats array must have exactly 6 elements.");

            HP = stats[0];
            Attack = stats[1];
            Defense = stats[2];
            SpecialAttack = stats[3];
            SpecialDefense = stats[4];
            Speed = stats[5];
        }
    }
}