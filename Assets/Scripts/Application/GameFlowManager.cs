using CardDuel.UI.Events;
using CardDuel.Networking;
using CardDuel.Utils;
using CardDuel.Gameplay;
using Assets.Scripts.Application;
using Unity.Netcode;
using System.Collections.Generic;

namespace Assets.Scripts.Application
{
    /// <summary>
    /// Unified manager for complete game flow including 6-turn structure
    /// Handles matchmaking, game phases, turn management, and reveal sequences
    /// </summary>
    public class GameFlowManager
    {
        private readonly NetcodeBootstrapper _netcode;
        private readonly MatchmakingNetworkController _networkMatchmaking;
        private readonly GameplayUseCase _gameplay;
        private readonly GameState _gameState;
        
        private int _currentTurn;
        private bool _isGameActive;
        private Dictionary<ulong, List<CardData>> _playerFoldedCards;
        
        public int CurrentTurn => _currentTurn;
        public bool IsGameActive => _isGameActive;
        public int TotalTurns => MatchStructure.TOTAL_TURNS;
        
        public GameFlowManager(
            NetcodeBootstrapper netcode,
            MatchmakingNetworkController networkMatchmaking,
            GameplayUseCase gameplay,
            GameState gameState)
        {
            _netcode = netcode;
            _networkMatchmaking = networkMatchmaking;
            _gameplay = gameplay;
            _gameState = gameState;
            
            _playerFoldedCards = new Dictionary<ulong, List<CardData>>();
            _currentTurn = 0;
            _isGameActive = false;

            RegisterEvents();
            Log.App("GameFlowManager initialized");
        }

        private void RegisterEvents()
        {
            // Matchmaking events
            UIEventBus.Subscribe<ConnectRequestedEvent>(OnConnectRequested);
            UIEventBus.Subscribe<PlayerReadyClickedEvent>(OnPlayerReady);
            
            // Game flow events
            UIEventBus.Subscribe<GameplayStartRequestedEvent>(OnGameplayStartRequested);
            UIEventBus.Subscribe<PlayerEndedTurnEvent>(OnPlayerEndedTurn);
            UIEventBus.Subscribe<RevealPhaseStartedEvent>(OnRevealPhaseStarted);
            UIEventBus.Subscribe<RevealSequenceCompletedEvent>(OnRevealSequenceCompleted);
            UIEventBus.Subscribe<NextTurnReadyEvent>(OnNextTurnReady);
        }

        #region Matchmaking Methods

        private void OnConnectRequested(ConnectRequestedEvent evt)
        {
            _netcode.StartConnection(evt.Role);

            UIEventBus.Publish(new UIStateChangedEvent
            {
                State = UIState.Matchmaking
            });
            
            Log.App($"Connection started | Role={evt.Role}");
        }

        private void OnPlayerReady(PlayerReadyClickedEvent evt)
        {
            _networkMatchmaking.SubmitReady();
            Log.App("Player ready submitted");
        }

        #endregion
        
        #region Game Flow Methods
        
        private void OnGameplayStartRequested(GameplayStartRequestedEvent evt)
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                Log.App("Gameplay start ignored (not server)");
                return;
            }
            
            Log.App("Starting 6-turn match");
            _isGameActive = true;
            _currentTurn = 0;
            _playerFoldedCards.Clear();
            
            // Reset game state
            _gameState.ResetMatch();
            
            // Initialize players
            foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
            {
                _gameState.RegisterPlayer(clientId);
                _playerFoldedCards[clientId] = new List<CardData>();
            }
            
            // Start first turn
            StartNextTurn();
        }
        
        private void StartNextTurn()
        {
            _currentTurn++;
            
            if (_currentTurn > MatchStructure.TOTAL_TURNS)
            {
                EndMatch();
                return;
            }
            
            Log.App($"Starting turn {_currentTurn}/{MatchStructure.TOTAL_TURNS}");
            
            // Clear turn ended players for new turn
            _gameState.TurnEndedPlayers.Clear();
            
            // Publish turn started event
            UIEventBus.Publish(new TurnStartedEvent
            {
                Turn = _currentTurn,
                Energy = MatchStructure.GetEnergyForTurn(_currentTurn),
                MaxEnergy = MatchStructure.MAX_ENERGY
            });
            
            // Update UI state
            UIEventBus.Publish(new UIStateChangedEvent
            {
                State = UIState.Gameplay
            });
        }
        
        private void OnPlayerEndedTurn(PlayerEndedTurnEvent evt)
        {
            Log.App($"Player {evt.PlayerId} ended turn {_currentTurn}");
            
            // Store folded cards from game state
            if (_gameState.PlayedCards.ContainsKey(evt.PlayerId))
            {
                _playerFoldedCards[evt.PlayerId] = new List<CardData>(_gameState.PlayedCards[evt.PlayerId]);
            }
            
            // Check if both players have ended turn
            if (_gameState.TurnEndedPlayers.Count == NetworkManager.Singleton.ConnectedClientsIds.Count)
            {
                Log.App("Both players ended turn - proceeding to reveal phase");
            }
        }
        
        private void OnRevealPhaseStarted(RevealPhaseStartedEvent evt)
        {
            Log.App($"Reveal phase started for turn {_currentTurn}");
            
            // Prepare cards for reveal
            var revealCards = new Dictionary<ulong, List<CardData>>();
            foreach (var kvp in _playerFoldedCards)
            {
                revealCards[kvp.Key] = new List<CardData>(kvp.Value);
            }
            
            // TODO: Trigger reveal sequence system with revealCards
        }
        
        private void OnRevealSequenceCompleted(RevealSequenceCompletedEvent evt)
        {
            Log.App($"Reveal sequence completed for turn {_currentTurn}");
            
            // Update scores in game state
            foreach (var kvp in evt.PlayerScores)
            {
                _gameState.Scores[kvp.Key] = kvp.Value;
            }
            
            // Check if match is complete
            if (_currentTurn >= MatchStructure.TOTAL_TURNS)
            {
                EndMatch();
            }
            else
            {
                // Prepare for next turn
                PrepareNextTurn();
            }
        }
        
        private void OnNextTurnReady(NextTurnReadyEvent evt)
        {
            StartNextTurn();
        }
        
        private void PrepareNextTurn()
        {
            Log.App($"Preparing for turn {_currentTurn + 1}");
            
            // Clear folded cards for next turn
            _playerFoldedCards.Clear();
            
            // TODO: Draw cards for next turn
            
            // Notify that next turn is ready
            UIEventBus.Publish(new NextTurnReadyEvent());
        }
        
        private void EndMatch()
        {
            _isGameActive = false;
            
            // Determine winner
            ulong winner = DetermineWinner();
            
            Log.App($"Match completed - Winner: {winner}");
            
            // Publish match results
            UIEventBus.Publish(new MatchCompletedEvent
            {
                WinnerId = winner,
                PlayerScores = new Dictionary<ulong, int>(_gameState.Scores)
            });
            
            UIEventBus.Publish(new UIStateChangedEvent
            {
                State = UIState.Results
            });
        }
        
        private ulong DetermineWinner()
        {
            if (_gameState.Scores.Count < 2) 
                return NetworkManager.ServerClientId;
            
            var playerIds = new List<ulong>(_gameState.Scores.Keys);
            ulong playerA = playerIds[0];
            ulong playerB = playerIds[1];
            
            int scoreA = _gameState.Scores[playerA];
            int scoreB = _gameState.Scores[playerB];
            
            if (scoreA > scoreB)
                return playerA;
            else if (scoreB > scoreA)
                return playerB;
            else
                return NetworkManager.ServerClientId; // Tie
        }
        
        #endregion

        public void Dispose()
        {
            // Unsubscribe from all events
            UIEventBus.Unsubscribe<ConnectRequestedEvent>(OnConnectRequested);
            UIEventBus.Unsubscribe<PlayerReadyClickedEvent>(OnPlayerReady);
            UIEventBus.Unsubscribe<GameplayStartRequestedEvent>(OnGameplayStartRequested);
            UIEventBus.Unsubscribe<PlayerEndedTurnEvent>(OnPlayerEndedTurn);
            UIEventBus.Unsubscribe<RevealPhaseStartedEvent>(OnRevealPhaseStarted);
            UIEventBus.Unsubscribe<RevealSequenceCompletedEvent>(OnRevealSequenceCompleted);
            UIEventBus.Unsubscribe<NextTurnReadyEvent>(OnNextTurnReady);
            
            Log.App("GameFlowManager disposed");
        }
    }
    
    // Additional events for game flow
    public class MatchCompletedEvent : IUIEvent
    {
        public ulong WinnerId;
        public Dictionary<ulong, int> PlayerScores;
    }
}