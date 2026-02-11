using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

namespace CardDuel.Gameplay
{
    /// <summary>
    /// Manages hidden state information for opponents
    /// Shows only card counts, not card identities
    /// </summary>
    public class HiddenStateSystem : NetworkBehaviour
    {
        private Dictionary<ulong, PlayerHiddenState> _playerStates;
        private ulong _localPlayerId;
        
        public Dictionary<ulong, PlayerHiddenState> PlayerStates => _playerStates;
        
        private void Awake()
        {
            _playerStates = new Dictionary<ulong, PlayerHiddenState>();
        }
        
        public void Initialize(ulong localPlayerId)
        {
            _localPlayerId = localPlayerId;
        }
        
        /// <summary>
        /// Update hidden state for a player
        /// </summary>
        public void UpdatePlayerState(ulong playerId, int handCount, int stagedCount, int foldedCount)
        {
            if (!_playerStates.ContainsKey(playerId))
            {
                _playerStates[playerId] = new PlayerHiddenState(playerId);
            }
            
            var state = _playerStates[playerId];
            state.HandCount = handCount;
            state.StagedCount = stagedCount;
            state.FoldedCount = foldedCount;
            state.LastUpdated = Time.time;
            
            // Only broadcast non-local player states to maintain hidden information
            if (playerId != _localPlayerId && IsServer)
            {
                BroadcastPlayerStateClientRpc(playerId, handCount, stagedCount, foldedCount);
            }
        }
        
        /// <summary>
        /// Get hidden state for a specific player
        /// </summary>
        public PlayerHiddenState GetPlayerState(ulong playerId)
        {
            if (_playerStates.TryGetValue(playerId, out var state))
            {
                return state;
            }
            
            // Create default state if not found
            var newState = new PlayerHiddenState(playerId);
            _playerStates[playerId] = newState;
            return newState;
        }
        
        /// <summary>
        /// Get visible information for opponent (hides card identities)
        /// </summary>
        public OpponentViewState GetOpponentViewState(ulong opponentId)
        {
            var playerState = GetPlayerState(opponentId);
            
            return new OpponentViewState
            {
                PlayerId = opponentId,
                HandCount = playerState.HandCount,
                StagedCount = playerState.StagedCount,
                FoldedCount = playerState.FoldedCount,
                IsTurnActive = playerState.IsTurnActive,
                TimeSinceLastUpdate = Time.time - playerState.LastUpdated
            };
        }
        
        /// <summary>
        /// Mark player as having active turn
        /// </summary>
        public void SetPlayerTurnActive(ulong playerId, bool isActive)
        {
            if (!_playerStates.ContainsKey(playerId))
            {
                _playerStates[playerId] = new PlayerHiddenState(playerId);
            }
            
            _playerStates[playerId].IsTurnActive = isActive;
            
            if (playerId != _localPlayerId && IsServer)
            {
                BroadcastTurnStatusClientRpc(playerId, isActive);
            }
        }
        
        /// <summary>
        /// Reset all player states for new turn
        /// </summary>
        public void ResetForNewTurn()
        {
            foreach (var kvp in _playerStates)
            {
                kvp.Value.StagedCount = 0;
                kvp.Value.IsTurnActive = false;
            }
        }
        
        /// <summary>
        /// Validate that hidden state matches actual game state
        /// </summary>
        public bool ValidateState(ulong playerId, int actualHandCount, int actualStagedCount, int actualFoldedCount)
        {
            var state = GetPlayerState(playerId);
            return state.HandCount == actualHandCount &&
                   state.StagedCount == actualStagedCount &&
                   state.FoldedCount == actualFoldedCount;
        }
        
        #region Network RPCs
        
        [ClientRpc]
        private void BroadcastPlayerStateClientRpc(ulong playerId, int handCount, int stagedCount, int foldedCount)
        {
            UpdatePlayerState(playerId, handCount, stagedCount, foldedCount);
        }
        
        [ClientRpc]
        private void BroadcastTurnStatusClientRpc(ulong playerId, bool isActive)
        {
            SetPlayerTurnActive(playerId, isActive);
        }
        
        #endregion
    }
    
    /// <summary>
    /// Represents hidden state information for a player
    /// </summary>
    public class PlayerHiddenState
    {
        public ulong PlayerId { get; }
        public int HandCount { get; set; }
        public int StagedCount { get; set; }
        public int FoldedCount { get; set; }
        public bool IsTurnActive { get; set; }
        public float LastUpdated { get; set; }
        
        public PlayerHiddenState(ulong playerId)
        {
            PlayerId = playerId;
            HandCount = 0;
            StagedCount = 0;
            FoldedCount = 0;
            IsTurnActive = false;
            LastUpdated = Time.time;
        }
    }
    
    /// <summary>
    /// View model for opponent information (hides card identities)
    /// </summary>
    public class OpponentViewState
    {
        public ulong PlayerId { get; set; }
        public int HandCount { get; set; }
        public int StagedCount { get; set; }
        public int FoldedCount { get; set; }
        public bool IsTurnActive { get; set; }
        public float TimeSinceLastUpdate { get; set; }
        
        /// <summary>
        /// Total cards committed to this turn (staged + folded)
        /// </summary>
        public int TotalCommittedCards => StagedCount + FoldedCount;
        
        /// <summary>
        /// Get status description for UI
        /// </summary>
        public string GetStatusDescription()
        {
            if (IsTurnActive)
            {
                if (StagedCount > 0)
                    return $"Playing {StagedCount} cards";
                else
                    return "Thinking...";
            }
            else
            {
                if (FoldedCount > 0)
                    return $"Played {FoldedCount} cards";
                else
                    return "Waiting...";
            }
        }
    }
    
    /// <summary>
    /// Event for hidden state updates
    /// </summary>
    public class HiddenStateUpdatedEvent
    {
        public ulong PlayerId;
        public OpponentViewState ViewState;
    }
}