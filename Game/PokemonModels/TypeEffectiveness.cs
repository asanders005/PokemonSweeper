using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonSweeper.Game.PokemonModels
{
    public class TypeEffectiveness
    {
        public PokemonType Type { get; set; }

        public Dictionary<PokemonType, float> AttackEffectiveness { get; set; } = new Dictionary<PokemonType, float>();
        public Dictionary<PokemonType, float> DefenseEffectiveness { get; set; } = new Dictionary<PokemonType, float>();

        /// <summary>
        /// Converts the TypeEffectiveness instance into a BsonDocument for storage in MongoDB.
        /// </summary>
        /// <returns>A BsonDocument representing the TypeEffectiveness instance.</returns>
        public BsonDocument ToBson()
        {
            var doc = new BsonDocument
            {
                { "type", Type.ToString() },
                { "attackEffectiveness", new BsonDocument(AttackEffectiveness.Select(kv => new BsonElement(kv.Key.ToString(), kv.Value))) },
                { "defenseEffectiveness", new BsonDocument(DefenseEffectiveness.Select(kv => new BsonElement(kv.Key.ToString(), kv.Value))) }
            };
            return doc;
        }

        /// <summary>
        /// Creates a TypeEffectiveness instance from a BsonDocument, typically retrieved from MongoDB.
        /// </summary>
        /// <param name="doc">The BsonDocument representing the TypeEffectiveness instance.</param>
        /// <returns>A TypeEffectiveness instance created from the BsonDocument.</returns>
        public static TypeEffectiveness FromBson(BsonDocument doc)
        {
            var typeEffectiveness = new TypeEffectiveness
            {
                Type = (PokemonType)Enum.Parse(typeof(PokemonType), doc["type"].AsString, true),
                AttackEffectiveness = doc["attackEffectiveness"].AsBsonDocument.ToDictionary(kv => (PokemonType)Enum.Parse(typeof(PokemonType), kv.Name, true), kv => (float)kv.Value.AsDouble),
                DefenseEffectiveness = doc["defenseEffectiveness"].AsBsonDocument.ToDictionary(kv => (PokemonType)Enum.Parse(typeof(PokemonType), kv.Name, true), kv => (float)kv.Value.AsDouble)
            };
            return typeEffectiveness;
        }
    }
}
