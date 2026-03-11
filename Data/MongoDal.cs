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
    /// <summary>
    /// A class to access and manage save data for the Pokemon Sweeper game. 
    /// It can use either a MongoDB database or the local file system for storage, depending on whether or not a MongoDB connection is provided in Program.cs. 
    /// It provides methods to save and load player Pokemon, Pokemon teams, and to fetch Pokemon data and type effectiveness information from the PokeAPI, caching results in the chosen storage method for future access.
    /// </summary>
    public class MongoDal : IDal
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

        public MongoDal(IMongoDatabase database)
        {
            _database = database;
            _ = CreatePokemonMasterList();
            _ = CreateDBPokemonCollection();
            _ = CreateDBTypeEffectivenessCollection();
            _ = CreateDBPlayerPokemonCollection();
        }

        /// <summary>
        /// Creates the master list of all Pokemon by fetching data from the PokeAPI and storing it in either MongoDB or a local JSON file.
        /// </summary>
        /// <exception cref="Exception">Thrown if there is an error creating the master list.</exception>
        private async Task CreatePokemonMasterList()
        {
            Dictionary<int, string> dict = await PokeApiService.GetAllPokemon();

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

        /// <summary>
        /// Creates the necessary collection for storing Pokemon data in MongoDB, or a directory for storing Pokemon JSON files if using the file system. 
        /// If the collection or directory already exists, it does nothing. 
        /// This method is called during DAL initialization to ensure the necessary storage structures are in place before any data operations are performed.
        /// </summary>
        private async Task CreateDBPokemonCollection()
        {

            var collectionNames = await _database.ListCollectionNames().ToListAsync();
            if (collectionNames.Contains("pokemon"))
                return;

            await _database.CreateCollectionAsync("pokemon");
        }

        /// <summary>
        /// Creates the necessary collection for storing type effectiveness data in MongoDB, or a directory for storing type effectiveness JSON files if using the file system.
        /// If the collection or directory already exists, it does nothing. 
        /// This method is called during DAL initialization to ensure the necessary storage structures are in place before any data operations are performed.
        /// </summary>
        private async Task CreateDBTypeEffectivenessCollection()
        {
            var collectionNames = await _database.ListCollectionNames().ToListAsync();
            if (collectionNames.Contains("type_effectiveness"))
                return;
            await _database.CreateCollectionAsync("type_effectiveness");
        }

        /// <summary>
        /// Creates the necessary collection for storing player Pokemon data in MongoDB, or a directory for storing player Pokemon JSON files if using the file system.
        /// If the collection or directory already exists, it checks for existing player Pokemon entries to determine the next available unique ID for new PlayerPokemon instances, and then does nothing else.
        /// This method is called during DAL initialization to ensure the necessary storage structures are in place before any data operations are performed.
        /// </summary>
        /// <returns></returns>
        private async Task CreateDBPlayerPokemonCollection()
        {
            var collectionNames = await _database.ListCollectionNames().ToListAsync();
            if (collectionNames.Contains("player_pokemon"))
            {
                playerPokemonIdCounter = (await _database.GetCollection<BsonDocument>("player_pokemon").Find(new BsonDocument()).SortByDescending(doc => doc["player_pokemon_id"]).FirstOrDefaultAsync())?["player_pokemon_id"].AsInt32 + 1 ?? 1;
                return;
            }
            await _database.CreateCollectionAsync("player_pokemon");
        }

        /// <summary>
        /// Creates a Pokemon object by fetching data from the PokeAPI using the provided DexNum.
        /// If the Pokemon already exists in the database or as a JSON file, it returns the existing Pokemon.
        /// </summary>
        /// <param name="dexNum">The PokeDex number of the Pokemon to fetch.</param>
        /// <returns>The fetched or existing Pokemon object.</returns>
        /// <exception cref="Exception">Thrown if the Pokemon cannot be fetched from the PokeAPI.</exception>
        private async Task<Pokemon> CreatePokemonFromAPI(int dexNum)
        {
            var pokemon = await PokeApiService.GetPokemonByDexNumber(dexNum);
            if (pokemon == null)
                throw new Exception($"Failed to fetch Pokemon with DexNum {dexNum} from PokeAPI.");

            var collection = _database.GetCollection<BsonDocument>("pokemon");
            if (collection == null)
                throw new Exception("Failed to get Pokemon collection from MongoDB.");
            if (collection.Find(Builders<BsonDocument>.Filter.Eq("dex_num", dexNum)).FirstOrDefault() != null)
                return pokemon;

            var BsonDoc = pokemon.ToBson();

            await collection.InsertOneAsync(BsonDoc);

            return pokemon;
        }

        /// <summary>
        /// Creates a TypeEffectiveness object by fetching data from the PokeAPI using the provided PokemonType.
        /// If the TypeEffectiveness already exists in the database or as a JSON file, it returns the existing TypeEffectiveness.
        /// </summary>
        /// <param name="type">The PokemonType for which to fetch type effectiveness.</param>
        /// <returns>The fetched or existing TypeEffectiveness object.</returns>
        /// <exception cref="Exception">Thrown if the TypeEffectiveness cannot be fetched from the PokeAPI.</exception>
        private async Task<TypeEffectiveness> CreateTypeEffectiveness(PokemonType type)
        {
            var effectiveness = await PokeApiService.GetTypeEffectiveness(type);
            if (effectiveness == null)
                throw new Exception($"Failed to fetch type effectiveness for {type} from PokeAPI.");

            var collection = _database.GetCollection<BsonDocument>("type_effectiveness");
            if (collection == null)
                throw new Exception("Failed to get TypeEffectiveness collection from MongoDB.");
            if (collection.Find(Builders<BsonDocument>.Filter.Eq("type", type.ToString())).FirstOrDefault() != null)
                return effectiveness;
            var BsonDoc = effectiveness.ToBson();
            await collection.InsertOneAsync(BsonDoc);

            return effectiveness;
        }

        /// <summary>
        /// Saves a PlayerPokemon object to the database or as a JSON file, depending on the storage method being used.
        /// </summary>
        /// <param name="playerPokemon">The PlayerPokemon object to save.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the playerPokemon parameter is null.</exception>
        public async Task SavePlayerPokemonAsync(PlayerPokemon playerPokemon)
        {
            if (playerPokemon == null)
                throw new ArgumentNullException(nameof(playerPokemon));

            if (playerPokemon.PlayerPokemonId == -1)
                playerPokemon.PlayerPokemonId = playerPokemonIdCounter++;

            var collection = _database.GetCollection<BsonDocument>("player_pokemon");
            var filter = Builders<BsonDocument>.Filter.Eq("player_pokemon_id", playerPokemon.PlayerPokemonId);
            var updateOptions = new ReplaceOptions { IsUpsert = true };
            await collection.ReplaceOneAsync(filter, playerPokemon.ToBson(), updateOptions);
        }

        /// <summary>
        /// Saves a collection of PlayerPokemon objects to the database or as JSON files, depending on the storage method being used.
        /// </summary>
        /// <param name="playerPokemons">The collection of PlayerPokemon objects to save.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the playerPokemons parameter is null.</exception>
        public async Task SavePlayerPokemonAsync(IEnumerable<PlayerPokemon> playerPokemons)
        {
            if (playerPokemons == null)
                throw new ArgumentNullException(nameof(playerPokemons));
            foreach (var playerPokemon in playerPokemons)
            {
                await SavePlayerPokemonAsync(playerPokemon);
            }
        }

        /// <summary>
        /// Saves a PokemonTeam object to the database or as a JSON file, depending on the storage method being used.
        /// </summary>
        /// <param name="team">The PokemonTeam object to save.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task SavePokemonTeamAsync(PokemonTeam team)
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

        /// <summary>
        /// Retrieves the master list of all Pokemon, which is a dictionary mapping PokeDex numbers to Pokemon names.
        /// </summary>
        /// <returns>A task representing the asynchronous operation, with a dictionary mapping PokeDex numbers to Pokemon names as the result.</returns>
        public async Task<Dictionary<int, string>> GetPokemonMasterListAsync()
        {
            var collection = _database.GetCollection<BsonDocument>("pokemon_master_list");
            var documents = await collection.Find(new BsonDocument()).ToListAsync();
            return documents.ToDictionary(doc => doc["dex_num"].AsInt32, doc => doc["name"].AsString);
        }

        /// <summary>
        /// Retrieves the master list of all Pokemon and stores it in the pokemonMasterList field.
        /// </summary>
        private void GetPokemonMasterList()
        {
            var collection = _database.GetCollection<BsonDocument>("pokemon_master_list");
            var documents = collection.Find(new BsonDocument()).ToList();
            pokemonMasterList = documents.ToDictionary(doc => doc["dex_num"].AsInt32, doc => doc["name"].AsString);
        }

        /// <summary>
        /// Retrieves a Pokemon object by its PokeDex number. 
        /// It first checks if the Pokemon exists in the database or as a JSON file, and returns it if found. 
        /// If not found, it fetches the Pokemon data from the PokeAPI, creates a new Pokemon object, saves it to the database or as a JSON file for future access, and then returns the newly created Pokemon object.
        /// </summary>
        /// <param name="dexNum">The PokeDex number of the Pokemon to retrieve.</param>
        /// <returns>A task representing the asynchronous operation, with the retrieved or newly created Pokemon object as the result.</returns>
        public async Task<Pokemon> GetPokemonByDexNumAsync(int dexNum)
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

        /// <summary>
        /// Gets a random Pokemon whose Base Stat Total (BST) falls within the specified minimum and maximum range.
        /// </summary>
        /// <param name="minBst">The minimum BST value.</param>
        /// <param name="maxBst">The maximum BST value.</param>
        /// <returns>A task representing the asynchronous operation, with the retrieved Pokemon object as the result.</returns>
        public async Task<Pokemon> GetRandomPokemonByBstAsync(int minBst, int maxBst)
        {
            var allPokemon = PokemonMasterList.Keys.ToList();
            var random = new Random();
            while (true)
            {
                int randomDexNum = allPokemon[random.Next(allPokemon.Count)];
                var pokemon = await GetPokemonByDexNumAsync(randomDexNum);
                if (pokemon.BST >= minBst && pokemon.BST <= maxBst)
                    return pokemon;
            }
        }

        /// <summary>
        /// Retrieves the type effectiveness information for a given PokemonType.
        /// It first checks if the information exists in the database or as a JSON file, and returns it if found.
        /// If not found, it fetches the type effectiveness data from the PokeAPI, creates a new TypeEffectiveness object, saves it to the database or as a JSON file for future access, and then returns the newly created TypeEffectiveness object.
        /// </summary>
        /// <param name="type">The PokemonType for which to retrieve type effectiveness information.</param>
        /// <returns>A task representing the asynchronous operation, with the retrieved or newly created TypeEffectiveness object as the result.</returns>
        public async Task<TypeEffectiveness> GetTypeEffectivenessAsync(PokemonType type)
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

        /// <summary>
        /// Retrieves the combined type effectiveness information for a collection of Pokemon types.
        /// </summary>
        /// <param name="types">The collection of Pokemon types for which to retrieve combined type effectiveness information.</param>
        /// <returns>A task representing the asynchronous operation, with the combined TypeEffectiveness object as the result.</returns>
        /// <exception cref="ArgumentException">Thrown if the types collection is null or empty.</exception>
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

        /// <summary>
        /// Retrieves a PlayerPokemon object by its unique playerPokemonId.
        /// </summary>
        /// <param name="playerPokemonId">The unique identifier of the PlayerPokemon to retrieve.</param>
        /// <returns>A task representing the asynchronous operation, with the retrieved PlayerPokemon object as the result.</returns>
        public async Task<PlayerPokemon> GetPlayerPokemonAsync(int playerPokemonId)
        {
            var collection = _database.GetCollection<BsonDocument>("player_pokemon");
            var filter = Builders<BsonDocument>.Filter.Eq("player_pokemon_id", playerPokemonId);
            var result = await collection.Find(filter).FirstOrDefaultAsync();
            if (result != null)
            {
                return PlayerPokemon.CreateFromBson(result, this);
            }

            return null;
        }

        /// <summary>
        /// Loads the player's Pokemon team from the database or from a JSON file, depending on the storage method being used.
        /// </summary>
        /// <returns>A task representing the asynchronous operation, with the loaded PokemonTeam object as the result.</returns>
        public async Task<PokemonTeam> LoadPokemonTeamAsync()
        {
            var collection = _database.GetCollection<BsonDocument>("pokemon_teams");
            var teamDoc = await collection.Find(Builders<BsonDocument>.Filter.Eq("team_id", "default")).FirstOrDefaultAsync();
            if (teamDoc != null)
            {
                return PokemonTeam.FromBson(teamDoc, this);
            }

            return new PokemonTeam(this) { Pokemon = new PlayerPokemon[6] };
        }
    }
}
