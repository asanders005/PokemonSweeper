using MongoDB.Bson;
using System;
using System.Collections.Generic;
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
        public Dictionary<PokemonStatsType, PokemonStat> Stats { get; set; }
        public Dictionary<PokemonStatsType, int> EvYield { get; set; }

        public static Pokemon CreateFromBson(BsonDocument bsonDoc)
        {
            var types = bsonDoc["types"].AsBsonArray;
            var stats = bsonDoc["stats"].AsBsonDocument;

            return new Pokemon
            {
                DexNum = bsonDoc["dex_num"].AsInt32,
                Name = bsonDoc["name"].AsString,
                DefaultSprite = bsonDoc["sprites"]["default"].AsString,
                ShinySprite = bsonDoc["sprites"]["shiny"].AsString,
                PrimaryType = (PokemonType)Enum.Parse(typeof(PokemonType), types[0].AsString),
                SecondaryType = (types.Count > 1 && !string.IsNullOrWhiteSpace(types[1].AsString)) ? (PokemonType?)Enum.Parse(typeof(PokemonType), types[1].AsString) : null,
                Stats = new Dictionary<PokemonStatsType, PokemonStat>
                {   { PokemonStatsType.HP, new PokemonStat { StatType = PokemonStatsType.HP, BaseValue = stats["HP"].AsInt32 } },
                    { PokemonStatsType.Attack, new PokemonStat { StatType = PokemonStatsType.Attack, BaseValue = stats["Attack"].AsInt32 } },
                    { PokemonStatsType.Defense, new PokemonStat { StatType = PokemonStatsType.Defense, BaseValue = stats["Defense"].AsInt32 } },
                    { PokemonStatsType.SpecialAttack, new PokemonStat { StatType = PokemonStatsType.SpecialAttack, BaseValue = stats["SpecialAttack"].AsInt32 } },
                    { PokemonStatsType.SpecialDefense, new PokemonStat { StatType = PokemonStatsType.SpecialDefense, BaseValue = stats["SpecialDefense"].AsInt32 } },
                    { PokemonStatsType.Speed, new PokemonStat { StatType = PokemonStatsType.Speed, BaseValue = stats["Speed"].AsInt32 } }
                }
            };
        }

        public static Pokemon CreateWithRandomStats(BsonDocument bsonDoc)
        {
            var pokemon = CreateFromBson(bsonDoc);
            var random = new Random();

            pokemon.IsShiny = random.NextDouble() < 0.00024;
            int posNatureStat = random.Next(0, 6);
            int negNatureStat = random.Next(0, 6);
            int statIndex = 0;

            foreach (var stat in pokemon.Stats.Values)
            {
                stat.IV = random.Next(0, 32);
                //stat.EV = random.Next(0, 256);

                stat.NatureType = (statIndex == posNatureStat && posNatureStat == negNatureStat) ? PokemonNatureType.Neutral :
                                  (statIndex == posNatureStat) ? PokemonNatureType.Beneficial :
                                  (statIndex == negNatureStat) ? PokemonNatureType.Hindering :
                                  PokemonNatureType.Neutral;

                statIndex++;
            }

            return pokemon;
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
                        SecondaryType?.ToString() ?? null
                    }
                },
                { "stats", new BsonDocument
                    {
                        { "HP", Stats[PokemonStatsType.HP].BaseValue },
                        { "Attack", Stats[PokemonStatsType.Attack].BaseValue },
                        { "Defense", Stats[PokemonStatsType.Defense].BaseValue },
                        { "SpecialAttack", Stats[PokemonStatsType.SpecialAttack].BaseValue },
                        { "SpecialDefense", Stats[PokemonStatsType.SpecialDefense].BaseValue },
                        { "Speed", Stats[PokemonStatsType.Speed].BaseValue }
                    }
                }
            };
        }
    }

    public enum PokemonStatsType
    {
        HP,
        Attack,
        Defense,
        SpecialAttack,
        SpecialDefense,
        Speed
    }

    public enum PokemonNatureType
    {
        Beneficial,
        Neutral,
        Hindering
    }

    public class PokemonStat
    {
        public PokemonStatsType StatType { get; set; }
        public int BaseValue { get; set; }
        public int IV { get; set; } = 0;
        public int EV { get; set; } = 0;
        public PokemonNatureType NatureType { get; set; } = PokemonNatureType.Neutral;

        public int CalculateStat(int level)
        {
            int statValue = ((2 * BaseValue + IV + (EV / 4)) * level) / 100;

            if (StatType == PokemonStatsType.HP)
            {
                return statValue + level + 10;
            }

            float natureModifier = NatureType switch
            {
                PokemonNatureType.Beneficial => 1.1f,
                PokemonNatureType.Neutral => 1.0f,
                PokemonNatureType.Hindering => 0.9f,
                _ => 1.0f
            };

            return (int)((statValue + 5) * natureModifier);
        }
    }
}