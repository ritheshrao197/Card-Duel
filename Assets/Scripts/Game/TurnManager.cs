using System.Collections.Generic;
using System.Collections;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using CardDuel.Domain.Models;
using CardDuel.Domain.GameState;
using CardDuel.Core.Events;

namespace CardDuel.Gameplay.Managers
{
    public class TurnManager : NetworkBehaviour
{
    [Header("Turn Settings")]
    public float TurnDuration = 30f;
    
    private float turnTimer;
    private bool turnTimerActive = false;
    private GameState gameState;
    private Dictionary<ulong, List<int>> revealedCardsOrder = new Dictionary<ulong, List<int>>();
    private int currentRevealIndex = 0;
    private ulong initiativePlayerId;
    
    void Start()
    {
        gameState = FindObjectOfType<GameState>();
        if (gameState == null)
        {
            gameState = gameObject.AddComponent<GameState>();
        }
        Initialize(gameState);
    }
    
    public void Initialize(GameState state)
    {
        gameState = state;
        ResetTurnTimer();
    }
    
    public void StartTurn(int turnNumber)
    {
        gameState.CurrentTurn = turnNumber;
        gameState.CurrentPhase = GamePhase.TurnInProgress;
        
        // Reset player states for this turn
        foreach (var kvp in gameState.PlayerStates)
        {
            kvp.Value.HasEndedTurn = false;
            kvp.Value.AvailableCost = turnNumber; // Cost increases each turn
        }
        
        ResetTurnTimer();
        turnTimerActive = true;
        
        // Draw card for each player
        DrawCardForAllPlayers();
        
        GameEvents.TriggerTurnStart(turnNumber);
    }
    
    public void EndCurrentTurn(ulong playerId)
    {
        if (gameState.PlayerStates.ContainsKey(playerId))
        {
            gameState.PlayerStates[playerId].HasEndedTurn = true;
            
            // Check if all players have ended their turn
            bool allPlayersEndedTurn = true;
            foreach (var playerState in gameState.PlayerStates.Values)
            {
                if (!playerState.HasEndedTurn)
                {
                    allPlayersEndedTurn = false;
                    break;
                }
            }
            
            if (allPlayersEndedTurn)
            {
                StartRevealPhase();
            }
        }
    }
    
    private void StartRevealPhase()
    {
        gameState.CurrentPhase = GamePhase.RevealingCards;
        
        // Determine initiative based on scores
        DetermineInitiative();
        
        // Begin alternating reveal sequence
        StartCoroutine(RevealSequence());
    }
    
    private void DetermineInitiative()
    {
        // Compare scores - higher score goes first
        // In case of tie, randomly choose
        var players = gameState.PlayerStates.Values.ToList();
        if (players.Count >= 2)
        {
            if (players[0].Score > players[1].Score)
            {
                initiativePlayerId = players[0].PlayerId;
            }
            else if (players[1].Score > players[0].Score)
            {
                initiativePlayerId = players[1].PlayerId;
            }
            else
            {
                // Tie - randomly choose
                initiativePlayerId = Random.Range(0, 2) == 0 ? players[0].PlayerId : players[1].PlayerId;
            }
        }
    }
    
    public System.Collections.IEnumerator RevealSequence()
    {
        // Prepare the reveal order - alternating between initiative player and opponent
        var players = gameState.PlayerStates.Keys.ToList();
        var opponentPlayerId = players[0] == initiativePlayerId ? players[1] : players[0];
        
        // Get all folded cards for each player and sort by order index
        var initiativeCards = gameState.FoldedCards[initiativePlayerId].OrderBy(c => c.OrderIndex).ToList();
        var opponentCards = gameState.FoldedCards[opponentPlayerId].OrderBy(c => c.OrderIndex).ToList();
        
        int maxCards = Mathf.Max(initiativeCards.Count, opponentCards.Count);
        currentRevealIndex = 0;
        
        while (currentRevealIndex < maxCards)
        {
            // Reveal initiative player's card if available
            if (currentRevealIndex < initiativeCards.Count)
            {
                RevealSingleCard(initiativePlayerId, initiativeCards[currentRevealIndex].CardId, currentRevealIndex);
            }
            
            // Reveal opponent's card if available
            if (currentRevealIndex < opponentCards.Count)
            {
                RevealSingleCard(opponentPlayerId, opponentCards[currentRevealIndex].CardId, currentRevealIndex);
            }
            
            currentRevealIndex++;
            
            // Wait for a brief moment between reveals
            yield return new WaitForSeconds(1.0f);
        }
        
        // All cards revealed, end the turn
        EndRevealPhase();
    }
    
    private void RevealSingleCard(ulong playerId, int cardId, int orderIndex)
    {
        var card = FindCardById(cardId);
        if (card != null)
        {
            var playerState = gameState.PlayerStates[playerId];
            var opponentPlayerId = GetOpponentId(playerId);
            var opponentState = gameState.PlayerStates[opponentPlayerId];
            
            // Process the card ability
            CardAbilitySystem.ProcessCardAbility(card, playerState, opponentState, gameState);
            
            // Mark the card as revealed
            var foldedCard = gameState.FoldedCards[playerId].FirstOrDefault(c => c.CardId == cardId);
            if (foldedCard != null)
            {
                foldedCard.IsRevealed = true;
            }
            
            GameEvents.TriggerRevealCard(playerId, cardId, orderIndex);
        }
    }
    
    private Card FindCardById(int cardId)
    {
        // This would typically fetch from a card database
        // For now, returning a placeholder card
        return new Card { Id = cardId, Name = "Placeholder", Cost = 1, Power = 1 };
    }
    
    private ulong GetOpponentId(ulong playerId)
    {
        foreach (var id in gameState.PlayerStates.Keys)
        {
            if (id != playerId) return id;
        }
        return 0; // Invalid ID
    }
    
    private void EndRevealPhase()
    {
        gameState.CurrentPhase = GamePhase.TurnComplete;
        
        // Clear folded cards for this turn
        foreach (var playerId in gameState.PlayerStates.Keys)
        {
            gameState.FoldedCards[playerId].Clear();
            gameState.SelectedCardIds[playerId].Clear(); // Clear selections for next turn
        }
        
        GameEvents.TriggerTurnEnd();
        
        // Check if game is over
        if (gameState.CurrentTurn >= gameState.MaxTurns)
        {
            EndGame();
        }
        else
        {
            // Start next turn after a delay
            Invoke(nameof(StartNextTurn), 2.0f);
        }
    }
    
    private void StartNextTurn()
    {
        StartTurn(gameState.CurrentTurn + 1);
    }
    
    private void EndGame()
    {
        gameState.CurrentPhase = GamePhase.GameOver;
        
        // Determine winner based on score
        ulong winningPlayerId = 0;
        int highestScore = -1;
        
        foreach (var kvp in gameState.PlayerStates)
        {
            if (kvp.Value.Score > highestScore)
            {
                highestScore = kvp.Value.Score;
                winningPlayerId = kvp.Key;
            }
        }
        
        GameEvents.TriggerGameEnd(gameState.CurrentTurn, winningPlayerId);
    }
    
    private void DrawCardForAllPlayers()
    {
        // Deal one card to each player's hand
        foreach (var kvp in gameState.PlayerStates)
        {
            // In a real implementation, this would draw from a deck
            // For now, adding a placeholder card
            kvp.Value.Hand.Add(new Card { Id = Random.Range(1, 100), Name = "Drawn Card", Cost = 1, Power = 1 });
        }
    }
    
    void Update()
    {
        if (turnTimerActive && IsServer)
        {
            turnTimer -= Time.deltaTime;
            if (turnTimer <= 0)
            {
                AutoEndTurnForInactivePlayers();
            }
        }
    }
    
    private void ResetTurnTimer()
    {
        turnTimer = TurnDuration;
    }
    
    private void AutoEndTurnForInactivePlayers()
    {
        foreach (var kvp in gameState.PlayerStates)
        {
            if (!kvp.Value.HasEndedTurn)
            {
                EndCurrentTurn(kvp.Key);
            }
        }
    }
}
}