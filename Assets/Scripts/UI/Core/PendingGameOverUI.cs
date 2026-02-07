using UnityEngine;
using TMPro;
using CardDuel.Events;
using CardDuel.UI.Core;

namespace CardDuel.UI.Game
{
    public class PendingGameOverPanel : UIPanel
    {
        [SerializeField] private TextMeshProUGUI countdownText;

        public override void SubscribeToEvents()
        {
            EventBus.Subscribe<PendingGameOverEvent>(OnPendingGameOver);
            EventBus.Subscribe<MatchResumedEvent>(OnMatchResumed);
            EventBus.Subscribe<GameEndEvent>(OnGameEnd);
        }

        public override void UnsubscribeFromEvents()
        {
            EventBus.Unsubscribe<PendingGameOverEvent>(OnPendingGameOver);
            EventBus.Unsubscribe<MatchResumedEvent>(OnMatchResumed);
            EventBus.Unsubscribe<GameEndEvent>(OnGameEnd);
        }

        private void OnPendingGameOver(PendingGameOverEvent e)
        {
            if (countdownText == null)
                return;

            countdownText.text =
                $"Opponent disconnected\nGame ends in {Mathf.CeilToInt(e.remainingTime)}s";

            SwitchUI(UIState.PendingGameOver);
        }

        private void OnMatchResumed(MatchResumedEvent e)
        {
            SwitchUI(UIState.Gameplay);
        }

        private void OnGameEnd(GameEndEvent e)
        {
            SwitchUI(UIState.GameOver);
        }

        private static void SwitchUI(UIState state)
        {
            if (UIManager.Instance == null)
                return;

            UIManager.Instance.SwitchTo(state);
        }
    }
}
