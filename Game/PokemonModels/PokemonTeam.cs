using MongoDB.Bson;
using PokemonSweeper.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PokemonSweeper.Game.PokemonModels
{
    public class PokemonTeam
    {
        private readonly Random _random = new Random();
        private readonly IDal _dal;

        public PlayerPokemon[] Pokemon { get; set; } = new PlayerPokemon[6];

        public PokemonTeam(IDal dal)
        {
            _dal = dal;
        }

        #region Battle Properties

        public int AverageLevel => Pokemon.Where(p => p != null).Select(p => p.Level).DefaultIfEmpty(1).Average() is double avgLevel ? (int)avgLevel : 1;

        public PlayerPokemon ActivePokemon { get => Pokemon[ActivePokemonIndex]; }
        public int ActivePokemonIndex { get; set; }

        public bool HasUsablePokemon => Pokemon.Any(p => p != null && !p.IsFainted);

        #endregion

        #region Battle Methods

        /// <summary>
        /// Function to battle a wild Pokemon.
        /// </summary>
        /// <param name="opponent">The wild Pokemon to battle against.</param>
        /// <returns>A tuple containing a boolean indicating if the player won the battle and a list of the Pokemon that fainted during the battle.</returns>
        public async Task<(bool, List<PlayerPokemon>)> Battle(PlayerPokemon opponent)
        {
            if (!HasUsablePokemon)
            {
                Console.WriteLine("No usable Pokemon in the team! You lost the battle.");
                return (false, new List<PlayerPokemon>());
            }

            List<PlayerPokemon> faintedPokemon = new List<PlayerPokemon>();
            while (HasUsablePokemon && !opponent.IsFainted)
            {
                bool playerGoesFirst = ActivePokemon.Speed >= opponent.Speed;

                bool playerUsesPhysicalAttack = (ActivePokemon.Attack == ActivePokemon.SpecialAttack) ?
                    opponent.Defense < opponent.SpecialDefense : ActivePokemon.Attack > ActivePokemon.SpecialAttack;
                bool opponentUsesPhysicalAttack = (opponent.Attack == opponent.SpecialAttack) ?
                    ActivePokemon.Defense < ActivePokemon.SpecialDefense : opponent.Attack > opponent.SpecialAttack;

                int damageToOpponent = await CalculateDamage(ActivePokemon, opponent, playerUsesPhysicalAttack);

                int damageToPlayer = await CalculateDamage(opponent, ActivePokemon, opponentUsesPhysicalAttack);

                if (playerGoesFirst)
                {
                    AttackOpponent(opponent, damageToOpponent);
                    if (!opponent.IsFainted)
                        AttackActivePokemon(damageToPlayer);
                }
                else
                {
                    AttackActivePokemon(damageToPlayer);
                    if (!ActivePokemon.IsFainted)
                        AttackOpponent(opponent, damageToOpponent);
                }
                if (ActivePokemon.IsFainted)
                {
                    faintedPokemon.Add(ActivePokemon);
                    Console.WriteLine($"{ActivePokemon.Pokemon.Name} has fainted!");
                    if (!HasUsablePokemon)
                    {
                        Console.WriteLine("All your Pokemon have fainted! You lost the battle.");
                        return (false, faintedPokemon);
                    }
                    NextPokemon();
                    Console.WriteLine($"Switching to {ActivePokemon.Pokemon.Name}.");
                }

            }

            return (true, faintedPokemon);
        }

        /// <summary>
        /// Calculates the damage dealt by the attacker to the defender based on their stats, the type effectiveness of the attack, and a random factor.
        /// </summary>
        /// <param name="attacker">The attacking Pokemon.</param>
        /// <param name="defender">The defending Pokemon.</param>
        /// <param name="isPhysical">Indicates whether the attack is physical or special.</param>
        /// <returns>The calculated damage as an integer.</returns>
        private async Task<int> CalculateDamage(PlayerPokemon attacker, PlayerPokemon defender, bool isPhysical)
        {
            int critical = _random.NextDouble() < 0.0625 ? 2 : 1; // 6.25% chance for a critical hit
            float statsModifier = isPhysical ? (float)attacker.Attack / defender.Defense : (float)attacker.SpecialAttack / defender.SpecialDefense;
            int baseDamage = (int)((((((2 * attacker.Level * critical) / 5) + 2) * statsModifier) / 50) + 2);

            TypeEffectiveness attackTypeEffectiveness = await _dal.GetTypeEffectivenessAsync(new PokemonType?[] { attacker.Pokemon.PrimaryType, attacker.Pokemon.SecondaryType });
            int typeModifiedDamage = (int)Math.Round(baseDamage * attackTypeEffectiveness.AttackEffectiveness[defender.Pokemon.PrimaryType] *
                (defender.Pokemon.SecondaryType.HasValue ? attackTypeEffectiveness.AttackEffectiveness[defender.Pokemon.SecondaryType.Value] : 1f));

            float randomFactor = typeModifiedDamage > 1 ? _random.Next(217, 256) / 255f : 1f; // Random factor between 0.85 and 1.00
            return (int)Math.Round(typeModifiedDamage * randomFactor); // Apply the random factor to the damage
        }

        /// <summary>
        /// Attacks the active Pokemon with the specified damage. If the active Pokemon has fainted, it does nothing.
        /// </summary>
        /// <param name="damageToPlayer">The amount of damage to deal to the active Pokemon.</param>
        private void AttackActivePokemon(int damageToPlayer)
        {
            if (ActivePokemon == null || ActivePokemon.IsFainted)
                return;

            ActivePokemon.CurrentHP -= damageToPlayer;
        }

        /// <summary>
        /// Attacks the opponent Pokemon and checks if it has fainted. If the opponent faints, grants experience and EV rewards to the active Pokemon.
        /// </summary>
        /// <param name="opponent">The opponent Pokemon to attack.</param>
        /// <param name="damageToOpponent">The amount of damage to deal to the opponent.</param>
        private void AttackOpponent(PlayerPokemon opponent, int damageToOpponent)
        {
            opponent.CurrentHP -= damageToOpponent;

            if (opponent.IsFainted)
            {
                Console.WriteLine($"{opponent.Pokemon.Name} fainted! You won the battle.");
                ActivePokemon.GrantBattleRewards(opponent.CalculateExpYield(), 
                    opponent.Stats.Select(s => new { s.Key, s.Value.EV }).Where(s => s.EV > 0).ToDictionary(s => s.Key, s => s.EV));
            }
        }

        /// <summary>
        /// Switches to the next available Pokemon in the team if the current active Pokemon has fainted.
        /// </summary>
        public void NextPokemon()
        {
            while (ActivePokemon == null || ActivePokemon.IsFainted)
            {
                ActivePokemonIndex++;
                if (ActivePokemonIndex >= Pokemon.Length)
                {
                    Console.WriteLine("No more usable Pokemon in the team!");
                    return;
                }
            }
        }

        /// <summary>
        /// Resets the HP of all Pokemon in the team to their maximum HP.
        /// </summary>
        public void RestTeam()
        {
            foreach (var pokemon in Pokemon)
            {
                if (pokemon != null)
                    pokemon.ResetHP();
            }
            ActivePokemonIndex = 0;
        }

        /// <summary>
        /// Grants experience points to all Pokemon in the team based on the experience yield of the defeated opponent Pokemon. 
        /// The experience gain is divided equally among all non-fainted Pokemon in the team.
        /// Used after successfully clearing a minesweeper board to reward the player's team for their victory.
        /// </summary>
        /// <param name="pokemons">The list of defeated opponent Pokemon.</param>
        /// <returns>The amount of experience points awarded to each Pokemon in the team.</returns>
        public int AwardExpToTeam(IEnumerable<PlayerPokemon> pokemons)
        {
            int awakePokemonCount = Pokemon.Count(p => p != null && !p.IsFainted);
            if (awakePokemonCount == 0)
                return 0;

            var expGain = pokemons.Sum(p => p.CalculateExpYield()) / awakePokemonCount;
            foreach (var pokemon in Pokemon)
            {
                if (pokemon != null && !pokemon.IsFainted)
                    pokemon.GrantBattleRewards(expGain, new Dictionary<PokemonStatsType, int>());
            }
            return expGain;
        }

        #endregion

        #region Data Methods

        /// <summary>
        /// Converts the PokemonTeam instance into a BsonDocument for storage in MongoDB. 
        /// Each Pokemon in the team is stored with a key corresponding to its position in the team (e.g., "Pokemon0", "Pokemon1", etc.) and the value being the PlayerPokemonId of that Pokemon. 
        /// If a slot in the team is empty (null), it is not included in the BsonDocument.
        /// </summary>
        /// <returns>A BsonDocument representing the PokemonTeam instance.</returns>
        public BsonDocument ToBson()
        {
            var doc = new BsonDocument();
            for (int i = 0; i < Pokemon.Length; i++)
            {
                if (Pokemon[i] != null)
                    doc.Add($"Pokemon{i}", Pokemon[i].PlayerPokemonId);
            }
            return doc;
        }

        /// <summary>
        /// Creates a PokemonTeam instance from a BsonDocument, typically retrieved from MongoDB.
        /// </summary>
        /// <param name="doc">The BsonDocument containing the PokemonTeam data.</param>
        /// <param name="Dal">The data access layer (IDal) instance used to retrieve PlayerPokemon instances.</param>
        /// <returns>A PokemonTeam instance populated with the data from the BsonDocument.</returns>
        public static PokemonTeam FromBson(BsonDocument doc, IDal dal)
        {
            var team = new PokemonTeam(dal);
            for (int i = 0; i < 6; i++)
            {
                if (doc.Contains($"Pokemon{i}"))
                {
                    var playerPokemonId = doc[$"Pokemon{i}"].AsInt32;
                    team.Pokemon[i] = dal.GetPlayerPokemonAsync(playerPokemonId).Result;
                }
            }
            return team;
        }

        /// <summary>
        /// Saves the current state of the PlayerPokemon in the team to the database by iterating through each Pokemon in the team and calling the SavePlayerPokemonAsync method of the DAL for each non-null Pokemon.
        /// </summary>
        /// <returns>A task representing the asynchronous save operation.</returns>
        public async Task SaveTeam()
        {
            foreach (var pokemon in Pokemon)
            {
                if (pokemon != null)
                    await _dal.SavePlayerPokemonAsync(pokemon);
            }
        }

        #endregion
    }
}
