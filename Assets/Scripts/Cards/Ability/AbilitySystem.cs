using System;
namespace CardDuel.Cards.AbilitySystems
{
    public static class AbilitySystem
{
    public static ICardAbility CreateAbility(string type)
    {
        return type switch
        {
            "GainPoints" => new GainPointsAbility(),
            "StealPoints" => new StealPointsAbility(),
            "DoublePower" => new DoublePowerAbility(),
            "DrawExtraCard" => new DrawExtraCardAbility(),
            "DiscardOpponentRandomCard" => new DiscardOpponentRandomCardAbility(),
            "DestroyOpponentCardInPlay" => new DestroyOpponentCardInPlayAbility(),
            _ => throw new Exception($"Unknown ability type: {type}")
        };
    }
}

}