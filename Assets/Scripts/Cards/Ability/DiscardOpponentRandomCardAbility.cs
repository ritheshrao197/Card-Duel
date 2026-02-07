using UnityEngine;

namespace CardDuel.Cards.AbilitySystems
{
    public class DiscardOpponentRandomCardAbility : ICardAbility
{
    public void Execute(AbilityContext context, int value)
    {
        for (int i = 0; i < value; i++)
        {
            if (context.opponent.hand.Count == 0)
                return;

            int index = Random.Range(0, context.opponent.hand.Count);
            context.opponent.hand.RemoveAt(index);
        }
    }
}

}