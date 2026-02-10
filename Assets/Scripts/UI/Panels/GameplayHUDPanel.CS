using UnityEngine;
using TMPro;
using Unity.Netcode;
using CardDuel.Core;
using CardDuel.Gameplay.Events;
using CardDuel.Networking;

namespace CardDuel.UI.Gameplay
{
    public class GameplayHUDPanel : MonoBehaviour
    {
        [Header("Turn Info")]
        [SerializeField] private TextMeshProUGUI turnText;

        [Header("Timer")]
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private TurnTimerNetwork turnTimer;

        [Header("Score")]
        [SerializeField] private TextMeshProUGUI scoreText;

        private ulong localPlayerId;

        private void Awake()
        {
            localPlayerId = NetworkManager.Singleton.LocalClientId;

            EventBus.Subscribe<TurnStartedEvent>(OnTurnStarted);
            EventBus.Subscribe<TurnEndedEvent>(OnTurnEnded);
            EventBus.Subscribe<ScoreUpdatedEvent>(OnScoreUpdated);
        }

        private void Update()
        {
            UpdateTimer();
        }

        // --------------------
        // Turn Events
        // --------------------

        private void OnTurnStarted(TurnStartedEvent evt)
        {
            turnText.text = evt.PlayerId == localPlayerId
                ? "YOUR TURN"
                : "OPPONENT'S TURN";
        }

        private void OnTurnEnded(TurnEndedEvent evt)
        {
            turnText.text = "WAITING...";
        }

        // --------------------
        // Timer
        // --------------------

        private void UpdateTimer()
        {
            if (turnTimer == null) return;

            var time = Mathf.Ceil(turnTimer.RemainingTime.Value);
            timerText.text = time.ToString();
        }

        // --------------------
        // Score
        // --------------------

        private void OnScoreUpdated(ScoreUpdatedEvent evt)
        {
            scoreText.text = $"{evt.LocalScore} - {evt.OpponentScore}";
        }
    }
}
