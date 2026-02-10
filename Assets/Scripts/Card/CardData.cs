namespace CardDuel.Gameplay
{
    [System.Serializable]
    public class CardData
    {
        public int Id;
        public string Name;
        public int Cost;
        public int Power;
        public string Role;
        public string[] Tags;
        public CardAbility Ability;
    }
}