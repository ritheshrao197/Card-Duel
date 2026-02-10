using System;
using System.Collections.Generic;

namespace CardDuel.Gameplay
{
    [Serializable]
    public class CardAbilityJson
    {
        public string type;
        public int value;
    }

    [Serializable]
    public class CardJson
    {
        public int id;
        public string cardName;
        public int cost;
        public int power;
        public CardAbilityJson ability;
    }

    // Wrapper is REQUIRED because JsonUtility cannot parse top-level arrays
    [Serializable]
    public class CardJsonList
    {
        public List<CardJson> cards;
    }
}