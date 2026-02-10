using UnityEngine;
using TMPro;
using CardDuel.Core;
using CardDuel.Gameplay.Events;

namespace CardDuel.UI.Gameplay.Cards
{
    public class DeckView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI deckText;

        private void Awake()
        {
            EventBus.Subscribe<DeckCountUpdatedEvent>(OnDeckUpdated);
        }

        private void OnDeckUpdated(DeckCountUpdatedEvent evt)
        {
            deckText.text = $"Deck: {evt.RemainingCards}";
        }
    }
}
