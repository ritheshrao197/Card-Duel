using System.Collections.Generic;
using CardDuel.Cards;

namespace CardDuel.Core
{
    public class PlayerState
    {
        public string playerId;  
        public ulong lastClientId;
        public bool isConnected; 
        public int score;

        public List<CardDefinition> deck = new();
        public List<CardDefinition> hand = new();
        public List<CardInstance> foldedCards = new();

        public void DrawCard()
        {
            if (deck.Count == 0) return;

            var card = deck[0];
            deck.RemoveAt(0);
            hand.Add(card);
        }
    }
}
