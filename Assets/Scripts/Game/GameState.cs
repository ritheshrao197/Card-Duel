using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using CardDuel.Domain.Models;

namespace CardDuel.Domain.GameState
{
    public enum GamePhase
{
    WaitingForPlayers,
    TurnInProgress,
    RevealingCards,
    TurnComplete,
    GameOver
}

public enum PlayerRole
{
    Player1,
    Player2
}

public class GameState : NetworkBehaviour
{
    public GamePhase CurrentPhase { get; set; }
    public int CurrentTurn { get; set; }
    public int MaxTurns { get; private set; }
    public Dictionary<ulong, PlayerState> PlayerStates { get; private set; }
    public Dictionary<ulong, List<FoldedCard>> FoldedCards { get; private set; }
    public Dictionary<ulong, List<int>> SelectedCardIds { get; private set; }
    
    void Start()
    {
        Initialize(6); // Default to 6 turns
    }
    
    public void Initialize(int maxTurns)
    {
        MaxTurns = maxTurns;
        CurrentTurn = 1;
        CurrentPhase = GamePhase.WaitingForPlayers;
        PlayerStates = new Dictionary<ulong, PlayerState>();
        FoldedCards = new Dictionary<ulong, List<FoldedCard>>();
        SelectedCardIds = new Dictionary<ulong, List<int>>();
    }
    
    public void AddPlayer(ulong playerId)
    {
        if (!PlayerStates.ContainsKey(playerId))
        {
            PlayerStates[playerId] = new PlayerState(playerId);
            FoldedCards[playerId] = new List<FoldedCard>();
            SelectedCardIds[playerId] = new List<int>();
        }
    }
    
    public void StartGame()
    {
        CurrentPhase = GamePhase.TurnInProgress;
        
        // Notify clients game started
        var gameStartMsg = new GameStartMessage
        {
            Action = new FixedString32Bytes("gameStart"),
            PlayerIds = new FixedString64Bytes($"P1,P2"),
            TotalTurns = MaxTurns
        };
        
        if (IsServer)
        {
            // Send message to all clients
            SendMessageToAllClients(gameStartMsg);
        }
        
        GameEvents.TriggerGameStart();
    }
    
    private void SendMessageToAllClients(GameStartMessage message)
    {
        // Implementation for sending message to all clients
        // This would use Unity Netcode's messaging system
    }
}

[System.Serializable]
public class PlayerState
{
    public ulong PlayerId;
    public int Score;
    public int AvailableCost;
    public List<Card> Hand;
    public bool HasEndedTurn;
    
    public PlayerState(ulong playerId)
    {
        PlayerId = playerId;
        Score = 0;
        Hand = new List<Card>();
        HasEndedTurn = false;
        AvailableCost = 0;
    }
}

[System.Serializable]
public class FoldedCard
{
    public int CardId;
    public int OrderIndex;
    public bool IsRevealed;
    
    public FoldedCard(int cardId, int orderIndex)
    {
        CardId = cardId;
        OrderIndex = orderIndex;
        IsRevealed = false;
    }
}
}