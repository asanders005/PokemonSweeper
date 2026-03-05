using MongoDB.Bson;
using MongoDB.Driver;
using PokemonSweeper.API;
using PokemonSweeper.Game;
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
        public Dictionary<int, string> PokemonMasterList 
        { get
            {
                if (pokemonMasterList.Count == 0)
                {
                    GetPokemonMasterList();
                }
                return pokemonMasterList;
            } }
        private Dictionary<int, string> pokemonMasterList = new Dictionary<int, string>();

        public DAL(IMongoDatabase database = null)
        {
            _database = database;
            _ = CreatePokemonMasterList();
            _ = CreateDBPokemonCollection();
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
    }
}
