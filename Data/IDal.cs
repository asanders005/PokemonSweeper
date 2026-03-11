using PokemonSweeper.Game.PokemonModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonSweeper.Data
{
    public interface IDal
    {
        private static int playerPokemonCounter;  // This will be used to assign unique IDs to PlayerPokemon instances

        public Dictionary<int, string> PokemonMasterList { get; }

        public Task SavePlayerPokemonAsync(PlayerPokemon playerPokemon);
        public Task SavePlayerPokemonAsync(IEnumerable<PlayerPokemon> playerPokemons);
        public Task SavePokemonTeamAsync(PokemonTeam team);
        public Task<Dictionary<int, string>> GetPokemonMasterListAsync();
        public Task<Pokemon> GetPokemonByDexNumAsync(int dexNum);
        public Task<Pokemon> GetRandomPokemonByBstAsync(int minBst, int maxBst);
        public Task<TypeEffectiveness> GetTypeEffectivenessAsync(PokemonType type);
        public Task<TypeEffectiveness> GetTypeEffectivenessAsync(IEnumerable<PokemonType?> types);
        public Task<PlayerPokemon> GetPlayerPokemonAsync(int playerPokemonId);
        public Task<PokemonTeam> LoadPokemonTeamAsync();
    }
}
