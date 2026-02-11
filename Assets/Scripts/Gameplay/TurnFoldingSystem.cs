using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using CardDuel.UI.Events;
using CardDuel.Networking;
using CardDuel.Gameplay;

namespace CardDuel.Gameplay
{
    /// <summary>
    /// Manages turn folding mechanism with 30-second timer and card locking
    /// Cards are folded when End Turn is pressed or timer expires
    /// Folded cards appear face-down on the board and cannot be changed
    /// </summary>
    public class TurnFoldingSystem : NetworkBehaviour
    {
        private TurnStagingSystem _stagingSystem;
        private float _turnTimer;
        private bool _isTurnActive;
        private List<CardData> _foldedCards;
        private ulong _playerId;
        private int _currentTurn;
        
        public float RemainingTime => Mathf.Max(0, MatchStructure.TURN_TIME_LIMIT - _turnTimer);
        public bool IsTurnActive => _isTurnActive;
        public List<CardData> FoldedCards => _foldedCards;
        public int CurrentTurn => _currentTurn;
        
        public event Action<float> OnTimerUpdated;
        public event Action OnTurnTimeout;
        public event Action<List<CardData>> OnTurnFolded;
        public event Action OnFoldCompleted;
        
        private void Awake()
        {
            _foldedCards = new List<CardData>();
            _turnTimer = 0f;
            _isTurnActive = false;
            _currentTurn = 0;
        }
        
        public void Initialize(TurnStagingSystem stagingSystem, ulong playerId)
        {
            _stagingSystem = stagingSystem;
            _playerId = playerId;
        }
        
        /// <summary>
        /// Start turn timer and activate folding system
        /// </summary>
        public void StartTurnTimer(int turnNumber)
        {
            _currentTurn = turnNumber;
            _turnTimer = 0f;
            _isTurnActive = true;
            _foldedCards.Clear();
            
            UIEventBus.Publish(new TurnTimerStartedEvent { Duration = MatchStructure.TURN_TIME_LIMIT });
        }
        
        /// <summary>
        /// Stop turn timer and deactivate system
        /// </summary>
        public void StopTurnTimer()
        {
            _isTurnActive = false;
            _turnTimer = 0f;
            
            UIEventBus.Publish(new TurnTimerStoppedEvent());
        }
        
        private void Update()
        {
            if (!_isTurnActive || !IsSpawned) return;
            
            _turnTimer += Time.deltaTime;
            
            // Update UI with remaining time
            OnTimerUpdated?.Invoke(RemainingTime);
            UIEventBus.Publish(new TurnTimerTickEvent { Remaining = RemainingTime });
            
            // Check for timeout
            if (_turnTimer >= MatchStructure.TURN_TIME_LIMIT)
            {
                TimeoutTurn();
            }
        }
        
        /// <summary>
        /// Player manually ends turn - fold staged cards
        /// </summary>
        public void EndTurn()
        {
            if (!_isTurnActive) return;
            
            Debug.Log($"Player {_playerId} ending turn {_currentTurn}");
            FoldCards();
            StopTurnTimer();
            
            // Send folded cards to server
            SubmitFoldedCardsServerRpc(GetFoldedCardIds());
        }
        
        /// <summary>
        /// Handle turn timeout - automatically fold cards
        /// </summary>
        private void TimeoutTurn()
        {
            if (!_isTurnActive) return;
            
            Debug.Log($"Turn {_currentTurn} timeout for player {_playerId}");
            OnTurnTimeout?.Invoke();
            UIEventBus.Publish(new TurnTimeoutEvent());
            
            FoldCards();
            StopTurnTimer();
            
            // Send folded cards to server
            SubmitFoldedCardsServerRpc(GetFoldedCardIds());
        }
        
        /// <summary>
        /// Fold current staged cards and lock them
        /// </summary>
        private void FoldCards()
        {
            if (_stagingSystem == null) return;
            
            // Get staged cards from staging system
            _foldedCards = _stagingSystem.FinalizeStaging();
            
            // Lock cards (no more changes allowed)
            foreach (var card in _foldedCards)
            {
                card.IsRevealed = false; // Cards remain hidden until reveal phase
            }
            
            OnTurnFolded?.Invoke(_foldedCards);
            OnFoldCompleted?.Invoke();
            
            UIEventBus.Publish(new CardsFoldedEvent 
            { 
                CardCount = _foldedCards.Count, 
                CardIds = GetFoldedCardIds().ToList() 
            });
            
            Debug.Log($"Folded {_foldedCards.Count} cards for player {_playerId} in turn {_currentTurn}");
        }
        
        /// <summary>
        /// Get IDs of folded cards for network transmission
        /// </summary>
        private int[] GetFoldedCardIds()
        {
            var cardIds = new List<int>();
            foreach (var card in _foldedCards)
            {
                cardIds.Add(card.Id);
            }
            return cardIds.ToArray();
        }
        
        /// <summary>
        /// Check if player has any cards staged
        /// </summary>
        public bool HasStagedCards()
        {
            return _stagingSystem?.StagingArea.Count > 0;
        }
        
        /// <summary>
        /// Get current turn time percentage (0-1)
        /// </summary>
        public float GetTimePercentage()
        {
            if (!_isTurnActive) return 0f;
            return Mathf.Clamp01(_turnTimer / MatchStructure.TURN_TIME_LIMIT);
        }
        
        /// <summary>
        /// Reset system for new turn
        /// </summary>
        public void ResetForNewTurn()
        {
            StopTurnTimer();
            _foldedCards.Clear();
            _currentTurn = 0;
        }
        
        #region Network RPCs
        
        [ServerRpc(RequireOwnership = false)]
        private void SubmitFoldedCardsServerRpc(int[] cardIds, ServerRpcParams rpcParams = default)
        {
            ulong clientId = rpcParams.Receive.SenderClientId;
            
            Debug.Log($"Server received folded cards from client {clientId}: {string.Join(",", cardIds)}");
            
            // Store folded cards for this player in game state
            var foldedCardsList = new List<CardData>();
            foreach (int cardId in cardIds)
            {
                var card = CardDatabaseProvider.Get(cardId);
                if (card != null)
                {
                    foldedCardsList.Add(card);
                }
            }
            
            GameState.Instance.PlayedCards[clientId] = foldedCardsList;
            GameState.Instance.TurnEndedPlayers.Add(clientId);
            
            // Check if both players have ended their turns
            if (GameState.Instance.TurnEndedPlayers.Count == 
                NetworkManager.Singleton.ConnectedClientsIds.Count)
            {
                Debug.Log("Both players have ended turns - starting reveal phase");
                StartRevealPhaseClientRpc();
            }
            else
            {
                // Notify opponent that player has ended turn
                NotifyPlayerEndedTurnClientRpc(clientId);
            }
        }
        
        [ClientRpc]
        private void NotifyPlayerEndedTurnClientRpc(ulong playerId)
        {
            UIEventBus.Publish(new PlayerEndedTurnEvent { PlayerId = playerId });
            UIEventBus.Publish(new WaitingForOpponentEvent { IsWaiting = true });
        }
        
        [ClientRpc]
        private void StartRevealPhaseClientRpc()
        {
            UIEventBus.Publish(new RevealPhaseStartedEvent());
            UIEventBus.Publish(new UIStateChangedEvent { State = UIState.Reveal });
        }
        
        #endregion
    }
    
    // Events for folding system
    public class TurnTimerStartedEvent : IUIEvent
    {
        public float Duration;
    }
    
    public class TurnTimerStoppedEvent : IUIEvent { }
    
    public class TurnTimeoutEvent : IUIEvent { }
    
    public class CardsFoldedEvent : IUIEvent
    {
        public int CardCount;
        public List<int> CardIds;
    }
    
    public class PlayerEndedTurnEvent : IUIEvent
    {
        public ulong PlayerId;
    }
    
    public class RevealPhaseStartedEvent : IUIEvent { }
    
    public class WaitingForOpponentEvent : IUIEvent
    {
        public bool IsWaiting;
    }
}