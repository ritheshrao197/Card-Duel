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

            foreach (var card in CardDatabaseProvider.Database.AllCards)
            {
                deck.Add(card.Id);
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

        private void Shuffle(List<int> deck)
        {
            for (int i = 0; i < deck.Count; i++)
            {
                int j = Random.Range(i, deck.Count);
                (deck[i], deck[j]) = (deck[j], deck[i]);
            }
        }
    }
}
