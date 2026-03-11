using MongoDB.Bson;
using PokemonSweeper.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PokemonSweeper.Game.PokemonModels
{
    public class PlayerPokemon
    {
        #region Pokemon Properties

        public int PlayerPokemonId { get; set; } = -1; // This will be set when the Pokemon is added to the player's database record

        public Pokemon Pokemon { get; set; }
        public int Level { get; set; }
        public int Experience { get; set; } = 0;
        public bool IsShiny { get; set; }
        [JsonIgnore]
        public string SpriteUrl => IsShiny ? Pokemon.ShinySprite : Pokemon.DefaultSprite;
        public Dictionary<PokemonStatsType, PokemonStat> Stats { get; set; }

        #endregion

        #region Battle Properties
        [JsonIgnore]
        public int MaxHP => Stats[PokemonStatsType.HP].CalculateStat(Level);
        [JsonIgnore]
        public int CurrentHP { get; set; }
        [JsonIgnore]
        public bool IsFainted => CurrentHP <= 0;

        [JsonIgnore]
        public int Attack => Stats[PokemonStatsType.Attack].CalculateStat(Level);
        [JsonIgnore]
        public int Defense => Stats[PokemonStatsType.Defense].CalculateStat(Level);
        [JsonIgnore]
        public int SpecialAttack => Stats[PokemonStatsType.SpecialAttack].CalculateStat(Level);
        [JsonIgnore]
        public int SpecialDefense => Stats[PokemonStatsType.SpecialDefense].CalculateStat(Level);
        [JsonIgnore]
        public int Speed => Stats[PokemonStatsType.Speed].CalculateStat(Level);

        [JsonIgnore]
        public int TotalEV => Stats.Values.Sum(s => s.EV);

        #endregion

        #region Battle Methods

        public void ResetHP()
        {
            CurrentHP = MaxHP;
        }

        public void GrantBattleRewards(int expGained, Dictionary<PokemonStatsType, int> evsGained)
        {
            Experience += expGained;
            CheckLevelUp();
            foreach (var ev in evsGained)
            {
                if (TotalEV >= 510) break; // Max total EVs is 510

                if (Stats[ev.Key].EV >= 252) continue; // Max EVs per stat is 252

                int evToAdd = Math.Min(Math.Min(ev.Value, 252 - Stats[ev.Key].EV), 510 - TotalEV); // Calculate how many EVs can be added without exceeding 252 or total EV limit
                Stats[ev.Key].EV += evToAdd;
            }
        }

        private void CheckLevelUp()
        {
            int expForNextLevel = CalculateExpForLevel(Level + 1);
            if (Experience >= expForNextLevel)
            {
                int prevMaxHP = MaxHP;
                Level++;
                CurrentHP += MaxHP - prevMaxHP; // Increase current HP by the amount max HP increased
                CheckLevelUp(); // Check for multiple level-ups
            }
        }

        public int CalculateExpForLevel(int targetLevel)
        {
            // TODO: Implement exp curve based on Pokemon's growth rate. For now, using a simple cubic formula as a placeholder.
            return (int)(Math.Pow(targetLevel, 3));
        }

        public int CalculateExpYield()
        {
            // Simplified exp yield calculation
            return (int)((Pokemon.BaseExpYield * Level) / 7.0f);
        }

        #endregion

        #region Factory Methods

        public static async Task<PlayerPokemon> CreateRandomFromBST(DAL dal, int minBst, int maxBst, int level = 0, int levelMargin = 10)
        {
            var pokemonBase = await dal.GetRandomPokemonByBstAsync(minBst, maxBst);

            return CreateWithRandomStats(pokemonBase, dal, level, levelMargin);
        }

        public static async Task<PlayerPokemon> CreateWithRandomStats(DAL dal, int level = 0, int levelMargin = 10)
        {
            var random = new Random();
            var randomDexNum = random.Next(1, dal.PokemonMasterList.Count);

            var pokemonBase = await dal.GetPokemonByDexNumAsync(randomDexNum);

            return CreateWithRandomStats(pokemonBase, dal, level, levelMargin);
        }

        public static PlayerPokemon CreateWithRandomStats(Pokemon pokemonBase, DAL dal, int level = 0, int levelMargin = 10)
        {
            var random = new Random();

            var pokemon = new PlayerPokemon()
            {
                Pokemon = pokemonBase,
                Level = level > 0 ? Math.Clamp(random.Next(level - levelMargin, level + levelMargin), 1, 100)
                    : random.Next(1, 101), // Random level between 1 and 100 if not specified
                Stats = new Dictionary<PokemonStatsType, PokemonStat>()
                {
                    { PokemonStatsType.HP, new PokemonStat { StatType = PokemonStatsType.HP, BaseValue = pokemonBase.BaseStats[PokemonStatsType.HP] } },
                    { PokemonStatsType.Attack, new PokemonStat { StatType = PokemonStatsType.Attack, BaseValue = pokemonBase.BaseStats[PokemonStatsType.Attack] } },
                    { PokemonStatsType.Defense, new PokemonStat { StatType = PokemonStatsType.Defense, BaseValue = pokemonBase.BaseStats[PokemonStatsType.Defense] } },
                    { PokemonStatsType.SpecialAttack, new PokemonStat { StatType = PokemonStatsType.SpecialAttack, BaseValue = pokemonBase.BaseStats[PokemonStatsType.SpecialAttack] } },
                    { PokemonStatsType.SpecialDefense, new PokemonStat { StatType = PokemonStatsType.SpecialDefense, BaseValue = pokemonBase.BaseStats[PokemonStatsType.SpecialDefense] } },
                    { PokemonStatsType.Speed, new PokemonStat { StatType = PokemonStatsType.Speed, BaseValue = pokemonBase.BaseStats[PokemonStatsType.Speed] } }
                }
            };

            pokemon.GenerateRandomStats();

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
            ResetHP(); // Set current HP to max HP after generating stats
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
                        BaseValue = pokemon.Pokemon.BaseStats[statType],
                        IV = stat.Value["iv"].AsInt32,
                        EV = stat.Value["ev"].AsInt32,
                        NatureType = (PokemonNatureType)Enum.Parse(typeof(PokemonNatureType), stat.Value["nature_type"].AsString)
                    };
                }
            }

            pokemon.ResetHP(); // Set current HP to max HP after loading stats
            return pokemon;
        }

        public BsonDocument ToBson()
        {
            var statsDoc = new BsonDocument();
            foreach (var stat in Stats)
            {
                statsDoc[stat.Key.ToString()] = new BsonDocument
                {
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

        #endregion
    }
}
