using System;
using System.Collections.Generic;
using UnityEngine;

namespace CardDuel.Gameplay
{
    public class DeckService
    {
        private readonly Dictionary<ulong, List<int>> _playerDecks = new();
        private readonly Dictionary<ulong, int> _drawIndex = new();

        public void InitializeDeckForPlayer(ulong clientId)
        {
            var deck = new List<int>();

            // Create balanced 12-card deck following match structure
            var cardTemplates = new List<(int power, int energy, CardAbilityType ability, int value)>()
            {
                // Low energy cards (1-2 energy)
                (2, 1, CardAbilityType.GainPoints, 1),
                (3, 1, CardAbilityType.None, 0),
                (1, 1, CardAbilityType.Disruption, 1),
                (4, 2, CardAbilityType.None, 0),
                (3, 2, CardAbilityType.DoublePower, 2),
                (2, 2, CardAbilityType.StealPoints, 1),
                
                // Medium energy cards (3 energy)
                (5, 3, CardAbilityType.None, 0),
                (4, 3, CardAbilityType.GainPoints, 2),
                (6, 3, CardAbilityType.DoublePower, 2),
                
                // High energy cards (4-5 energy)
                (7, 4, CardAbilityType.StealPoints, 2),
                (8, 4, CardAbilityType.None, 0),
                (6, 5, CardAbilityType.Disruption, 2)
            };

            // Create cards with IDs 1-12
            for (int i = 0; i < MatchStructure.DECK_SIZE; i++)
            {
                deck.Add(i + 1);
            }

            Shuffle(deck);

            _playerDecks[clientId] = deck;
            _drawIndex[clientId] = 0;
        }

        public int Draw(ulong clientId)
        {
            if (!_playerDecks.ContainsKey(clientId))
            {
                Debug.LogError($"Deck not found for client {clientId}");
                return -1;
            }

            var deck = _playerDecks[clientId];
            int index = _drawIndex[clientId];

            if (index >= deck.Count)
            {
                Shuffle(deck);
                _drawIndex[clientId] = 0;
                index = 0;
            }

            _drawIndex[clientId]++;
            return deck[index];
        }

        /// <summary>
        /// Draw cards for current turn based on match structure
        /// </summary>
        public List<int> DrawCardsForTurn(ulong clientId, int turnNumber)
        {
            int cardsToDraw = MatchStructure.GetCardsToDraw(turnNumber);
            var drawnCards = new List<int>();

            for (int i = 0; i < cardsToDraw; i++)
            {
                int cardId = Draw(clientId);
                if (cardId != -1)
                {
                    drawnCards.Add(cardId);
                }
            }

            return drawnCards;
        }

        /// <summary>
        /// Create initial 3-card hand
        /// </summary>
        public List<int> CreateInitialHand(ulong clientId)
        {
            return DrawCardsForTurn(clientId, 1); // Turn 1 = 3 cards
        }

        /// <summary>
        /// Get remaining cards in deck
        /// </summary>
        public int GetRemainingCards(ulong clientId)
        {
            if (!_playerDecks.ContainsKey(clientId))
                return 0;

            var deck = _playerDecks[clientId];
            int drawn = _drawIndex[clientId];
            return Math.Max(0, deck.Count - drawn);
        }

        /// <summary>
        /// Check if player can draw requested number of cards
        /// </summary>
        public bool CanDrawCards(ulong clientId, int count)
        {
            return GetRemainingCards(clientId) >= count;
        }

        private void Shuffle(List<int> deck)
        {
            for (int i = 0; i < deck.Count; i++)
            {
                int j = UnityEngine.Random.Range(i, deck.Count);
                (deck[i], deck[j]) = (deck[j], deck[i]);
            }
        }
    }
}
