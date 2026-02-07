namespace CardDuel.Cards.AbilitySystems
{
    public class StealPointsAbility : ICardAbility
{
    public void Execute(AbilityContext context, int value)
    {
        int stolen = System.Math.Min(value, context.opponent.score);
        context.opponent.score -= stolen;
        context.owner.score += stolen;
    }
}

}