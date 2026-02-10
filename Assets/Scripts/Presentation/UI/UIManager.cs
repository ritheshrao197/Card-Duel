using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CardDuel.Core;
using CardDuel.Domain.Events;
using CardDuel.Domain.Commands;
using CardDuel.Infrastructure.Networking;

namespace CardDuel.Presentation.UI
{
    /// <summary>
    /// Reactive UI manager - listens to events, never validates
    /// Presentation is reactive only
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        [Header("UI References")]
        public GameObject HandPanel;
        public GameObject BoardPanel;
        public TextMeshProUGUI TurnIndicator;
        public TextMeshProUGUI ScoreText;
        public Button EndTurnButton;
        public TextMeshProUGUI TimerText;

        private int localScore = 0;
        private int opponentScore = 0;
        private int currentTurn = 1;
        private float turnTimer = 30f;

        void Start()
        {
            SubscribeToEvents();
            InitializeUI();
        }

        private void SubscribeToEvents()
        {
            // Listen to all relevant domain events
            EventBus.Subscribe<GameStartedEvent>(OnGameStarted);
            EventBus.Subscribe<TurnStartedEvent>(OnTurnStarted);
            EventBus.Subscribe<PlayerEndedTurnEvent>(OnPlayerEndedTurn);
            EventBus.Subscribe<CardPlayedEvent>(OnCardPlayed);
            EventBus.Subscribe<ScoreUpdatedEvent>(OnScoreUpdated);
            EventBus.Subscribe<TurnEndedEvent>(OnTurnEnded);
            EventBus.Subscribe<GameEndedEvent>(OnGameEnded);
        }

        private void InitializeUI()
        {
            if (EndTurnButton != null)
            {
                EndTurnButton.onClick.AddListener(OnEndTurnButtonClicked);
            }
        }

        // Event Handlers - Pure reactions, no validation
        private void OnGameStarted(GameStartedEvent evt)
        {
            Debug.Log($"Game started with {evt.TotalTurns} turns");
            UpdateTurnIndicator(1, evt.TotalTurns);
            
            if (EndTurnButton != null)
                EndTurnButton.interactable = true;
        }

        private void OnTurnStarted(TurnStartedEvent evt)
        {
            Debug.Log($"Turn {evt.TurnNumber} started");
            currentTurn = evt.TurnNumber;
            UpdateTurnIndicator(evt.TurnNumber, 6); // Assuming 6 total turns
            turnTimer = 30f;
            
            if (EndTurnButton != null)
                EndTurnButton.interactable = true;
        }

        private void OnPlayerEndedTurn(PlayerEndedTurnEvent evt)
        {
            Debug.Log($"Player {evt.PlayerId} ended their turn");
            
            // Visual feedback for local player
            if (evt.PlayerId == GetLocalPlayerId())
            {
                if (EndTurnButton != null)
                    EndTurnButton.interactable = false;
            }
        }

        private void OnCardPlayed(CardPlayedEvent evt)
        {
            Debug.Log($"Card {evt.CardId} played by player {evt.PlayerId}");
            
            // Animate card movement from hand to board
            AnimateCardPlay(evt.PlayerId, evt.CardId);
        }

        private void OnScoreUpdated(ScoreUpdatedEvent evt)
        {
            Debug.Log($"Player {evt.PlayerId} score updated to {evt.NewScore}");
            
            if (evt.PlayerId == GetLocalPlayerId())
            {
                localScore = evt.NewScore;
                UpdateScoreDisplay();
            }
            else
            {
                opponentScore = evt.NewScore;
                UpdateScoreDisplay();
            }
        }

        private void OnTurnEnded(TurnEndedEvent evt)
        {
            Debug.Log($"Turn {evt.TurnNumber} ended");
            turnTimer = 0f;
        }

        private void OnGameEnded(GameEndedEvent evt)
        {
            Debug.Log($"Game ended! Winner: Player {evt.WinningPlayerId} with score {evt.WinningScore}");
            turnTimer = 0f;
            
            if (EndTurnButton != null)
                EndTurnButton.interactable = false;
                
            ShowGameOverScreen(evt.WinningPlayerId == GetLocalPlayerId());
        }

        // UI Interaction - Emits Intent Events
        private void OnEndTurnButtonClicked()
        {
            Debug.Log("End Turn button clicked");
            
            // Emit intent - never change state directly
            var endTurnCommand = new EndTurnCommand(GetLocalPlayerId());
            
            // Send through network service (which ensures server authority)
            if (NetworkService.Instance != null)
            {
                NetworkService.Instance.SendCommandToServer(endTurnCommand);
            }
        }

        // Helper methods for UI updates
        private void UpdateTurnIndicator(int turn, int maxTurns)
        {
            if (TurnIndicator != null)
            {
                TurnIndicator.text = $"Turn {turn}/{maxTurns}";
            }
        }

        private void UpdateScoreDisplay()
        {
            if (ScoreText != null)
            {
                ScoreText.text = $"Score: {localScore} - {opponentScore}";
            }
        }

        private void AnimateCardPlay(ulong playerId, int cardId)
        {
            // In a real implementation, this would animate the card
            // moving from hand position to board position
            Debug.Log($"Animating card {cardId} play for player {playerId}");
        }

        private void ShowGameOverScreen(bool isWinner)
        {
            // Display win/lose screen
            Debug.Log(isWinner ? "You won!" : "You lost!");
        }

        private ulong GetLocalPlayerId()
        {
            // In a real implementation, this would get the actual local player ID
            return 1; // Placeholder
        }

        void Update()
        {
            // Update turn timer display
            if (turnTimer > 0)
            {
                turnTimer -= Time.deltaTime;
                if (TimerText != null)
                {
                    TimerText.text = Mathf.CeilToInt(turnTimer).ToString();
                }
            }
        }

        void OnDestroy()
        {
            // Clean up event subscriptions
            EventBus.Unsubscribe<GameStartedEvent>(OnGameStarted);
            EventBus.Unsubscribe<TurnStartedEvent>(OnTurnStarted);
            EventBus.Unsubscribe<PlayerEndedTurnEvent>(OnPlayerEndedTurn);
            EventBus.Unsubscribe<CardPlayedEvent>(OnCardPlayed);
            EventBus.Unsubscribe<ScoreUpdatedEvent>(OnScoreUpdated);
            EventBus.Unsubscribe<TurnEndedEvent>(OnTurnEnded);
            EventBus.Unsubscribe<GameEndedEvent>(OnGameEnded);
        }
    }
}