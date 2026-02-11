namespace CardDuel.Gameplay
{
    [System.Serializable]
    public class CardAbility
    {
        public string Type;   // e.g. "GainPoints", "StealPoints", "DoublePower"
        public int Value;     // e.g. 2
        
        public CardAbility() { }
        
        public CardAbility(string type, int value)
        {
            Type = type;
            Value = value;
        }
    }
}
