using System.Collections.Generic;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using CardDuel.Core;
using CardDuel.Domain.Events;
using CardDuel.Gameplay.Events;
using CardDuel.Domain.Models;
using CardDuel.Domain.Data;
using CardDuel.Domain.GameState;
using CardDuel.Gameplay.Managers;

namespace CardDuel.Gameplay.Controllers
{
    public class GameController : NetworkBehaviour
{
    [Header("Game Settings")]
    public int TotalTurns = 6;
    public int StartingHandSize = 3;
    public int MaxHandSize = 7;
    
    [Header("References")]
    public CardDatabase CardDatabase;
    public UIManager UIManager;
    public TurnManager TurnManager;
    public GameState GameState;
    
    private List<Card> deck = new List<Card>();
    private Dictionary<ulong, List<Card>> playerHands = new Dictionary<ulong, List<Card>>();
    private Dictionary<ulong, List<Card>> foldedCards = new Dictionary<ulong, List<Card>>();
    
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        if (IsServer)
        {
            InitializeGame();
        }
    }
    
    private void InitializeGame()
    {
        // Load card database
        if (CardDatabase == null)
        {
            CardDatabase = ScriptableObject.CreateInstance<CardDatabase>();
            CardDatabase.LoadFromJson("Cards/CardDefinitions");
        }
        
        // Initialize game state
        if (GameState == null)
        {
            GameState = gameObject.AddComponent<GameState>();
        }
        GameState.Initialize(TotalTurns);
        
        // Initialize turn manager
        if (TurnManager == null)
        {
            TurnManager = gameObject.AddComponent<TurnManager>();
            TurnManager.Initialize(GameState);
        }
        
        // Create deck
        CreateDeck();
        
        // Add connected players to game state
        foreach (var clientId in NetworkManager.Singleton.ConnectedClients.Keys)
        {
            GameState.AddPlayer(clientId);
            playerHands[clientId] = new List<Card>();
        }
        
        // Deal starting hands
        DealStartingHands();
        
        // Start the game
        GameState.StartGame();
        
        // Start first turn
        TurnManager.StartTurn(1);
    }
    
    private void CreateDeck()
    {
        // For now, create a simple deck with multiples of each card
        // In a real implementation, you'd load from the database
        for (int i = 1; i <= 12; i++)
        {
            var card = new Card
            {
                Id = i,
                Name = $"Card {i}",
                Cost = (i % 4) + 1, // Cycle costs 1-4
                Power = (i % 5) + 1, // Cycle power 1-5
                Ability = i % 3 == 0 ? new CardAbility("GainPoints", 2) : null
            };
            deck.Add(card);
        }
        
        // Shuffle the deck
        ShuffleDeck();
    }
    
    private void ShuffleDeck()
    {
        for (int i = 0; i < deck.Count; i++)
        {
            var temp = deck[i];
            int randomIndex = Random.Range(i, deck.Count);
            deck[i] = deck[randomIndex];
            deck[randomIndex] = temp;
        }
    }
    
    private void DealStartingHands()
    {
        foreach (var clientId in NetworkManager.Singleton.ConnectedClients.Keys)
        {
            for (int i = 0; i < StartingHandSize; i++)
            {
                if (deck.Count > 0)
                {
                    var card = deck[deck.Count - 1];
                    deck.RemoveAt(deck.Count - 1);
                    playerHands[clientId].Add(card);
                }
            }
        }
    }
    
    [ServerRpc]
    public void PlayCardServerRpc(ulong playerId, int cardId, ServerRpcParams rpcParams = default)
    {
        // Validate the move
        if (!playerHands.ContainsKey(playerId) || GameState.CurrentPhase != GamePhase.TurnInProgress)
            return;
            
        var card = playerHands[playerId].Find(c => c.Id == cardId);
        if (card == null || card.Cost > GameState.PlayerStates[playerId].AvailableCost)
            return;
        
        // Deduct cost and move card to folded cards
        GameState.PlayerStates[playerId].AvailableCost -= card.Cost;
        playerHands[playerId].Remove(card);
        
        if (!foldedCards.ContainsKey(playerId))
            foldedCards[playerId] = new List<Card>();
        
        // Add card with order index (position in play sequence)
        int orderIndex = foldedCards[playerId].Count;
        foldedCards[playerId].Add(card);
        
        // Send event to all clients
        var cardPlayedEvent = new CardPlayedEvent
        {
            PlayerId = playerId,
            CardId = cardId
        };
        
        EventBus.Publish(cardPlayedEvent);
    }
    
    [ServerRpc]
    public void EndTurnServerRpc(ulong playerId, ServerRpcParams rpcParams = default)
    {
        if (GameState.PlayerStates.ContainsKey(playerId))
        {
            GameState.PlayerStates[playerId].HasEndedTurn = true;
            
            // Check if all players have ended their turn
            bool allPlayersEndedTurn = true;
            foreach (var playerState in GameState.PlayerStates.Values)
            {
                if (!playerState.HasEndedTurn)
                {
                    allPlayersEndedTurn = false;
                    break;
                }
            }
            
            if (allPlayersEndedTurn)
            {
                // Start reveal phase
                // We need to call the reveal sequence on the TurnManager component
                // Since it's a coroutine, we need to call it from the same MonoBehaviour
                StartCoroutine(TurnManager.RevealSequence());
            }
        }
    }
    
    public void DrawCardForPlayer(ulong playerId)
    {
        if (deck.Count > 0 && playerHands.ContainsKey(playerId))
        {
            var card = deck[deck.Count - 1];
            deck.RemoveAt(deck.Count - 1);
            playerHands[playerId].Add(card);
            
            // Update UI if this is the local player
            if (playerId == NetworkManager.Singleton.LocalClientId && UIManager != null)
            {
                UIManager.UpdateHand(playerHands[playerId]);
            }
        }
    }
}
}