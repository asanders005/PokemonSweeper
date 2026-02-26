using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace PokemonSweeper.Game.Field.API
{
    public class PokeApiService
    {
        private static readonly HttpClient _httpClient = new HttpClient()
        {
            BaseAddress = new Uri(BaseUri)
        };

        private const string BaseUri = "https://pokeapi.co/api/v2/";
        private readonly IMongoDatabase _database;

        public PokeApiService(IMongoDatabase database)
        {
            _database = database;
        }

        public async Task CreateDBPokemonList()
        {
            var collectionNames = await _database.ListCollectionNames().ToListAsync();
            if (collectionNames.Contains("pokemon_master_list"))
                return;

            await _database.CreateCollectionAsync("pokemon_master_list");
            var collection = _database.GetCollection<BsonDocument>("pokemon_master_list");

            if (collection == null)
                throw new Exception("Failed to create Pokemon collection in MongoDB.");

            var response = await _httpClient.GetAsync("pokemon?limit=100000&offset=0");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var results = BsonDocument.Parse(content)["results"];

            var docs = new List<BsonDocument>();

            foreach (var result in results.AsBsonArray)
            {
                string name = result["name"].AsString;
                int pokedexNum = GetPokemonIdFromUrl(result["url"].AsString);

                var pokemonDoc = new BsonDocument
                {
                    { "dex_num", pokedexNum },
                    { "name", name }
                };

                docs.Add(pokemonDoc);
            }

            if (docs.Count > 0)
                await collection.InsertManyAsync(docs);
        }

        public async Task<Dictionary<int, string>> GetAllPokemon()
        {
            var collection = _database.GetCollection<BsonDocument>("pokemon_master_list");

            Dictionary<int, string> pokemonDict = new Dictionary<int, string>();

            foreach (var doc in await collection.Find(new BsonDocument()).ToListAsync())
            {
                int number = doc["dex_num"].AsInt32;
                string name = doc["name"].AsString;
                pokemonDict[number] = name;
            }
            return pokemonDict;
        }

        public async Task<Pokemon> GetPokemonByDexNumber(int dexNum)
        {
            var collection = _database.GetCollection<BsonDocument>("pokemon");
            if (collection == null)
                throw new Exception("Pokemon collection does not exist in MongoDB.");

            var filter = Builders<BsonDocument>.Filter.Eq("dex_num", dexNum);
            var pokemonDoc = await collection.Find(filter).FirstOrDefaultAsync();

            Pokemon pokemon = null;
            if (pokemonDoc != null)
            {

            }
            else
                pokemon = await GetPokemonFromAPI(dexNum);

            return pokemon;
        }

        private async Task CreatePokemonCollection()
        {
            var collectionNames = await _database.ListCollectionNames().ToListAsync();
            if (collectionNames.Contains("pokemon"))
                return;
            
            await _database.CreateCollectionAsync("pokemon");
        }

        private async Task<Pokemon> GetPokemonFromAPI(int number)
        {
            // Temporary Implementation - In a real implementation, this would call the PokeAPI to get the Pokemon details
            return new Pokemon
            {
                Number = number,
                Name = $"Pokemon {number}"
            };
        }

        private int GetPokemonIdFromUrl(string url)
        {
            var segments = url.TrimEnd('/').Split('/');
            if (segments.Length < 2 || !int.TryParse(segments[^1], out int id))
                throw new Exception($"Invalid Pokemon URL: {url}");
            return id;
        }
    }
}
