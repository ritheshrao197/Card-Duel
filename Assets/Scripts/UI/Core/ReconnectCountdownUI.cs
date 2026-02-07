using UnityEngine;
using UnityEngine.UI;
using CardDuel.Events;
using CardDuel.UI.Core;
using TMPro;

namespace CardDuel.UI.Game
{
    public class ReconnectCountdownUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI countdownText;

        public void OnEnable()
        {
            EventBus.Subscribe<PendingGameOverEvent>(OnPendingGameOver);
            EventBus.Subscribe<MatchResumedEvent>(OnMatchResumed);
            EventBus.Subscribe<GameEndEvent>(OnGameEnd);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<PendingGameOverEvent>(OnPendingGameOver);
            EventBus.Unsubscribe<MatchResumedEvent>(OnMatchResumed);
            EventBus.Unsubscribe<GameEndEvent>(OnGameEnd);
        }

        private void OnPendingGameOver(PendingGameOverEvent e)
        {
            countdownText.text =
                $"Opponent disconnected\nGame ends in {Mathf.CeilToInt(e.remainingTime)}s";

            UIManager.Instance.SwitchTo(UIState.PendingGameOver);
        }

        private void OnMatchResumed(MatchResumedEvent e)
        {
            UIManager.Instance.SwitchTo(UIState.Gameplay);
        }

        private void OnGameEnd(GameEndEvent e)
        {
            UIManager.Instance.SwitchTo(UIState.GameOver);
        }
    }
}
