
using System.Collections.Generic;

namespace CardDuel.Gameplay
{
    public class PlayerHandState
    {
        public List<CardData> Hand { get; } = new();
        public List<CardData> Drawn { get; } = new();
        
        // Dictionary for O(1) lookup by card ID
        private Dictionary<int, CardData> handLookup = new();
        private Dictionary<int, CardData> drawnLookup = new();

        public int TotalDrawnCost()
        {
            int cost = 0;
            foreach (var c in Drawn)
                cost += c.Cost;
            return cost;
        }
        
        /// <summary>
        /// Adds a card to the hand and updates the lookup index
        /// </summary>
        public void AddToHand(CardData card)
        {
            Hand.Add(card);
            handLookup[card.Id] = card;
        }
        
        /// <summary>
        /// Removes a card from the hand and updates the lookup index
        /// </summary>
        public bool RemoveFromHand(CardData card)
        {
            if (Hand.Remove(card))
            {
                handLookup.Remove(card.Id);
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// Adds a card to the drawn pile and updates the lookup index
        /// </summary>
        public void AddToDrawn(CardData card)
        {
            Drawn.Add(card);
            drawnLookup[card.Id] = card;
        }
        
        /// <summary>
        /// Removes a card from the drawn pile and updates the lookup index
        /// </summary>
        public bool RemoveFromDrawn(CardData card)
        {
            if (Drawn.Remove(card))
            {
                drawnLookup.Remove(card.Id);
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// Gets a card from hand by ID in O(1) time
        /// </summary>
        public CardData GetCardFromHand(int cardId)
        {
            handLookup.TryGetValue(cardId, out CardData card);
            return card;
        }
        
        /// <summary>
        /// Gets a card from drawn pile by ID in O(1) time
        /// </summary>
        public CardData GetCardFromDrawn(int cardId)
        {
            drawnLookup.TryGetValue(cardId, out CardData card);
            return card;
        }
    }
}
