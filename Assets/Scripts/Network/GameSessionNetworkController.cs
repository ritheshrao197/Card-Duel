using Unity.Netcode;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CardDuel.UI.Events;
using CardDuel.Utils;
using CardDuel.Gameplay;

namespace CardDuel.Networking
{
    /// <summary>
    /// Consolidated network controller for all game session functionality
    /// Handles turns, card drawing, card playing, reveal phase, and game flow
    /// </summary>
    public class GameSessionNetworkController : JsonNetworkController
    {
        #region Private Fields
        
        // Turn management
        private float _turnRemaining;
        private bool _turnRunning;
        private int _currentRound = 1;
        private readonly Dictionary<ulong, int> _scores = new();
        
        // Card management
        private CardDrawNetworkController _cardDrawController;
        private CardPlayNetworkController _cardPlayController;
        [SerializeField] private GameplayManager _gameplayManager;
        
        #endregion

        #region Lifecycle Methods
        
        protected override void RegisterMessageHandlers()
        {
            RegisterHandler("drawCard", HandleDrawCardMessage);
            RegisterHandler("playCard", HandlePlayCardMessage);
            RegisterHandler("endTurn", HandleEndTurnMessage);
            RegisterHandler("turnStart", HandleTurnStartMessage);
            RegisterHandler("revealStart", HandleRevealStartMessage);
            RegisterHandler("roundResult", HandleRoundResultMessage);
        }

        private void Start()
        {
            // Find dependent controllers
            _cardDrawController = GetComponent<CardDrawNetworkController>();
            _cardPlayController = GetComponent<CardPlayNetworkController>();

            // Find gameplay manager if not wired in inspector
            if (_gameplayManager == null)
            {
                _gameplayManager = FindObjectOfType<GameplayManager>();
            }
        }

        private void Update()
        {
            if (!IsServer || !_turnRunning)
                return;

            _turnRemaining -= Time.deltaTime;

            BroadcastTurnTimerTickClientRpc(_turnRemaining);

            if (_turnRemaining <= 0f)
            {
                _turnRunning = false;
                Log.Net("Turn timer expired → auto end");

                // Notify all clients that the turn timer expired so they
                // can auto-end their turn using the existing UI flow.
                BroadcastTurnTimedOutClientRpc();
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Start a new turn for the game
        /// </summary>
        public void StartTurn()
        {
            if (!IsServer) return;

            _turnRemaining = 30f; // 30 seconds per turn
            _turnRunning = true;

            Log.Net($"Turn started | Round: {_currentRound}");

            // Ensure the authoritative gameplay systems are running
            if (_gameplayManager != null)
            {
                if (!_gameplayManager.IsGameActive)
                {
                    _gameplayManager.StartMatch();
                }

                _gameplayManager.StartTurn(_currentRound);
            }

            int energyForTurn = MatchStructure.GetEnergyForTurn(_currentRound);

            // Broadcast turn start to all clients
            var turnStartMessage = new TurnStartMessage
            {
                turn = _currentRound,
                energy = energyForTurn,
                maxEnergy = MatchStructure.MAX_ENERGY
            };

            SendToAllClients(turnStartMessage);
        }

        /// <summary>
        /// Request to end the current turn
        /// </summary>
        public void RequestEndTurn()
        {
            if (!IsSpawned || !IsClient) return;

            var endTurnMessage = new EndTurnMessage
            {
                playerId = GetPlayerId(NetworkManager.Singleton.LocalClientId)
            };

            SendToServer(endTurnMessage);
        }

        /// <summary>
        /// Start the reveal phase after both players end their turns
        /// </summary>
        public void StartRevealPhase(List<int> playerACards, List<int> playerBCards, ulong playerAId, ulong playerBId)
        {
            if (!IsServer) return;

            Log.Net("Starting reveal phase");

            // Initialize scores if needed
            if (!_scores.ContainsKey(playerAId)) _scores[playerAId] = 0;
            if (!_scores.ContainsKey(playerBId)) _scores[playerBId] = 0;

            StartCoroutine(RevealRoutine(playerAId, playerACards, playerBCards, playerAId, playerBId));
        }

        /// <summary>
        /// Start a new round
        /// </summary>
        public void StartNewRound()
        {
            _currentRound++;
            Log.Net($"Starting round {_currentRound}");
        }

        #endregion

        #region Message Handlers

        private void HandleDrawCardMessage(string jsonMessage)
        {
            var message = NetworkMessageSerializer.Deserialize<DrawCardMessage>(jsonMessage);
            if (message == null) return;

            // Forward to card draw controller for processing
            if (_cardDrawController != null)
            {
                _cardDrawController.GetComponent<NetworkObject>().SendMessage("HandleDrawCardMessage", jsonMessage);
            }
        }

        private void HandlePlayCardMessage(string jsonMessage)
        {
            var message = NetworkMessageSerializer.Deserialize<PlayCardMessage>(jsonMessage);
            if (message == null) return;

            // Forward to card play controller for validation and processing
            if (_cardPlayController != null)
            {
                _cardPlayController.GetComponent<NetworkObject>().SendMessage("HandlePlayCardMessage", jsonMessage);
            }
        }

        private void HandleEndTurnMessage(string jsonMessage)
        {
            var message = NetworkMessageSerializer.Deserialize<EndTurnMessage>(jsonMessage);
            if (message == null) return;

            ulong clientId = GetClientId(message.playerId);
            Log.Net($"EndTurn message received from {message.playerId}");

            // Process turn ending logic
            ProcessTurnEnd(clientId);

            // Check if both players have ended their turns
            CheckBothPlayersTurnEnd();
        }

        private void HandleTurnStartMessage(string jsonMessage)
        {
            var message = NetworkMessageSerializer.Deserialize<TurnStartMessage>(jsonMessage);
            if (message == null) return;

            Log.Net($"TurnStart message | Turn={message.turn} Energy={message.energy}");

            UIEventBus.Publish(new TurnStartedEvent
            {
                Turn = message.turn,
                Energy = message.energy,
                MaxEnergy = message.maxEnergy
            });
        }

        private void HandleRevealStartMessage(string jsonMessage)
        {
            Log.Net("Reveal phase started");
            UIEventBus.Publish(new UIStateChangedEvent
            {
                State = UIState.Reveal
            });
        }

        private void HandleRoundResultMessage(string jsonMessage)
        {
            // In a real implementation, parse scores from message
            UIEventBus.Publish(new RoundResultEvent
            {
                PlayerAScore = 0, // Get from message
                PlayerBScore = 0  // Get from message
            });
        }

        #endregion

        #region Turn Management

        private void ProcessTurnEnd(ulong clientId)
        {
            // Add client to ended players list in game state
            GameState.Instance.TurnEndedPlayers.Add(clientId);
        }

        private void CheckBothPlayersTurnEnd()
        {
            if (GameState.Instance.TurnEndedPlayers.Count == 
                NetworkManager.Singleton.ConnectedClientsIds.Count)
            {
                Log.Net("Both players ended turn - starting reveal");
                
                // Start reveal phase
                StartRevealPhaseForCurrentTurn();
            }
        }

        private void StartRevealPhaseForCurrentTurn()
        {
            // Get played cards for this turn from game state
            var playedCards = GameState.Instance.PlayedCards;
            
            // Extract card lists for each player
            List<int> playerACards = new List<int>();
            List<int> playerBCards = new List<int>();
            
            foreach (var kvp in playedCards)
            {
                foreach (var card in kvp.Value)
                {
                    if (kvp.Key == NetworkManager.ServerClientId)
                        playerACards.Add(card.Id);
                    else
                        playerBCards.Add(card.Id);
                }
            }

            // Start reveal with both players
            StartRevealPhase(
                playerACards, 
                playerBCards, 
                NetworkManager.ServerClientId, 
                GetFirstNonServerClientId()
            );
        }

        #endregion

        #region Reveal Phase

        private IEnumerator RevealRoutine(
            ulong initiativePlayer,
            List<int> playerACards,
            List<int> playerBCards,
            ulong playerAId,
            ulong playerBId)
        {
            int index = 0;
            ulong current = initiativePlayer;

            while (index < playerACards.Count || index < playerBCards.Count)
            {
                if (current == playerAId && index < playerACards.Count)
                    ResolveCard(playerAId, playerACards[index++]);

                else if (current == playerBId && index < playerBCards.Count)
                    ResolveCard(playerBId, playerBCards[index++]);

                current = current == playerAId ? playerBId : playerAId;

                yield return new WaitForSeconds(0.8f);
            }

            // Broadcast reveal finished
            var revealFinishedMessage = new NetworkMessage("revealFinished");
            SendToAllClients(revealFinishedMessage);

            // Update round
            _currentRound++;
            
            // Calculate scores
            CalculateRoundScores();
        }

        private void ResolveCard(ulong owner, int cardId)
        {
            // Host resolves logic using its own database
            var card = CardDatabaseProvider.Get(cardId);

            int delta = card.Power;

            if (card.Ability != null && card.Ability.Type == "GainRuns")
                delta += card.Ability.Value;

            _scores[owner] += delta;

            // Broadcast revealed card
            BroadcastCardRevealed(owner, cardId, delta, _scores[owner]);
        }

        private void BroadcastCardRevealed(ulong owner, int cardId, int delta, int newScore)
        {
            // In a real implementation, we would broadcast this as a JSON message
            UIEventBus.Publish(new CardRevealEvent
            {
                OwnerClientId = owner,
                CardId = cardId
            });

            UIEventBus.Publish(new ScoreUpdatedEvent
            {
                PlayerId = owner,
                Delta = delta,
                NewScore = newScore
            });
        }

        private void CalculateRoundScores()
        {
            // Calculate scores based on played cards
            foreach (var kvp in GameState.Instance.PlayedCards)
            {
                int total = 0;

                foreach (var card in kvp.Value)
                    total += card.Power;

                _scores[kvp.Key] += total;
            }

            // Broadcast results
            var resultMessage = new NetworkMessage("roundResult");
            SendToAllClients(resultMessage);

            GameState.Instance.NextRound();
        }

        #endregion

        #region Helper Methods

        private int GetPlayerAvailableEnergy(ulong clientId)
        {
            // Calculate available energy based on current turn and used energy
            // This is a simplified calculation - in practice, you'd track per-player energy
            var localHand = GameState.Instance.LocalHand;
            int usedEnergy = localHand.TotalDrawnCost();
            int totalEnergy = GameState.Instance.EnergyCap;
            return Mathf.Max(0, totalEnergy - usedEnergy);
        }

        private ulong GetFirstNonServerClientId()
        {
            foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
            {
                if (clientId != NetworkManager.ServerClientId)
                {
                    return clientId;
                }
            }
            return NetworkManager.ServerClientId; // fallback
        }

        [ClientRpc]
        private void BroadcastTurnTimerTickClientRpc(float remaining)
        {
            UIEventBus.Publish(new TurnTimerTickEvent { Remaining = remaining });
        }

        /// <summary>
        /// Notify clients that the turn timer has expired so they can
        /// trigger their local end-turn flow.
        /// </summary>
        [ClientRpc]
        private void BroadcastTurnTimedOutClientRpc()
        {
            UIEventBus.Publish(new EndTurnClickedEvent());
        }

        #endregion
    }
}