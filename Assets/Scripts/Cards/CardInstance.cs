namespace CardDuel.Cards 
{
    public class CardInstance
{
    public CardDefinition definition;
    public int currentPower;
    public int orderIndex;

    public CardInstance(CardDefinition definition, int orderIndex)
    {
        this.definition = definition;
        this.currentPower = definition.power;
        this.orderIndex = orderIndex;
    }
}

}