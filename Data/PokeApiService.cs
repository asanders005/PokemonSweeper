using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using PokemonSweeper.Game.PokemonModels;

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
                int pokedexNum = GetPokemonIdFromUrl(result["url"].AsString);
                if (pokedexNum <= 0 || pokedexNum > 10000)
                    continue; // Skip invalid dex numbers and special forms

                string name = result["name"].AsString;
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
            var stats = pokemonData["stats"].AsBsonArray;
            var types = pokemonData["types"].AsBsonArray;

            return new Pokemon
            {
                DexNum = pokemonData["id"].AsInt32,
                Name = pokemonData["name"].AsString,
                DefaultSprite = sprites["front_default"].AsString,
                ShinySprite = sprites["front_shiny"].AsString,
                BaseStats = new Dictionary<PokemonStatsType, int>
                {
                    { PokemonStatsType.HP, stats[0]["base_stat"].AsInt32 },
                    { PokemonStatsType.Attack, stats[1]["base_stat"].AsInt32 },
                    { PokemonStatsType.Defense, stats[2]["base_stat"].AsInt32 },
                    { PokemonStatsType.SpecialAttack, stats[3]["base_stat"].AsInt32 },
                    { PokemonStatsType.SpecialDefense, stats[4]["base_stat"].AsInt32 },
                    { PokemonStatsType.Speed, stats[5]["base_stat"].AsInt32 }
                },
                EvYield = new Dictionary<PokemonStatsType, int>
                {
                    { PokemonStatsType.HP, stats[0]["effort"].AsInt32 },
                    { PokemonStatsType.Attack, stats[1]["effort"].AsInt32 },
                    { PokemonStatsType.Defense, stats[2]["effort"].AsInt32 },
                    { PokemonStatsType.SpecialAttack, stats[3]["effort"].AsInt32 },
                    { PokemonStatsType.SpecialDefense, stats[4]["effort"].AsInt32 },
                    { PokemonStatsType.Speed, stats[5]["effort"].AsInt32 }
                },
                BaseExpYield = pokemonData["base_experience"].AsInt32,
                PrimaryType = (PokemonType)Enum.Parse(typeof(PokemonType), types[0]["type"]["name"].AsString, true),
                SecondaryType = types.Count > 1 ? (PokemonType?)Enum.Parse(typeof(PokemonType), types[1]["type"]["name"].AsString, true) : null
            };
        }

        public static async Task<TypeEffectiveness> GetTypeEffectiveness(PokemonType type)
        {
            var response = await _httpClient.GetAsync($"type/{type.ToString().ToLower()}");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var typeData = BsonDocument.Parse(content);
            var damageRelations = typeData["damage_relations"];
            var effectiveness = new TypeEffectiveness
            {
                Type = type
            };

            foreach (var relation in damageRelations.AsBsonDocument)
            {
                string relationType = relation.Name;
                var relatedTypes = relation.Value.AsBsonArray;
                foreach (var relatedType in relatedTypes)
                {
                    PokemonType relatedPokemonType = (PokemonType)Enum.Parse(typeof(PokemonType), relatedType["name"].AsString, true);
                    float multiplier = relationType switch
                    {
                        "double_damage_to" => 2.0f,
                        "half_damage_to" => 0.5f,
                        "no_damage_to" => 0.0f,
                        "double_damage_from" => 2.0f,
                        "half_damage_from" => 0.5f,
                        "no_damage_from" => 0.0f,
                        _ => throw new Exception($"Unknown damage relation type: {relationType}")
                    };
                    if (relationType.EndsWith("to"))
                        effectiveness.AttackEffectiveness[relatedPokemonType] = multiplier;
                    else
                        effectiveness.DefenseEffectiveness[relatedPokemonType] = multiplier;
                }
            }
            return effectiveness;
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
