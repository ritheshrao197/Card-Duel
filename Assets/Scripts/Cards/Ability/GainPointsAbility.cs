namespace CardDuel.Cards.AbilitySystems
{
    public class GainPointsAbility : ICardAbility
{
    public void Execute(AbilityContext context, int value)
    {
        context.owner.score += value;
    }
}

}