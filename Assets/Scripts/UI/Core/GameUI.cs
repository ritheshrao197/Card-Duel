using UnityEngine;
using CardDuel.Events;
using TMPro;

namespace CardDuel.UI.Game
{
    public class GameUI : MonoBehaviour
    {
        public TextMeshProUGUI gameStatusText;

        private void OnEnable()
        {
            EventBus.Subscribe<TurnStartEvent>(OnTurnStart);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<TurnStartEvent>(OnTurnStart);
        }

        private void OnTurnStart(TurnStartEvent e)
        {
            gameStatusText.text = $"Turn {e.turnNumber} | Cost {e.availableCost}";
        }
    }
}
