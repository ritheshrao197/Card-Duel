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
    /// </summary>
    public class TurnFoldingSystem : NetworkBehaviour
    {
        private TurnStagingSystem _stagingSystem;
        private float _turnTimer;
        private bool _isTurnActive;
        private List<CardData> _foldedCards;
        private ulong _playerId;
        
        public float RemainingTime => Mathf.Max(0, MatchStructure.TURN_TIME_LIMIT - _turnTimer);
        public bool IsTurnActive => _isTurnActive;
        public List<CardData> FoldedCards => _foldedCards;
        
        public event Action<float> OnTimerUpdated;
        public event Action OnTurnTimeout;
        public event Action<List<CardData>> OnTurnFolded;
        
        private void Awake()
        {
            _foldedCards = new List<CardData>();
            _turnTimer = 0f;
            _isTurnActive = false;
        }
        
        public void Initialize(TurnStagingSystem stagingSystem, ulong playerId)
        {
            _stagingSystem = stagingSystem;
            _playerId = playerId;
        }
        
        /// <summary>
        /// Start turn timer and activate folding system
        /// </summary>
        public void StartTurnTimer()
        {
            _turnTimer = 0f;
            _isTurnActive = true;
            _foldedCards.Clear();
            
            // TODO: Publish turn timer started event
        }
        
        /// <summary>
        /// Stop turn timer and deactivate system
        /// </summary>
        public void StopTurnTimer()
        {
            _isTurnActive = false;
            _turnTimer = 0f;
            
            // TODO: Publish turn timer stopped event
        }
        
        private void Update()
        {
            if (!_isTurnActive || !IsSpawned) return;
            
            _turnTimer += Time.deltaTime;
            
            // Update UI with remaining time
            OnTimerUpdated?.Invoke(RemainingTime);
            
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
            
            OnTurnTimeout?.Invoke();
            FoldCards();
            StopTurnTimer();
            
            // TODO: Publish turn timeout event
            
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
            // Card locking would be handled at the UI level
            
            OnTurnFolded?.Invoke(_foldedCards);
            
            // TODO: Publish cards folded event
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
        
        #region Network RPCs
        
        [ServerRpc(RequireOwnership = false)]
        private void SubmitFoldedCardsServerRpc(int[] cardIds, ServerRpcParams rpcParams = default)
        {
            ulong clientId = rpcParams.Receive.SenderClientId;
            
            // TODO: Store folded cards for this player
            
            // Add player to ended players list
            GameState.Instance.TurnEndedPlayers.Add(clientId);
            
            // Check if both players have ended their turns
            if (GameState.Instance.TurnEndedPlayers.Count == 
                NetworkManager.Singleton.ConnectedClientsIds.Count)
            {
                // Start reveal phase
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
            // TODO: Publish player ended turn event
        }
        
        [ClientRpc]
        private void StartRevealPhaseClientRpc()
        {
            // TODO: Publish reveal phase started event
        }
        
        #endregion
    }
    
    // Events for folding system
    public class TurnTimerStartedEvent
    {
        public float Duration;
    }
    
    public class TurnTimerStoppedEvent { }
    
    public class TurnTimeoutEvent { }
    
    public class CardsFoldedEvent
    {
        public int CardCount;
        public List<int> CardIds;
    }
    
    public class PlayerEndedTurnEvent
    {
        public ulong PlayerId;
    }
    
    public class RevealPhaseStartedEvent { }
}