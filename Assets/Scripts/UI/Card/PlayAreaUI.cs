using UnityEngine;
using CardDuel.UI.Events;
using System.Collections.Generic;
using CardDuel.UI.Card;

namespace CardDuel.UI.Gameplay
{
    public class PlayAreaUI : MonoBehaviour
    {
        private readonly Dictionary<int, CardUI> _cards = new();

        private void OnEnable()
        {
            UIEventBus.Subscribe<CardMovedEvent>(OnCardMoved);
        }

        private void OnDisable()
        {
            UIEventBus.Unsubscribe<CardMovedEvent>(OnCardMoved);
        }

        private void OnCardMoved(CardMovedEvent evt)
        {
            if (evt.ToDrawnArea)
            {
                var card = FindCard(evt.CardId);
                if (card == null) return;

                card.transform.SetParent(transform);
                card.SetInPlayArea(true);

                _cards[evt.CardId] = card;
            }
        }

        private CardUI FindCard(int cardId)
        {
            foreach (var card in FindObjectsOfType<CardUI>())
            {
                if (card.CardId == cardId)
                    return card;
            }
            return null;
        }
    }
}
