using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using PokemonSweeper.Game;

namespace PokemonSweeper.API
{
    public static class PokeApiService
    {
        private static readonly HttpClient _httpClient = new HttpClient()
        {
            BaseAddress = new Uri(BaseUri)
        };

        private const string BaseUri = "https://pokeapi.co/api/v2/";


        public static async Task<Dictionary<int, string>> GetAllPokemon()
        {
            var response = await _httpClient.GetAsync("pokemon?limit=100000&offset=0");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var results = BsonDocument.Parse(content)["results"].AsBsonArray;

            Dictionary<int, string> pokemonDict = new Dictionary<int, string>();
            foreach (var result in results)
            {
                string name = result["name"].AsString;
                int pokedexNum = GetPokemonIdFromUrl(result["url"].AsString);
                pokemonDict[pokedexNum] = name;
            }

            return pokemonDict;
        }

        public static async Task<Pokemon> GetPokemonByDexNumber(int dexNum)
        {
            var response = await _httpClient.GetAsync($"pokemon/{dexNum}");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var pokemonData = BsonDocument.Parse(content);

            var sprites = pokemonData["sprites"];
            var stats = pokemonData["stats"].AsBsonDocument;
            var types = pokemonData["types"].AsBsonArray;

            return new Pokemon
            {
                DexNum = pokemonData["id"].AsInt32,
                Name = pokemonData["name"].AsString,
                DefaultSprite = sprites["front_default"].AsString,
                ShinySprite = sprites["front_shiny"].AsString,
                Stats = new Dictionary<PokemonStatsType, PokemonStat>
                {
                    { PokemonStatsType.HP, new PokemonStat { StatType = PokemonStatsType.HP, BaseValue = stats["HP"].AsInt32 } },
                    { PokemonStatsType.Attack, new PokemonStat { StatType = PokemonStatsType.Attack, BaseValue = stats["Attack"].AsInt32 } },
                    { PokemonStatsType.Defense, new PokemonStat { StatType = PokemonStatsType.Defense, BaseValue = stats["Defense"].AsInt32 } },
                    { PokemonStatsType.SpecialAttack, new PokemonStat { StatType = PokemonStatsType.SpecialAttack, BaseValue = stats["SpecialAttack"].AsInt32 } },
                    { PokemonStatsType.SpecialDefense, new PokemonStat { StatType = PokemonStatsType.SpecialDefense, BaseValue = stats["SpecialDefense"].AsInt32 } },
                    { PokemonStatsType.Speed, new PokemonStat { StatType = PokemonStatsType.Speed, BaseValue = stats["Speed"].AsInt32 } }
                },
                PrimaryType = (PokemonType)Enum.Parse(typeof(PokemonType), types[0]["type"]["name"].AsString, true),
                SecondaryType = types.Count > 1 ? (PokemonType?)Enum.Parse(typeof(PokemonType), types[1]["type"]["name"].AsString, true) : null
            };
        }

        private static int GetPokemonIdFromUrl(string url)
        {
            var segments = url.TrimEnd('/').Split('/');
            if (segments.Length < 2 || !int.TryParse(segments[^1], out int id))
                throw new Exception($"Invalid Pokemon URL: {url}");
            return id;
        }
    }
}
