namespace CardDuel.Cards.AbilitySystems
{
    public class DrawExtraCardAbility : ICardAbility
{
    public void Execute(AbilityContext context, int value)
    {
        for (int i = 0; i < value; i++)
        {
            context.owner.DrawCard();
        }
    }
}

}