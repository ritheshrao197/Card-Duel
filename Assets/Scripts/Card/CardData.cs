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
        
        // For tracking in gameplay
        public int OrderIndex; // Position in reveal sequence
        public bool IsRevealed; // Whether this card has been revealed
        
        public CardData() { }
        
        public CardData(int id, string name, int cost, int power, CardAbility ability = null)
        {
            Id = id;
            Name = name;
            Cost = cost;
            Power = power;
            Ability = ability ?? new CardAbility();
            OrderIndex = -1;
            IsRevealed = false;
        }
    }
}