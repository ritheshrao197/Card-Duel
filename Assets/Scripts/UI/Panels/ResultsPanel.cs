using TMPro;
using CardDuel.UI.Core;
using CardDuel.UI.Events;
using UnityEngine;

namespace CardDuel.UI.Panels
{
    public class ResultsPanel : UIPanel
    {
        [SerializeField] private TextMeshProUGUI resultText;
        [SerializeField] private TextMeshProUGUI scoreText;

        protected override void RegisterEvents()
        {
            UIEventBus.Subscribe<GameEndedEvent>(OnGameEnded);
        }

        protected override void UnregisterEvents()
        {
            UIEventBus.Unsubscribe<GameEndedEvent>(OnGameEnded);
        }

        private void OnGameEnded(GameEndedEvent evt)
        {
            Show();
            resultText.text = evt.IsVictory ? "Victory" : "Defeat";
            scoreText.text = $"{evt.PlayerScore} - {evt.OpponentScore}";
        }
    }
}
