using UnityEngine;
using System.Collections.Generic;

namespace CardDuel.UI.Gameplay.Cards
{
    public class HandView : MonoBehaviour
    {
        [SerializeField] private CardView cardPrefab;
        [SerializeField] private Transform container;
        [SerializeField] private GameplayInputHandler inputHandler;

        private readonly List<CardView> cards = new();

        public void RebuildHand(int[] cardIds)
        {
            Clear();

            foreach (var id in cardIds)
            {
                var card = Instantiate(cardPrefab, container);
                card.Init(id, inputHandler);
                cards.Add(card);
            }
        }

        private void Clear()
        {
            foreach (var card in cards)
                Destroy(card.gameObject);

            cards.Clear();
        }
    }
}
