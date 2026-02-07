namespace CardDuel.Cards.AbilitySystems
{
    public class DestroyOpponentCardInPlayAbility : ICardAbility
{
    public void Execute(AbilityContext context, int value)
    {
        if (context.opponent.foldedCards.Count == 0)
            return;

        context.opponent.foldedCards.RemoveAt(0);
    }
}

}