using CardDuel.Cards.AbilitySystems;
using CardDuel.Core;

namespace CardDuel.Cards 
{
    public static class CardResolver
{
    public static void ResolveCard(
        CardInstance card,
        PlayerState owner,
        PlayerState opponent)
    {
        // Apply base power
        owner.score += card.currentPower;

        // Resolve ability
        if (card.definition.ability != null &&
            !string.IsNullOrEmpty(card.definition.ability.type))
        {
            var ability = AbilitySystem.CreateAbility(card.definition.ability.type);
            var context = new AbilityContext(owner, opponent, card);
            ability.Execute(context, card.definition.ability.value);
        }
    }
}

}