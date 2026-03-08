using PokemonSweeper.Data;
using PokemonSweeper.Game.PokemonModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonSweeper.Services
{
    public class PokemonTeamService
    {
        public PokemonTeamService(DAL dal)
        {
            _dal = dal;
        }

        public PokemonTeam CurrentTeam { get; set; } = null!;

        private readonly DAL _dal;
    }
}
