namespace CardDuel.Cards 
{
    public interface ICardAbility
{
    void Execute(AbilityContext context, int value);
}

}