using CardDuel.Core;

namespace CardDuel.Cards 
{
    public class AbilityContext
{
    public PlayerState owner;
    public PlayerState opponent;
    public CardInstance card;

    public AbilityContext(PlayerState owner, PlayerState opponent, CardInstance card)
    {
        this.owner = owner;
        this.opponent = opponent;
        this.card = card;
    }
}
}