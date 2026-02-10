using UnityEngine;
using CardDuel.Core;
using CardDuel.Gameplay.Events;

namespace CardDuel.UI.Gameplay.Cards
{
    public class RevealAreaView : MonoBehaviour
    {
        [SerializeField] private Transform container;
        [SerializeField] private CardView cardPrefab;
        [SerializeField] private GameplayInputHandler dummyHandler;

        private void Awake()
        {
            EventBus.Subscribe<CardPlayedEvent>(OnCardPlayed);
        }

        private void OnCardPlayed(CardPlayedEvent evt)
        {
            var card = Instantiate(cardPrefab, container);
            card.Init(evt.CardId, dummyHandler);
        }
    }
}
