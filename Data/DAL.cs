using MongoDB.Bson;
using MongoDB.Driver;
using PokemonSweeper.API;
using PokemonSweeper.Game.PokemonModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PokemonSweeper.Data
{
    public class DAL
    {
        private readonly IMongoDatabase _database;
        private static int playerPokemonIdCounter = 1; // This will be used to assign unique IDs to PlayerPokemon instances

        public Dictionary<int, string> PokemonMasterList
        {
            get
            {
                if (pokemonMasterList.Count == 0)
                {
                    GetPokemonMasterList();
                }
                return pokemonMasterList;
            }
        }
        private Dictionary<int, string> pokemonMasterList = new Dictionary<int, string>();

        public DAL(IMongoDatabase database = null)
        {
            _database = database;
            _ = CreatePokemonMasterList();
            _ = CreateDBPokemonCollection();
            _ = CreateDBTypeEffectivenessCollection();
            _ = CreateDBPlayerPokemonCollection();
        }

        private async Task CreatePokemonMasterList()
        {
            Dictionary<int, string> dict = await PokeApiService.GetAllPokemon();

            if (_database != null)
            {

                var collectionNames = await _database.ListCollectionNames().ToListAsync();
                Dictionary<int, string> existingEntries = new Dictionary<int, string>();
                IMongoCollection<BsonDocument> collection = null;

                if (collectionNames.Contains("pokemon_master_list"))
                {
                    collection = _database.GetCollection<BsonDocument>("pokemon_master_list");
                    existingEntries = (await collection.Find(new BsonDocument()).ToListAsync())
                        .ToDictionary(doc => doc["dex_num"].AsInt32, doc => doc["name"].AsString);

                    if (existingEntries.Count == dict.Count && !existingEntries.Except(dict).Any())
                        return;
                }
                else
                {
                    await _database.CreateCollectionAsync("pokemon_master_list");
                    collection = _database.GetCollection<BsonDocument>("pokemon_master_list");

                    if (collection == null)
                        throw new Exception("Failed to create Pokemon collection in MongoDB.");
                }

                var docs = dict.Select(entry => new BsonDocument
                {
                    { "dex_num", entry.Key },
                    { "name", entry.Value }
                }).ToList();

                if (docs.Count > 0)
                    await collection.InsertManyAsync(docs);
            }
            else
            {
                string filePath = "save_data/pokemon_master_list.json";
                Dictionary<int, string> existingEntries = new Dictionary<int, string>();

                if (System.IO.File.Exists(filePath))
                {
                    existingEntries = JsonSerializer.Deserialize<Dictionary<int, string>>(await System.IO.File.ReadAllTextAsync(filePath));
                    if (existingEntries.Count == dict.Count && !existingEntries.Except(dict).Any())
                        return;
                }

                string json = JsonSerializer.Serialize(dict);

                try
                {
                    if (!System.IO.Directory.Exists("save_data"))
                        System.IO.Directory.CreateDirectory("save_data");

                    await System.IO.File.WriteAllTextAsync(filePath, json);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error writing to file: {ex.Message}");
                }
            }
        }

        private async Task CreateDBPokemonCollection()
        {
            if (_database != null)
            {
                var collectionNames = await _database.ListCollectionNames().ToListAsync();
                if (collectionNames.Contains("pokemon"))
                    return;

                await _database.CreateCollectionAsync("pokemon");
            }
            else
            {
                string filePath = "save_data/pokemon";
                if (!System.IO.Directory.Exists(filePath))
                    System.IO.Directory.CreateDirectory(filePath);
            }
        }

        private async Task CreateDBTypeEffectivenessCollection()
        {
            if (_database != null)
            {
                var collectionNames = await _database.ListCollectionNames().ToListAsync();
                if (collectionNames.Contains("type_effectiveness"))
                    return;
                await _database.CreateCollectionAsync("type_effectiveness");
            }
            else
            {
                string filePath = "save_data/type_effectiveness";
                if (!System.IO.Directory.Exists(filePath))
                    System.IO.Directory.CreateDirectory(filePath);
            }
        }

        private async Task CreateDBPlayerPokemonCollection()
        {
            if (_database != null)
            {
                var collectionNames = await _database.ListCollectionNames().ToListAsync();
                if (collectionNames.Contains("player_pokemon"))
                {
                    playerPokemonIdCounter = (await _database.GetCollection<BsonDocument>("player_pokemon").Find(new BsonDocument()).SortByDescending(doc => doc["player_pokemon_id"]).FirstOrDefaultAsync())?["player_pokemon_id"].AsInt32 + 1 ?? 1;
                    return;
                }
                await _database.CreateCollectionAsync("player_pokemon");
            }
            else
            {
                string filePath = "save_data/player_pokemon";
                if (!System.IO.Directory.Exists(filePath))
                    System.IO.Directory.CreateDirectory(filePath);
                else
                {
                    var files = System.IO.Directory.GetFiles(filePath, "*.json");
                    if (files.Length > 0)
                    {
                        var maxId = files.Select(f => int.TryParse(System.IO.Path.GetFileNameWithoutExtension(f), out int id) ? id : 0).Max();
                        playerPokemonIdCounter = maxId + 1;
                    }
                }
            }
        }

        private async Task<Pokemon> CreatePokemonFromAPI(int dexNum)
        {
            var pokemon = await PokeApiService.GetPokemonByDexNumber(dexNum);
            if (pokemon == null)
                throw new Exception($"Failed to fetch Pokemon with DexNum {dexNum} from PokeAPI.");

            if (_database != null)
            {
                var collection = _database.GetCollection<BsonDocument>("pokemon");
                if (collection == null)
                    throw new Exception("Failed to get Pokemon collection from MongoDB.");
                if (collection.Find(Builders<BsonDocument>.Filter.Eq("dex_num", dexNum)).FirstOrDefault() != null)
                    return pokemon;

                var BsonDoc = pokemon.ToBson();

                await collection.InsertOneAsync(BsonDoc);
            }
            else
            {
                try
                {
                    string filePath = $"save_data/pokemon/{dexNum}.json";
                    if (System.IO.File.Exists(filePath))
                        return pokemon;

                    string json = JsonSerializer.Serialize(pokemon);
                    if (!System.IO.Directory.Exists("save_data/pokemon"))
                        System.IO.Directory.CreateDirectory("save_data/pokemon");
                    await System.IO.File.WriteAllTextAsync(filePath, json);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error writing Pokemon data to file: {ex.Message}");
                    return pokemon;
                }
            }

            return pokemon;
        }

        private async Task<TypeEffectiveness> CreateTypeEffectiveness(PokemonType type)
        {
            var effectiveness = await PokeApiService.GetTypeEffectiveness(type);
            if (effectiveness == null)
                throw new Exception($"Failed to fetch type effectiveness for {type} from PokeAPI.");

            if (_database != null)
            {
                var collection = _database.GetCollection<BsonDocument>("type_effectiveness");
                if (collection == null)
                    throw new Exception("Failed to get TypeEffectiveness collection from MongoDB.");
                if (collection.Find(Builders<BsonDocument>.Filter.Eq("type", type.ToString())).FirstOrDefault() != null)
                    return effectiveness;
                var BsonDoc = effectiveness.ToBson();
                await collection.InsertOneAsync(BsonDoc);
            }
            else
            {
                try
                {
                    string filePath = $"save_data/type_effectiveness/{type}.json";
                    if (System.IO.File.Exists(filePath))
                        return effectiveness;
                    string json = JsonSerializer.Serialize(effectiveness);
                    if (!System.IO.Directory.Exists("save_data/type_effectiveness"))
                        System.IO.Directory.CreateDirectory("save_data/type_effectiveness");
                    await System.IO.File.WriteAllTextAsync(filePath, json);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error writing TypeEffectiveness data to file: {ex.Message}");
                    return effectiveness;
                }
            }
            return effectiveness;
        }

        public async Task SavePlayerPokemonAsync(PlayerPokemon playerPokemon)
        {
            if (playerPokemon == null)
                throw new ArgumentNullException(nameof(playerPokemon));

            if (playerPokemon.PlayerPokemonId == -1)
                playerPokemon.PlayerPokemonId = playerPokemonIdCounter++;

            if (_database != null)
            {
                var collection = _database.GetCollection<BsonDocument>("player_pokemon");
                var filter = Builders<BsonDocument>.Filter.Eq("player_pokemon_id", playerPokemon.PlayerPokemonId);
                var updateOptions = new ReplaceOptions { IsUpsert = true };
                await collection.ReplaceOneAsync(filter, playerPokemon.ToBson(), updateOptions);
            }
            else
            {
                try
                {
                    string filePath = $"save_data/player_pokemon/{playerPokemon.PlayerPokemonId}.json";
                    string json = JsonSerializer.Serialize(playerPokemon);
                    if (!System.IO.Directory.Exists("save_data/player_pokemon"))
                        System.IO.Directory.CreateDirectory("save_data/player_pokemon");
                    await System.IO.File.WriteAllTextAsync(filePath, json);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error writing PlayerPokemon data to file: {ex.Message}");
                }
            }
        }

        public async Task SavePokemonTeamAsync(PokemonTeam team)
        {
            if (_database != null)
            {
                var collectionNames = await _database.ListCollectionNames().ToListAsync();
                if (!collectionNames.Contains("pokemon_teams"))
                {
                    await _database.CreateCollectionAsync("pokemon_teams");
                }

                var teamDoc = team.ToBson();
                teamDoc["team_id"] = "default";

                await _database.GetCollection<BsonDocument>("pokemon_teams").ReplaceOneAsync(Builders<BsonDocument>.Filter.Eq("team_id", "default"), teamDoc, new ReplaceOptions { IsUpsert = true });
            }
            else
            {
                string filePath = "save_data/pokemon_team.json";
                if (!System.IO.Directory.Exists("save_data"))
                    System.IO.Directory.CreateDirectory("save_data");

                string json = "{";

                for (int i = 0; i < team.Pokemon.Length; i++)
                {
                    json += $"\"Pokemon[{i}]\": {team.Pokemon[i].PlayerPokemonId},";
                }
                json = json.TrimEnd(',') + "}";

                try
                {
                    await System.IO.File.WriteAllTextAsync(filePath, json);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error writing PokemonTeam data to file: {ex.Message}");
                }
            }
        }

        public async Task<Dictionary<int, string>> GetPokemonMasterListAsync()
        {
            if (_database != null)
            {
                var collection = _database.GetCollection<BsonDocument>("pokemon_master_list");
                var documents = await collection.Find(new BsonDocument()).ToListAsync();
                return documents.ToDictionary(doc => doc["dex_num"].AsInt32, doc => doc["name"].AsString);
            }
            else
            {
                string filePath = "save_data/pokemon_master_list.json";
                if (System.IO.File.Exists(filePath))
                {
                    string json = await System.IO.File.ReadAllTextAsync(filePath);
                    return JsonSerializer.Deserialize<Dictionary<int, string>>(json);
                }
            }

            return new Dictionary<int, string>();
        }

        private void GetPokemonMasterList()
        {
            if (_database != null)
            {
                var collection = _database.GetCollection<BsonDocument>("pokemon_master_list");
                var documents = collection.Find(new BsonDocument()).ToList();
                pokemonMasterList = documents.ToDictionary(doc => doc["dex_num"].AsInt32, doc => doc["name"].AsString);
            }
            else
            {
                string filePath = "save_data/pokemon_master_list.json";
                if (System.IO.File.Exists(filePath))
                {
                    string json = System.IO.File.ReadAllText(filePath);
                    pokemonMasterList = JsonSerializer.Deserialize<Dictionary<int, string>>(json);
                }
            }
        }

        public async Task<Pokemon> GetPokemonByDexNumAsync(int dexNum)
        {
            if (_database != null)
            {
                var collection = _database.GetCollection<BsonDocument>("pokemon");
                var filter = Builders<BsonDocument>.Filter.Eq("dex_num", dexNum);
                var result = await collection.Find(filter).FirstOrDefaultAsync();
                if (result != null)
                {
                    return Pokemon.CreateFromBson(result);
                }
                else
                {
                    return await CreatePokemonFromAPI(dexNum);
                }
            }
            else
            {
                string filePath = $"save_data/pokemon/{dexNum}.json";
                if (System.IO.File.Exists(filePath))
                {
                    string json = await System.IO.File.ReadAllTextAsync(filePath);
                    return JsonSerializer.Deserialize<Pokemon>(json);
                }
                else
                {
                    return await CreatePokemonFromAPI(dexNum);
                }
            }
        }

        public async Task<TypeEffectiveness> GetTypeEffectivenessAsync(PokemonType type)
        {
            if (_database != null)
            {
                var collection = _database.GetCollection<BsonDocument>("type_effectiveness");
                var filter = Builders<BsonDocument>.Filter.Eq("type", type.ToString());
                var result = await collection.Find(filter).FirstOrDefaultAsync();
                if (result != null)
                {
                    return TypeEffectiveness.FromBson(result);
                }
                else
                {
                    return await CreateTypeEffectiveness(type);
                }
            }
            else
            {
                string filePath = $"save_data/type_effectiveness/{type}.json";
                if (System.IO.File.Exists(filePath))
                {
                    string json = await System.IO.File.ReadAllTextAsync(filePath);
                    return JsonSerializer.Deserialize<TypeEffectiveness>(json);
                }
                else
                {
                    return await CreateTypeEffectiveness(type);
                }
            }
        }

        public async Task<TypeEffectiveness> GetTypeEffectivenessAsync(IEnumerable<PokemonType?> types)
        {
            if (types == null || !types.Any())
                throw new ArgumentException("At least one type must be provided.");

            TypeEffectiveness combinedEffectiveness = new TypeEffectiveness
            {
                Type = types.FirstOrDefault() ?? PokemonType.Normal // Just for reference, the actual type doesn't matter here
            };
            foreach (var type in Enum.GetValues(typeof(PokemonType)).Cast<PokemonType>())
            {
                combinedEffectiveness.AttackEffectiveness[type] = 1.0f;
                combinedEffectiveness.DefenseEffectiveness[type] = 1.0f;
            }
            foreach (var type in types)
            {
                if (type == null)
                    continue;

                PokemonType existingType = type.Value;
                var effectiveness = await GetTypeEffectivenessAsync(existingType);
                foreach (var kvp in effectiveness.AttackEffectiveness)
                {
                    combinedEffectiveness.AttackEffectiveness[kvp.Key] *= kvp.Value;
                }
                foreach (var kvp in effectiveness.DefenseEffectiveness)
                {
                    combinedEffectiveness.DefenseEffectiveness[kvp.Key] *= kvp.Value;
                }
            }
            return combinedEffectiveness;
        }

        public async Task<PlayerPokemon> GetPlayerPokemonAsync(int playerPokemonId)
        {
            if (_database != null)
            {
                var collection = _database.GetCollection<BsonDocument>("player_pokemon");
                var filter = Builders<BsonDocument>.Filter.Eq("player_pokemon_id", playerPokemonId);
                var result = await collection.Find(filter).FirstOrDefaultAsync();
                if (result != null)
                {
                    return PlayerPokemon.CreateFromBson(result, this);
                }
            }
            else
            {
                string filePath = $"save_data/player_pokemon/{playerPokemonId}.json";
                if (System.IO.File.Exists(filePath))
                {
                    string json = await System.IO.File.ReadAllTextAsync(filePath);
                    return JsonSerializer.Deserialize<PlayerPokemon>(json);
                }
            }
            return null;
        }

        public async Task<PokemonTeam> LoadPokemonTeamAsync()
        {
            if (_database != null)
            {
                var collection = _database.GetCollection<BsonDocument>("pokemon_teams");
                var teamDoc = await collection.Find(Builders<BsonDocument>.Filter.Eq("team_id", "default")).FirstOrDefaultAsync();
                if (teamDoc != null)
                {
                    return PokemonTeam.FromBson(teamDoc, this);
                }
            }
            else
            {
                string filePath = "save_data/pokemon_team.json";
                if (System.IO.File.Exists(filePath))
                {
                    string json = await System.IO.File.ReadAllTextAsync(filePath);
                    var teamData = JsonSerializer.Deserialize<Dictionary<string, int>>(json);
                    if (teamData != null)
                    {
                        var pokemonIds = teamData.Values.ToArray();
                        var pokemons = new PlayerPokemon[pokemonIds.Length];
                        for (int i = 0; i < pokemonIds.Length; i++)
                        {
                            pokemons[i] = new PlayerPokemon { Pokemon = await GetPokemonByDexNumAsync(pokemonIds[i]), Level = 1, CurrentHP = 10 };
                        }
                        return new PokemonTeam(this) { Pokemon = pokemons };
                    }
                }
            }
            return new PokemonTeam(this) { Pokemon = new PlayerPokemon[6] };
        }
    }
}
