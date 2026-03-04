using MongoDB.Bson;
using PokemonSweeper.Data;
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

   

    public class Pokemon
    {
        public int DexNum { get; set; }

        public string Name { get; set; } = "";
        public string DefaultSprite { get; set; } = "";
        public string ShinySprite { get; set; } = "";

        public PokemonType PrimaryType { get; set; }
        public PokemonType? SecondaryType { get; set; } = null;
        public Dictionary<PokemonStatsType, int> BaseStats { get; set; }
        public Dictionary<PokemonStatsType, int> EvYield { get; set; }

        public static Pokemon CreateFromBson(BsonDocument bsonDoc)
        {
            var types = bsonDoc["types"].AsBsonArray;
            var stats = bsonDoc["stats"].AsBsonDocument;
            var evYield = bsonDoc["ev_yield"].AsBsonDocument;

            return new Pokemon
            {
                DexNum = bsonDoc["dex_num"].AsInt32,
                Name = bsonDoc["name"].AsString,
                DefaultSprite = bsonDoc["sprites"]["default"].AsString,
                ShinySprite = bsonDoc["sprites"]["shiny"].AsString,
                PrimaryType = (PokemonType)Enum.Parse(typeof(PokemonType), types[0].AsString),
                SecondaryType = (types.Count > 1 && !string.IsNullOrWhiteSpace(types[1].AsString)) ? (PokemonType?)Enum.Parse(typeof(PokemonType), types[1].AsString) : null,
                BaseStats = new Dictionary<PokemonStatsType, int>
                {   { PokemonStatsType.HP, stats["HP"].AsInt32 },
                    { PokemonStatsType.Attack, stats["Attack"].AsInt32 },
                    { PokemonStatsType.Defense, stats["Defense"].AsInt32 },
                    { PokemonStatsType.SpecialAttack, stats["SpecialAttack"].AsInt32 },
                    { PokemonStatsType.SpecialDefense, stats["SpecialDefense"].AsInt32 },
                    { PokemonStatsType.Speed, stats["Speed"].AsInt32 }
                },
                EvYield = new Dictionary<PokemonStatsType, int>
                {
                    { PokemonStatsType.HP, evYield["HP"].AsInt32 },
                    { PokemonStatsType.Attack, evYield["Attack"].AsInt32 },
                    { PokemonStatsType.Defense, evYield["Defense"].AsInt32 },
                    { PokemonStatsType.SpecialAttack, evYield["SpecialAttack"].AsInt32 },
                    { PokemonStatsType.SpecialDefense, evYield["SpecialDefense"].AsInt32 },
                    { PokemonStatsType.Speed, evYield["Speed"].AsInt32 }
                }
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
                        SecondaryType?.ToString() ?? null
                    }
                },
                { "stats", new BsonDocument
                    {
                        { "HP", BaseStats[PokemonStatsType.HP] },
                        { "Attack", BaseStats[PokemonStatsType.Attack] },
                        { "Defense", BaseStats[PokemonStatsType.Defense] },
                        { "SpecialAttack", BaseStats[PokemonStatsType.SpecialAttack] },
                        { "SpecialDefense", BaseStats[PokemonStatsType.SpecialDefense] },
                        { "Speed", BaseStats[PokemonStatsType.Speed] }
                    }
                },
                { "ev_yield", new BsonDocument
                    {
                        { "HP", EvYield[PokemonStatsType.HP] },
                        { "Attack", EvYield[PokemonStatsType.Attack] },
                        { "Defense", EvYield[PokemonStatsType.Defense] },
                        { "SpecialAttack", EvYield[PokemonStatsType.SpecialAttack] },
                        { "SpecialDefense", EvYield[PokemonStatsType.SpecialDefense] },
                        { "Speed", EvYield[PokemonStatsType.Speed] }
                    }
                }
            };
        }
    }

    public class PlayerPokemon
    {
        public Pokemon Pokemon { get; set; }
        public int Level { get; set; }
        public bool IsShiny { get; set; }
        public Dictionary<PokemonStatsType, PokemonStat> Stats { get; set; }

        public static PlayerPokemon CreateWithRandomStats(BsonDocument bsonDoc)
        {
            var pokemon = new PlayerPokemon() { Pokemon = Pokemon.CreateFromBson(bsonDoc) };
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

        public void GenerateRandomStats()
        {
            var random = new Random();

            IsShiny = random.NextDouble() < 0.00024;
            int posNatureStat = random.Next(0, 6);
            int negNatureStat = random.Next(0, 6);
            int statIndex = 0;

            foreach (var stat in Stats.Values)
            {
                stat.IV = random.Next(0, 32);

                stat.NatureType = (statIndex == posNatureStat && posNatureStat == negNatureStat) ? PokemonNatureType.Neutral :
                                  (statIndex == posNatureStat) ? PokemonNatureType.Beneficial :
                                  (statIndex == negNatureStat) ? PokemonNatureType.Hindering :
                                  PokemonNatureType.Neutral;

                statIndex++;
            }
        }

        public static PlayerPokemon CreateFromBson(BsonDocument bsonDoc, DAL dal)
        {
            var pokemon = new PlayerPokemon
            {
                Pokemon = dal.GetPokemonByDexNumAsync(bsonDoc["dex_num"].AsInt32).Result,
                Level = bsonDoc["level"].AsInt32,
                IsShiny = bsonDoc["is_shiny"].AsBoolean,
                Stats = new Dictionary<PokemonStatsType, PokemonStat>()
            };
            var statsDoc = bsonDoc["stats"].AsBsonDocument;
            foreach (var stat in statsDoc.Elements)
            {
                if (Enum.TryParse(stat.Name, out PokemonStatsType statType))
                {
                    pokemon.Stats[statType] = new PokemonStat
                    {
                        StatType = statType,
                        BaseValue = stat.Value["base_value"].AsInt32,
                        IV = stat.Value["iv"].AsInt32,
                        EV = stat.Value["ev"].AsInt32,
                        NatureType = (PokemonNatureType)Enum.Parse(typeof(PokemonNatureType), stat.Value["nature_type"].AsString)
                    };
                }
            }
            return pokemon;
        }

        public BsonDocument ToBson()
        {
            var statsDoc = new BsonDocument();
            foreach (var stat in Stats)
            {
                statsDoc[stat.Key.ToString()] = new BsonDocument
                {
                    { "base_value", stat.Value.BaseValue },
                    { "iv", stat.Value.IV },
                    { "ev", stat.Value.EV },
                    { "nature_type", stat.Value.NatureType.ToString() }
                };
            }
            return new BsonDocument
            {
                { "dex_num", Pokemon.DexNum },
                { "level", Level },
                { "is_shiny", IsShiny },
                { "stats", statsDoc }
            };
        }
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