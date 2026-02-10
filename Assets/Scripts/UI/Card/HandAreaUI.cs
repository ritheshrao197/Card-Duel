using UnityEngine;
using CardDuel.Gameplay;
using CardDuel.UI.Events;
using CardDuel.Utils;

namespace CardDuel.UI.Gameplay
{
    public class HandAreaUI : MonoBehaviour
    {
        [SerializeField] private Transform handRoot;
        [SerializeField] private CardUI cardPrefab;

        private void OnEnable()
        {
            UIEventBus.Subscribe<CardAddedToHandEvent>(OnCardAdded);
            UIEventBus.Subscribe<HandStateChangedEvent>(_ => Rebuild());
        }

        private void OnDisable()
        {
            UIEventBus.Unsubscribe<CardAddedToHandEvent>(OnCardAdded);
            UIEventBus.Unsubscribe<TurnStartedEvent>(_ => Rebuild());
        }

        private void Rebuild()
        {
            foreach (Transform child in handRoot)
                Destroy(child.gameObject);

            var hand = GameState.Instance.LocalHand.Hand;

            Log.UI($"Rebuilding hand UI | Cards={hand.Count}");

            foreach (var card in hand)
                Spawn(card);
        }

        private void OnCardAdded(CardAddedToHandEvent evt)
        {
            var card = CardDatabaseProvider.Get(evt.CardId);
            if (card == null)
                return;

            Spawn(card);
        }

        private void Spawn(CardData card)
        {
            var view = Instantiate(cardPrefab, handRoot);
            view.Bind(card);

            Log.UI($"Card instantiated | {card.Name}");
        }
    }
}
