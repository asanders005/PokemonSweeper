using PokemonSweeper.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonSweeper.Game.PokemonModels
{
    public class PokemonTeam
    {
        private readonly Random _random = new Random();
        private readonly DAL _dal;

        public PlayerPokemon[] Pokemon { get; set; } = new PlayerPokemon[6];

        public PokemonTeam(DAL dal)
        {
            _dal = dal;
        }

        #region Battle Properties

        public PlayerPokemon ActivePokemon { get => Pokemon[ActivePokemonIndex]; }
        public int ActivePokemonIndex { get; set; }

        public bool HasUsablePokemon => Pokemon.Any(p => p != null && !p.IsFainted);

        #endregion

        #region Battle Methods

        /// <summary>
        /// Function to battle a wild Pokemon.
        /// </summary>
        /// <param name="opponent">The wild Pokemon to battle against.</param>
        /// <returns>True if the battle was successful, otherwise false.</returns>
        public async Task<bool> Battle(PlayerPokemon opponent)
        {
            while (HasUsablePokemon && !opponent.IsFainted)
            {
                bool playerGoesFirst = ActivePokemon.Speed >= opponent.Speed;

                bool playerUsesPhysicalAttack = (ActivePokemon.Attack == ActivePokemon.SpecialAttack) ?
                    opponent.Defense < opponent.SpecialDefense : ActivePokemon.Attack > ActivePokemon.SpecialAttack;
                bool opponentUsesPhysicalAttack = (opponent.Attack == opponent.SpecialAttack) ?
                    ActivePokemon.Defense < ActivePokemon.SpecialDefense : opponent.Attack > opponent.SpecialAttack;

                int damageToOpponent = playerUsesPhysicalAttack ?
                    Math.Max(1, ActivePokemon.Attack - opponent.Defense) :
                    Math.Max(1, ActivePokemon.SpecialAttack - opponent.SpecialDefense);

                TypeEffectiveness playerTypeEffectiveness = await _dal.GetTypeEffectivenessAsync(new PokemonType?[] { ActivePokemon.Pokemon.PrimaryType, ActivePokemon.Pokemon.SecondaryType });
                damageToOpponent = (int)(damageToOpponent * playerTypeEffectiveness.AttackEffectiveness[opponent.Pokemon.PrimaryType] *
                    (opponent.Pokemon.SecondaryType.HasValue ? playerTypeEffectiveness.AttackEffectiveness[opponent.Pokemon.SecondaryType.Value] : 1f));
                damageToOpponent += (int)(damageToOpponent * (float)(_random.NextDouble() * 0.2f) - 0.1f); // Add some randomness to the damage (-10% - 10%)

                int damageToPlayer = opponentUsesPhysicalAttack ?
                    Math.Max(1, opponent.Attack - ActivePokemon.Defense) :
                    Math.Max(1, opponent.SpecialAttack - ActivePokemon.SpecialDefense);

                TypeEffectiveness opponentTypeEffectiveness = await _dal.GetTypeEffectivenessAsync(new PokemonType?[] { opponent.Pokemon.PrimaryType, opponent.Pokemon.SecondaryType });
                damageToPlayer = (int)(damageToPlayer * opponentTypeEffectiveness.AttackEffectiveness[ActivePokemon.Pokemon.PrimaryType] *
                    (ActivePokemon.Pokemon.SecondaryType.HasValue ? opponentTypeEffectiveness.AttackEffectiveness[ActivePokemon.Pokemon.SecondaryType.Value] : 1f));
                damageToPlayer += (int)(damageToPlayer * (float)(_random.NextDouble() * 0.2f) - 0.1f); // Add some randomness to the damage (-10% - 10%)

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
                    Console.WriteLine($"{ActivePokemon.Pokemon.Name} has fainted!");
                    if (!HasUsablePokemon)
                    {
                        Console.WriteLine("All your Pokemon have fainted! You lost the battle.");
                        return false;
                    }
                    Console.WriteLine($"Switching to {ActivePokemon.Pokemon.Name}.");
                    NextPokemon();
                }

            }

            return true;
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
        }

        #endregion
    }
}
