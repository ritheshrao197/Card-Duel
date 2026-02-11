using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using CardDuel.UI.Events;
using CardDuel.Gameplay;

namespace CardDuel.Gameplay
{
    /// <summary>
    /// Manages the reveal sequence with initiative determination and alternating reveals
    /// </summary>
    public class RevealSequenceSystem : NetworkBehaviour
    {
        private Dictionary<ulong, List<CardData>> _playerRevealQueues;
        private Dictionary<ulong, int> _playerScores;
        private ulong _initiativePlayer;
        private bool _isRevealActive;
        private int _currentRevealIndex;
        
        public bool IsRevealActive => _isRevealActive;
        public ulong InitiativePlayer => _initiativePlayer;
        public Dictionary<ulong, int> PlayerScores => _playerScores;
        
        public event Action OnRevealStarted;
        public event Action<RevealStep> OnCardRevealed;
        public event Action OnRevealCompleted;
        
        private void Awake()
        {
            _playerRevealQueues = new Dictionary<ulong, List<CardData>>();
            _playerScores = new Dictionary<ulong, int>();
            _isRevealActive = false;
            _currentRevealIndex = 0;
        }
        
        /// <summary>
        /// Initialize reveal sequence with folded cards from both players
        /// </summary>
        public void InitializeReveal(Dictionary<ulong, List<CardData>> playerCards)
        {
            _playerRevealQueues.Clear();
            _currentRevealIndex = 0;
            
            // Copy player cards to reveal queues
            foreach (var kvp in playerCards)
            {
                _playerRevealQueues[kvp.Key] = new List<CardData>(kvp.Value);
                if (!_playerScores.ContainsKey(kvp.Key))
                {
                    _playerScores[kvp.Key] = 0;
                }
            }
            
            // Determine initiative based on current scores
            DetermineInitiative();
            
            _isRevealActive = true;
            
            OnRevealStarted?.Invoke();
            // TODO: Publish reveal sequence started event
        }
        
        /// <summary>
        /// Determine which player has initiative (higher score).
        /// On a tie, choose a random player as required by the spec.
        /// </summary>
        private void DetermineInitiative()
        {
            ulong playerA = GetFirstPlayerId();
            ulong playerB = GetSecondPlayerId();
            
            int scoreA = _playerScores.GetValueOrDefault(playerA, 0);
            int scoreB = _playerScores.GetValueOrDefault(playerB, 0);
            
            // Higher score gets initiative, tie → random player
            if (scoreA > scoreB)
            {
                _initiativePlayer = playerA;
            }
            else if (scoreB > scoreA)
            {
                _initiativePlayer = playerB;
            }
            else
            {
                _initiativePlayer = UnityEngine.Random.value < 0.5f ? playerA : playerB;
            }
        }
        
        /// <summary>
        /// Start the reveal sequence
        /// </summary>
        public void StartRevealSequence()
        {
            if (!_isRevealActive) return;
            
            StartCoroutine(ExecuteRevealSequence());
        }
        
        /// <summary>
        /// Execute the alternating reveal sequence
        /// </summary>
        private IEnumerator ExecuteRevealSequence()
        {
            ulong currentPlayer = _initiativePlayer;
            int maxReveals = GetMaxRevealCount();
            
            while (_currentRevealIndex < maxReveals)
            {
                // Reveal card from current player
                if (HasCardToReveal(currentPlayer, _currentRevealIndex))
                {
                    var revealedCard = RevealCard(currentPlayer, _currentRevealIndex);
                    if (revealedCard != null)
                    {
                        yield return ProcessRevealedCard(currentPlayer, revealedCard);
                    }
                }
                
                // Switch to other player
                currentPlayer = GetOtherPlayer(currentPlayer);
                
                // Reveal card from other player
                if (HasCardToReveal(currentPlayer, _currentRevealIndex))
                {
                    var revealedCard = RevealCard(currentPlayer, _currentRevealIndex);
                    if (revealedCard != null)
                    {
                        yield return ProcessRevealedCard(currentPlayer, revealedCard);
                    }
                }
                
                // Switch back and increment index
                currentPlayer = GetOtherPlayer(currentPlayer);
                _currentRevealIndex++;
                
                // Small delay between reveals for visual effect
                yield return new WaitForSeconds(0.8f);
            }
            
            // Reveal sequence completed
            CompleteRevealSequence();
        }
        
        /// <summary>
        /// Process a revealed card and update scores
        /// </summary>
        private IEnumerator ProcessRevealedCard(ulong playerId, CardData card)
        {
            // Create simple score tracking for ability execution
            int currentPlayerScore = _playerScores[playerId];
            int opponentScore = _playerScores[GetOtherPlayer(playerId)];
            
            // Execute card ability
            int baseScore = CardAbilityManager.ExecuteAbility(card.Ability, card.Power, currentPlayerScore, opponentScore);
            
            // Apply disruption effects if any
            if (CardAbilityManager.GetAbilityType(card.Ability?.Type) == CardAbilityType.Disruption)
            {
                int opponentCardCount = _playerRevealQueues[GetOtherPlayer(playerId)].Count;
                int remainingCards = CardAbilityManager.ApplyDisruption(card.Ability.Value, opponentCardCount);
                // Update opponent's reveal queue to reflect disruption
                while (_playerRevealQueues[GetOtherPlayer(playerId)].Count > remainingCards)
                {
                    _playerRevealQueues[GetOtherPlayer(playerId)].RemoveAt(_playerRevealQueues[GetOtherPlayer(playerId)].Count - 1);
                }
            }
            
            // Update scores
            _playerScores[playerId] = currentPlayerScore + baseScore;
            // Opponent score is handled by the ability execution
            
            // Create reveal step data
            var revealStep = new RevealStep
            {
                PlayerId = playerId,
                Card = card,
                BasePower = card.Power,
                FinalScore = baseScore,
                NewPlayerScore = _playerScores[playerId],
                NewOpponentScore = _playerScores[GetOtherPlayer(playerId)]
            };
            
            // Broadcast reveal step
            OnCardRevealed?.Invoke(revealStep);
            
            // Send network event
            BroadcastCardRevealedClientRpc(
                playerId, 
                card.Id, 
                baseScore, 
                _playerScores[playerId],
                _playerScores[GetOtherPlayer(playerId)]
            );
            
            // Wait for UI to process
            yield return new WaitForSeconds(0.3f);
        }
        
        /// <summary>
        /// Complete the reveal sequence and prepare for next turn
        /// </summary>
        private void CompleteRevealSequence()
        {
            _isRevealActive = false;
            _currentRevealIndex = 0;
            
            OnRevealCompleted?.Invoke();
            
            // TODO: Publish reveal sequence completed event
            
            // Notify that round is complete and next turn can start
            if (IsServer)
            {
                RoundCompletedServerRpc();
            }
        }
        
        #region Helper Methods
        
        private ulong GetFirstPlayerId()
        {
            foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
            {
                return clientId;
            }
            return NetworkManager.ServerClientId;
        }
        
        private ulong GetSecondPlayerId()
        {
            foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
            {
                if (clientId != GetFirstPlayerId())
                {
                    return clientId;
                }
            }
            return GetFirstPlayerId();
        }
        
        private ulong GetOtherPlayer(ulong currentPlayer)
        {
            return currentPlayer == GetFirstPlayerId() ? GetSecondPlayerId() : GetFirstPlayerId();
        }
        
        private bool HasCardToReveal(ulong playerId, int index)
        {
            return _playerRevealQueues.ContainsKey(playerId) && 
                   _playerRevealQueues[playerId].Count > index;
        }
        
        private CardData RevealCard(ulong playerId, int index)
        {
            if (!HasCardToReveal(playerId, index))
                return null;
                
            return _playerRevealQueues[playerId][index];
        }
        
        private int GetMaxRevealCount()
        {
            int maxCount = 0;
            foreach (var queue in _playerRevealQueues.Values)
            {
                maxCount = Math.Max(maxCount, queue.Count);
            }
            return maxCount;
        }
        
        private Dictionary<ulong, int> GetPlayerCardCounts()
        {
            var counts = new Dictionary<ulong, int>();
            foreach (var kvp in _playerRevealQueues)
            {
                counts[kvp.Key] = kvp.Value.Count;
            }
            return counts;
        }
        
        #endregion
        
        #region Network RPCs
        
        [ClientRpc]
        private void BroadcastCardRevealedClientRpc(ulong playerId, int cardId, int scoreChange, int newPlayerScore, int newOpponentScore)
        {
            // TODO: Publish card revealed event
        }
        
        [ServerRpc(RequireOwnership = false)]
        private void RoundCompletedServerRpc()
        {
            // Update game state for next round
            GameState.Instance.NextRound();
            
            // Notify all clients that next turn can start
            NextTurnReadyClientRpc();
        }
        
        [ClientRpc]
        private void NextTurnReadyClientRpc()
        {
            // TODO: Publish next turn ready event
        }
        
        #endregion
    }
    
    /// <summary>
    /// Data structure for a single reveal step
    /// </summary>
    public class RevealStep
    {
        public ulong PlayerId { get; set; }
        public CardData Card { get; set; }
        public int BasePower { get; set; }
        public int FinalScore { get; set; }
        public int NewPlayerScore { get; set; }
        public int NewOpponentScore { get; set; }
    }
    
    // Events for reveal system
    public class RevealSequenceStartedEvent
    {
        public ulong InitiativePlayer;
        public Dictionary<ulong, int> PlayerCardCounts;
    }
    
    public class CardRevealedEvent
    {
        public ulong PlayerId;
        public int CardId;
        public int ScoreChange;
        public int NewPlayerScore;
        public int NewOpponentScore;
    }
    
    public class RevealSequenceCompletedEvent
    {
        public Dictionary<ulong, int> PlayerScores;
    }
    
    public class NextTurnReadyEvent { }
}