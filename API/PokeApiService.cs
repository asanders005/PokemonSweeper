using System.Net.Http;
using System.Threading.Tasks;

namespace PokemonSweeper.Game.Field.API
{
    public static class PokeApiService
    {
        public static async Task GetAndSavePokemonAsync()
        {
            
        }

        private const string BaseUri = "https://pokeapi.co/api/v2/";
        private static readonly HttpClient _httpClient = new HttpClient()
        {
            BaseAddress = new System.Uri(BaseUri)
        };
    }
}
