using System;
using UnityEngine;

namespace CardDuel.Domain.Models
{
    [Serializable]
    public class Card
{
    public int Id;
    public string Name;
    public int Cost;
    public int Power;
    public CardAbility Ability;
    
    public Card Clone()
    {
        return new Card
        {
            Id = this.Id,
            Name = this.Name,
            Cost = this.Cost,
            Power = this.Power,
            Ability = this.Ability != null ? new CardAbility(this.Ability.Type, this.Ability.Value) : null
        };
    }
}

[Serializable]
public class CardAbility
{
    public string Type; // "GainPoints", "StealPoints", "DoublePower", etc.
    public int Value;
    
    public CardAbility(string type, int value)
    {
        Type = type;
        Value = value;
    }
}
}