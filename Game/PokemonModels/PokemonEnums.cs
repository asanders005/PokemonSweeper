using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonSweeper.Game.PokemonModels
{
    public enum PokemonType
    {
        Normal,
        Fire,
        Water,
        Electric,
        Grass,
        Ice,
        Fighting,
        Poison,
        Ground,
        Flying,
        Psychic,
        Bug,
        Rock,
        Ghost,
        Dragon,
        Dark,
        Steel,
        Fairy
    }

    public enum PokemonStatsType
    {
        HP,
        Attack,
        Defense,
        SpecialAttack,
        SpecialDefense,
        Speed
    }

    public enum PokemonNatureType
    {
        Beneficial,
        Neutral,
        Hindering
    }
}
