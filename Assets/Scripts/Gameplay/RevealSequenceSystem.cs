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
    /// Manages the reveal sequence with initiative determination and alternating reveals
    /// Initiative is determined once per turn at reveal start
    /// Cards are revealed in alternating sequence: P1-Card1, P2-Card1, P1-Card2, P2-Card2, etc.
    /// Score is updated immediately after every reveal
    /// </summary>
    public class RevealSequenceSystem : NetworkBehaviour
    {
        private Dictionary<ulong, List<CardData>> _playerRevealQueues;
        private Dictionary<ulong, int> _playerScores;
        private ulong _initiativePlayer;
        private bool _isRevealActive;
        private int _currentRevealIndex;
        private int _currentTurn;
        
        public bool IsRevealActive => _isRevealActive;
        public ulong InitiativePlayer => _initiativePlayer;
        public Dictionary<ulong, int> PlayerScores => _playerScores;
        public int CurrentTurn => _currentTurn;
        
        public event Action OnRevealStarted;
        public event Action<RevealStep> OnCardRevealed;
        public event Action OnRevealCompleted;
        
        private void Awake()
        {
            _playerRevealQueues = new Dictionary<ulong, List<CardData>>();
            _playerScores = new Dictionary<ulong, int>();
            _isRevealActive = false;
            _currentRevealIndex = 0;
            _currentTurn = 0;
        }
        
        /// <summary>
        /// Initialize reveal sequence with folded cards from both players
        /// </summary>
        public void InitializeReveal(Dictionary<ulong, List<CardData>> playerCards, int turnNumber)
        {
            _currentTurn = turnNumber;
            _playerRevealQueues.Clear();
            _currentRevealIndex = 0;
            
            Debug.Log($"Initializing reveal sequence for turn {turnNumber}");
            
            // Copy player cards to reveal queues
            foreach (var kvp in playerCards)
            {
                _playerRevealQueues[kvp.Key] = new List<CardData>(kvp.Value);
                if (!_playerScores.ContainsKey(kvp.Key))
                {
                    _playerScores[kvp.Key] = 0;
                }
                
                Debug.Log($"Player {kvp.Key} has {_playerRevealQueues[kvp.Key].Count} cards to reveal");
            }
            
            // Determine initiative based on current scores
            DetermineInitiative();
            
            _isRevealActive = true;
            
            OnRevealStarted?.Invoke();
            
            // Publish reveal sequence started event
            var playerCardCounts = new Dictionary<ulong, int>();
            foreach (var kvp in _playerRevealQueues)
            {
                playerCardCounts[kvp.Key] = kvp.Value.Count;
            }
            
            UIEventBus.Publish(new RevealSequenceStartedEvent
            {
                InitiativePlayer = _initiativePlayer,
                PlayerCardCounts = playerCardCounts
            });
        }
        
        /// <summary>
        /// Determine which player has initiative (higher score).
        /// On a tie, choose a random player as required by the spec.
        /// Initiative does not change during reveal.
        /// </summary>
        private void DetermineInitiative()
        {
            ulong playerA = GetFirstPlayerId();
            ulong playerB = GetSecondPlayerId();
            
            int scoreA = _playerScores.GetValueOrDefault(playerA, 0);
            int scoreB = _playerScores.GetValueOrDefault(playerB, 0);
            
            Debug.Log($"Initiative determination - Player {playerA}: {scoreA}, Player {playerB}: {scoreB}");
            
            // Higher score gets initiative, tie → random player
            if (scoreA > scoreB)
            {
                _initiativePlayer = playerA;
                Debug.Log($"Player {playerA} gets initiative (higher score)");
            }
            else if (scoreB > scoreA)
            {
                _initiativePlayer = playerB;
                Debug.Log($"Player {playerB} gets initiative (higher score)");
            }
            else
            {
                _initiativePlayer = UnityEngine.Random.value < 0.5f ? playerA : playerB;
                Debug.Log($"Tie - Player {_initiativePlayer} randomly selected for initiative");
            }
        }
        
        /// <summary>
        /// Start the reveal sequence
        /// </summary>
        public void StartRevealSequence()
        {
            if (!_isRevealActive) return;
            
            Debug.Log($"Starting reveal sequence for turn {_currentTurn}");
            StartCoroutine(ExecuteRevealSequence());
        }
        
        /// <summary>
        /// Execute the alternating reveal sequence
        /// Cards must be revealed in the order they were played
        /// In alternating sequence: Initiative player reveals card #1 → resolve → update score
        ///                          Opponent reveals card #1 → resolve → update score
        ///                          Initiative player reveals card #2 → resolve → update score
        ///                          Continue until all cards are revealed
        /// </summary>
        private IEnumerator ExecuteRevealSequence()
        {
            int maxReveals = GetMaxRevealCount();
            Debug.Log($"Executing reveal sequence with {maxReveals} rounds");
            
            for (int revealIndex = 0; revealIndex < maxReveals; revealIndex++)
            {
                _currentRevealIndex = revealIndex;
                Debug.Log($"Reveal round {_currentRevealIndex + 1}/{maxReveals}");
                
                // Initiative player reveals first
                if (HasCardToReveal(_initiativePlayer, _currentRevealIndex))
                {
                    var revealedCard = RevealCard(_initiativePlayer, _currentRevealIndex);
                    if (revealedCard != null)
                    {
                        yield return ProcessRevealedCard(_initiativePlayer, revealedCard, _currentRevealIndex);
                    }
                }
                
                // Small delay for visual effect
                yield return new WaitForSeconds(0.5f);
                
                // Opponent reveals next
                ulong opponentPlayer = GetOtherPlayer(_initiativePlayer);
                if (HasCardToReveal(opponentPlayer, _currentRevealIndex))
                {
                    var revealedCard = RevealCard(opponentPlayer, _currentRevealIndex);
                    if (revealedCard != null)
                    {
                        yield return ProcessRevealedCard(opponentPlayer, revealedCard, _currentRevealIndex);
                    }
                }
                
                // Wait before next reveal round
                if (revealIndex < maxReveals - 1)
                {
                    yield return new WaitForSeconds(1.0f);
                }
            }
            
            // Reveal sequence completed
            CompleteRevealSequence();
        }
        
        /// <summary>
        /// Process a revealed card and update scores
        /// Score updates after every reveal immediately
        /// Next reveal begins only after the update
        /// </summary>
        private IEnumerator ProcessRevealedCard(ulong playerId, CardData card, int orderIndex)
        {
            Debug.Log($"Revealing card {card.Name} (ID: {card.Id}) for player {playerId} at index {orderIndex}");
            
            // Mark card as revealed
            card.IsRevealed = true;
            card.OrderIndex = orderIndex;
            
            // Get current scores
            int currentPlayerScore = _playerScores[playerId];
            int opponentScore = _playerScores[GetOtherPlayer(playerId)];
            int opponentHandCount = GameState.Instance.LocalHand.Hand.Count; // Approximation
            int opponentPlayCount = _playerRevealQueues[GetOtherPlayer(playerId)].Count;
            
            // Execute card ability with full context
            var abilityResult = CardAbilityManager.ExecuteAbility(
                card.Ability, 
                card.Power, 
                currentPlayerScore, 
                opponentScore,
                opponentHandCount,
                opponentPlayCount
            );
            
            // Apply score changes
            _playerScores[playerId] = currentPlayerScore + abilityResult.ScoreChange;
            if (abilityResult.OpponentScoreChange != 0)
            {
                _playerScores[GetOtherPlayer(playerId)] += abilityResult.OpponentScoreChange;
            }
            
            // Handle disruptive effects
            if (CardAbilityManager.HasDisruptiveEffect(card.Ability))
            {
                ApplyDisruptionEffects(playerId, abilityResult);
            }
            
            // Create reveal step data
            var revealStep = new RevealStep
            {
                PlayerId = playerId,
                Card = card,
                BasePower = card.Power,
                FinalScore = abilityResult.ScoreChange,
                NewPlayerScore = _playerScores[playerId],
                NewOpponentScore = _playerScores[GetOtherPlayer(playerId)],
                OrderIndex = orderIndex
            };
            
            // Broadcast reveal step locally
            OnCardRevealed?.Invoke(revealStep);
            
            // Send network event
            BroadcastCardRevealedNetwork(playerId, card.Id, abilityResult, orderIndex);
            
            // Publish UI event
            UIEventBus.Publish(new CardRevealedEvent
            {
                PlayerId = playerId,
                CardId = card.Id,
                ScoreChange = abilityResult.ScoreChange,
                NewPlayerScore = _playerScores[playerId],
                NewOpponentScore = _playerScores[GetOtherPlayer(playerId)]
            });
            
            // Wait for UI to process
            yield return new WaitForSeconds(0.3f);
        }
        
        /// <summary>
        /// Apply disruption effects from card abilities
        /// </summary>
        private void ApplyDisruptionEffects(ulong playerId, AbilityExecutionResult abilityResult)
        {
            ulong opponentId = GetOtherPlayer(playerId);
            
            // Handle discarded cards
            if (abilityResult.DiscardedCardIds.Count > 0)
            {
                // TODO: Implement actual card discard logic
                Debug.Log($"Player {playerId} discarded {abilityResult.DiscardedCardIds.Count} opponent cards");
            }
            
            // Handle destroyed cards
            if (abilityResult.DestroyedCardIds.Count > 0)
            {
                // Remove cards from opponent's reveal queue
                var opponentQueue = _playerRevealQueues[opponentId];
                int cardsToDestroy = Math.Min(abilityResult.DestroyedCardIds.Count, opponentQueue.Count);
                
                for (int i = 0; i < cardsToDestroy; i++)
                {
                    if (opponentQueue.Count > 0)
                    {
                        opponentQueue.RemoveAt(opponentQueue.Count - 1);
                    }
                }
                
                Debug.Log($"Player {playerId} destroyed {cardsToDestroy} opponent cards in play");
            }
        }
        
        /// <summary>
        /// Broadcast card revealed to network
        /// </summary>
        private void BroadcastCardRevealedNetwork(ulong playerId, int cardId, AbilityExecutionResult result, int orderIndex)
        {
            if (IsServer)
            {
                // Send to all clients
                var message = new RevealSingleCardMessage
                {
                    playerId = GetPlayerId(playerId),
                    cardId = cardId,
                    orderIndex = orderIndex,
                    turnNumber = _currentTurn
                };
                
                SendNetworkMessage(message);
                
                // Send score update
                var scoreMessage = new ScoreUpdatedMessage
                {
                    playerId = GetPlayerId(playerId),
                    delta = result.ScoreChange,
                    newScore = _playerScores[playerId],
                    opponentScore = _playerScores[GetOtherPlayer(playerId)]
                };
                
                SendNetworkMessage(scoreMessage);
            }
        }
        
        /// <summary>
        /// Send network message to all clients
        /// </summary>
        private void SendNetworkMessage(NetworkMessage message)
        {
            string json = NetworkMessageSerializer.Serialize(message);
            if (json != null)
            {
                SendJsonToAllClientsClientRpc(json);
            }
        }
        
        /// <summary>
        /// Complete the reveal sequence and prepare for next turn
        /// </summary>
        private void CompleteRevealSequence()
        {
            _isRevealActive = false;
            _currentRevealIndex = 0;
            
            Debug.Log("Reveal sequence completed");
            
            OnRevealCompleted?.Invoke();
            
            // Publish reveal sequence completed event
            UIEventBus.Publish(new RevealSequenceCompletedEvent
            {
                PlayerScores = new Dictionary<ulong, int>(_playerScores)
            });
            
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
        
        private string GetPlayerId(ulong clientId)
        {
            return clientId == NetworkManager.ServerClientId ? "P1" : "P2";
        }
        
        private ulong GetClientId(string playerId)
        {
            return playerId == "P1" ? NetworkManager.ServerClientId : GetSecondPlayerId();
        }
        
        #endregion
        
        #region Network RPCs
        
        [ClientRpc]
        private void SendJsonToAllClientsClientRpc(string jsonMessage)
        {
            // Handle received message
            var baseMessage = NetworkMessageSerializer.Deserialize(jsonMessage);
            if (baseMessage != null)
            {
                // Route message based on action
                switch (baseMessage.action)
                {
                    case "revealSingleCard":
                        var revealMessage = NetworkMessageSerializer.Deserialize<RevealSingleCardMessage>(jsonMessage);
                        if (revealMessage != null)
                        {
                            // Handle card reveal
                            Debug.Log($"Received reveal for player {revealMessage.playerId} card {revealMessage.cardId}");
                        }
                        break;
                    
                    case "scoreUpdated":
                        var scoreMessage = NetworkMessageSerializer.Deserialize<ScoreUpdatedMessage>(jsonMessage);
                        if (scoreMessage != null)
                        {
                            // Handle score update
                            Debug.Log($"Score updated for player {scoreMessage.playerId}: +{scoreMessage.delta} = {scoreMessage.newScore}");
                        }
                        break;
                }
            }
        }
        
        [ServerRpc(RequireOwnership = false)]
        private void RoundCompletedServerRpc()
        {
            Debug.Log("Round completed - preparing next turn");
            
            // Update game state for next round
            GameState.Instance.NextRound();
            
            // Notify all clients that next turn can start
            NextTurnReadyClientRpc();
        }
        
        [ClientRpc]
        private void NextTurnReadyClientRpc()
        {
            UIEventBus.Publish(new NextTurnReadyEvent());
            Debug.Log("Next turn ready notification sent to clients");
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
        public int OrderIndex { get; set; }
    }
    
    // Events for reveal system
    public class RevealSequenceStartedEvent : IUIEvent
    {
        public ulong InitiativePlayer;
        public Dictionary<ulong, int> PlayerCardCounts;
    }
    
    public class CardRevealedEvent : IUIEvent
    {
        public ulong PlayerId;
        public int CardId;
        public int ScoreChange;
        public int NewPlayerScore;
        public int NewOpponentScore;
    }
    
    public class RevealSequenceCompletedEvent : IUIEvent
    {
        public Dictionary<ulong, int> PlayerScores;
    }
    
    public class NextTurnReadyEvent : IUIEvent { }
}