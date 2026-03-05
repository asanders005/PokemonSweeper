using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonSweeper.Game.PokemonModels
{
    public class PokemonStat
    {
        public PokemonStatsType StatType { get; set; }
        public int BaseValue { get; set; }
        public int IV { get; set; } = 0;
        public int EV { get; set; } = 0;
        public PokemonNatureType NatureType { get; set; } = PokemonNatureType.Neutral;

        public int CalculateStat(int level)
        {
            int statValue = ((2 * BaseValue + IV + (EV / 4)) * level) / 100;

            if (StatType == PokemonStatsType.HP)
            {
                return statValue + level + 10;
            }

            float natureModifier = NatureType switch
            {
                PokemonNatureType.Beneficial => 1.1f,
                PokemonNatureType.Neutral => 1.0f,
                PokemonNatureType.Hindering => 0.9f,
                _ => 1.0f
            };

            return (int)((statValue + 5) * natureModifier);
        }
    }
}
