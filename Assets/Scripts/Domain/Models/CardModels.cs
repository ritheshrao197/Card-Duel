using System;
using System.Collections.Generic;

namespace CardDuel.Domain.Models
{
    /// <summary>
    /// Core card model - pure C#, no Unity dependencies
    /// </summary>
    public class Card
    {
        public int Id { get; }
        public string Name { get; }
        public int Cost { get; }
        public int Power { get; }
        public CardAbility Ability { get; }

        public Card(int id, string name, int cost, int power, CardAbility ability = null)
        {
            Id = id;
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Cost = cost;
            Power = power;
            Ability = ability;
        }

        public Card Clone()
        {
            return new Card(Id, Name, Cost, Power, Ability?.Clone());
        }
    }

    /// <summary>
    /// Card ability definition
    /// </summary>
    public class CardAbility
    {
        public string Type { get; } // "GainPoints", "StealPoints", "DoublePower", etc.
        public int Value { get; }

        public CardAbility(string type, int value)
        {
            Type = type ?? throw new ArgumentNullException(nameof(type));
            Value = value;
        }

        public CardAbility Clone()
        {
            return new CardAbility(Type, Value);
        }
    }

    /// <summary>
    /// Represents a card that has been played/folded
    /// </summary>
    public class PlayedCard
    {
        public int CardId { get; }
        public int OrderIndex { get; }
        public bool IsRevealed { get; set; }

        public PlayedCard(int cardId, int orderIndex)
        {
            CardId = cardId;
            OrderIndex = orderIndex;
            IsRevealed = false;
        }
    }
}