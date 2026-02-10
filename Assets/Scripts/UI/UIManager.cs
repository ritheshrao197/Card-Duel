using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Unity.Netcode;
using CardDuel.Core;
using CardDuel.Domain.Events;
using CardDuel.Gameplay.Events;

namespace CardDuel.UI.Managers
{
    public class UIManager : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject HandPanel;
    public GameObject BoardPanel;
    public GameObject TurnIndicator;
    public TextMeshProUGUI ScoreText;
    public Button EndTurnButton;
    public TextMeshProUGUI TimerText;
    public GameObject CardPrefab;
    
    private List<GameObject> handDisplays = new List<GameObject>();
    private int localScore = 0;
    private int opponentScore = 0;
    
    void Start()
    {
        InitializeUI();
        SubscribeToEvents();
    }
    
    private void InitializeUI()
    {
        if (EndTurnButton != null)
        {
            EndTurnButton.onClick.AddListener(OnEndTurnClicked);
        }
    }
    
    private void SubscribeToEvents()
    {
        // Subscribe to existing event system
        EventBus.Subscribe<TurnStartedEvent>(OnTurnStarted);
        EventBus.Subscribe<TurnEndedEvent>(OnTurnEnded);
        EventBus.Subscribe<ScoreUpdatedEvent>(OnScoreUpdatedEvent);
        EventBus.Subscribe<CardPlayedEvent>(OnCardPlayed);
    }
    
    private void OnTurnStarted(TurnStartedEvent evt)
    {
        Debug.Log($"Turn started for player: {evt.PlayerId}");
        // Update UI for new turn
        UpdateTurnIndicator();
    }
    
    private void OnTurnEnded(TurnEndedEvent evt)
    {
        Debug.Log($"Turn ended for player: {evt.PlayerId}");
    }
    
    private void OnScoreUpdatedEvent(ScoreUpdatedEvent evt)
    {
        localScore = evt.LocalScore;
        opponentScore = evt.OpponentScore;
        
        if (ScoreText != null)
        {
            ScoreText.text = $"Score: {localScore}";
        }
    }
    
    private void OnCardPlayed(CardPlayedEvent evt)
    {
        Debug.Log($"Card {evt.CardId} played by player {evt.PlayerId}");
    }
    
    private void OnEndTurnClicked()
    {
        Debug.Log("End Turn Button Clicked!");
        // Send end turn command to server
        var command = new CardDuel.Gameplay.Events.EndTurnCommand
        {
            PlayerId = NetworkManager.Singleton.LocalClientId
        };
        
        // Send command through existing system
        // This would use the existing command system in the project
    }
    
    private void UpdateTurnIndicator()
    {
        if (TurnIndicator != null)
        {
            var tmpText = TurnIndicator.GetComponent<TextMeshProUGUI>();
            if (tmpText != null)
            {
                tmpText.text = "Turn In Progress";
            }
        }
    }
    
    void OnDestroy()
    {
        // EventBus doesn't support unsubscribe in this implementation
        // So we just let the handlers remain
    }
}
}