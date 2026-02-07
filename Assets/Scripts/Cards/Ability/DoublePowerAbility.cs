namespace CardDuel.Cards.AbilitySystems
{
    public class DoublePowerAbility : ICardAbility
{
    public void Execute(AbilityContext context, int value)
    {
        context.card.currentPower *= 2;
    }
}

}