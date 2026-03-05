using MongoDB.Bson;
using PokemonSweeper.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace PokemonSweeper.Game.PokemonModels
{
    public class Pokemon
    {
        public int DexNum { get; set; }

        public string Name { get; set; } = "";
        public string DefaultSprite { get; set; } = "";
        public string ShinySprite { get; set; } = "";

        public PokemonType PrimaryType { get; set; }
        public PokemonType? SecondaryType { get; set; } = null;
        public Dictionary<PokemonStatsType, int> BaseStats { get; set; }
        [JsonIgnore]
        public int BST => BaseStats.Values.Sum();
        public Dictionary<PokemonStatsType, int> EvYield { get; set; }
        public int BaseExpYield { get; set; }

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
                },
                BaseExpYield = bsonDoc["base_exp_yield"].AsInt32
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
                },
                {
                    "base_exp_yield", BaseExpYield
                }
            };
        }
    }
}