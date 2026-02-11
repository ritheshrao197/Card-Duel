using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using CardDuel.UI.Events;
using CardDuel.Networking;
using CardDuel.Gameplay;

namespace CardDuel.Gameplay
{
    /// <summary>
    /// Main gameplay manager that orchestrates all game systems
    /// </summary>
    public class GameplayManager : NetworkBehaviour
    {
        [Header("Game Systems")]
        [SerializeField] private TurnStagingSystem _stagingSystem;
        [SerializeField] private TurnFoldingSystem _foldingSystem;
        [SerializeField] private HiddenStateSystem _hiddenStateSystem;
        [SerializeField] private RevealSequenceSystem _revealSystem;
        [SerializeField] private DeckService _deckService;
        
        [Header("Player References")]
        [SerializeField] private PlayerHandState _playerHand;
        [SerializeField] private ulong _localPlayerId;
        
        private int _currentTurn = 1;
        private bool _isGameActive = false;
        private Dictionary<ulong, List<CardData>> _playerDecks;
        private Dictionary<ulong, List<CardData>> _foldedCards;
        
        public int CurrentTurn => _currentTurn;
        public bool IsGameActive => _isGameActive;
        public TurnStagingSystem StagingSystem => _stagingSystem;
        public TurnFoldingSystem FoldingSystem => _foldingSystem;
        
        private void Awake()
        {
            _playerDecks = new Dictionary<ulong, List<CardData>>();
            _foldedCards = new Dictionary<ulong, List<CardData>>();
            
            InitializeSystems();
        }
        
        private void InitializeSystems()
        {
            // Initialize staging system
            _stagingSystem = new TurnStagingSystem(_playerHand);
            
            // Initialize folding system
            _foldingSystem = gameObject.AddComponent<TurnFoldingSystem>();
            _foldingSystem.Initialize(_stagingSystem, _localPlayerId);
            
            // Initialize hidden state system
            _hiddenStateSystem = gameObject.AddComponent<HiddenStateSystem>();
            _hiddenStateSystem.Initialize(_localPlayerId);
            
            // Initialize reveal system
            _revealSystem = gameObject.AddComponent<RevealSequenceSystem>();
            
            // Setup event listeners
            SetupEventListeners();
        }
        
        private void SetupEventListeners()
        {
            // Staging system events
            _stagingSystem.OnCardMovedToStaging += OnCardStaged;
            _stagingSystem.OnCardMovedBackToHand += OnCardUnstaged;
            _stagingSystem.OnEnergyChanged += OnEnergyChanged;
            
            // Folding system events
            _foldingSystem.OnTimerUpdated += OnTimerUpdated;
            _foldingSystem.OnTurnTimeout += OnTurnTimeout;
            _foldingSystem.OnTurnFolded += OnTurnFolded;
            
            // Reveal system events
            _revealSystem.OnRevealStarted += OnRevealStarted;
            _revealSystem.OnCardRevealed += OnCardRevealed;
            _revealSystem.OnRevealCompleted += OnRevealCompleted;
        }
        
        /// <summary>
        /// Start the game match
        /// </summary>
        public void StartMatch()
        {
            if (_isGameActive) return;
            
            _currentTurn = 1;
            _isGameActive = true;
            
            // Initialize player decks
            foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
            {
                InitializePlayerDeck(clientId);
            }

            // TODO: Publish match started event
        }
        
        /// <summary>
        /// Initialize a player's deck
        /// </summary>
        private void InitializePlayerDeck(ulong clientId)
        {
            // This would typically be handled by the DeckService
            // For now, we'll create placeholder decks
            _playerDecks[clientId] = new List<CardData>();
        }
        
        /// <summary>
        /// Start a new turn
        /// </summary>
        public void StartTurn(int turnNumber)
        {
            _currentTurn = turnNumber;
            
            // Calculate energy for this turn
            int energy = MatchStructure.GetEnergyForTurn(turnNumber);
            
            // Initialize staging system
            _stagingSystem.InitializeTurn(energy);
            
            // Start turn timer
            _foldingSystem.StartTurnTimer();
            
            // Reset hidden state for new turn
            _hiddenStateSystem.ResetForNewTurn();
            
            // Draw cards for this turn
            DrawCardsForTurn(turnNumber);
            
            // TODO: Publish turn started event with proper event type
        }
        
        /// <summary>
        /// Draw cards for current turn
        /// </summary>
        private void DrawCardsForTurn(int turnNumber)
        {
            // Card drawing is handled by the existing DeckService /
            // GameSessionNetworkController / GameplayUseCase flow.
            // Here we only refresh hidden-state counts based on the
            // current hand and staging areas.
            _hiddenStateSystem.UpdatePlayerState(
                _localPlayerId, 
                _playerHand.Hand.Count, 
                _stagingSystem.StagingArea.Count, 
                0
            );
        }
        
        /// <summary>
        /// Player ends their turn
        /// </summary>
        public void EndTurn()
        {
            _foldingSystem.EndTurn();
        }
        
        /// <summary>
        /// Handle card staging
        /// </summary>
        public bool StageCard(int cardId)
        {
            bool success = _stagingSystem.MoveCardToStaging(cardId);
            
            if (success)
            {
                // Update hidden state
                _hiddenStateSystem.UpdatePlayerState(
                    _localPlayerId,
                    _playerHand.Hand.Count,
                    _stagingSystem.StagingArea.Count,
                    _foldingSystem.FoldedCards.Count
                );
            }
            
            return success;
        }
        
        /// <summary>
        /// Handle card unstaging
        /// </summary>
        public bool UnstageCard(int cardId)
        {
            bool success = _stagingSystem.MoveCardBackToHand(cardId);
            
            if (success)
            {
                // Update hidden state
                _hiddenStateSystem.UpdatePlayerState(
                    _localPlayerId,
                    _playerHand.Hand.Count,
                    _stagingSystem.StagingArea.Count,
                    _foldingSystem.FoldedCards.Count
                );
            }
            
            return success;
        }
        
        /// <summary>
        /// Start reveal phase when both players have folded
        /// </summary>
        private void StartRevealPhase()
        {
            _revealSystem.InitializeReveal(_foldedCards);
            _revealSystem.StartRevealSequence();
        }
        
        #region Event Handlers
        
        private void OnCardStaged(CardData card)
        {
            // Update UI and hidden state
            _hiddenStateSystem.UpdatePlayerState(
                _localPlayerId,
                _playerHand.Hand.Count,
                _stagingSystem.StagingArea.Count,
                _foldingSystem.FoldedCards.Count
            );
        }
        
        private void OnCardUnstaged(CardData card)
        {
            // Update UI and hidden state
            _hiddenStateSystem.UpdatePlayerState(
                _localPlayerId,
                _playerHand.Hand.Count,
                _stagingSystem.StagingArea.Count,
                _foldingSystem.FoldedCards.Count
            );
        }
        
        private void OnEnergyChanged()
        {
            // TODO: Publish energy updated event
        }
        
        private void OnTimerUpdated(float remainingTime)
        {
            // TODO: Publish turn timer updated event
        }
        
        private void OnTurnTimeout()
        {
            // Turn automatically ends due to timeout
            EndTurn();
        }
        
        private void OnTurnFolded(List<CardData> foldedCards)
        {
            // Store folded cards
            _foldedCards[_localPlayerId] = new List<CardData>(foldedCards);
            
            // Check if both players have folded
            if (_foldedCards.Count == NetworkManager.Singleton.ConnectedClientsIds.Count)
            {
                StartRevealPhase();
            }
        }
        
        private void OnRevealStarted()
        {
            // TODO: Publish reveal phase started event
        }
        
        private void OnCardRevealed(RevealStep step)
        {
            // Update scores in UI
            UIEventBus.Publish(new ScoreUpdatedEvent
            {
                PlayerId = step.PlayerId,
                Delta = step.FinalScore,
                NewScore = step.NewPlayerScore
            });
        }
        
        private void OnRevealCompleted()
        {
            // Check if match is complete
            if (MatchStructure.IsMatchComplete(_currentTurn))
            {
                EndMatch();
            }
            else
            {
                // Start next turn
                StartTurn(_currentTurn + 1);
            }
        }
        
        private void EndMatch()
        {
            _isGameActive = false;
            
            // Determine winner
            ulong winner = DetermineWinner();
            
            // TODO: Publish match completed event
        }
        
        private ulong DetermineWinner()
        {
            var scores = _revealSystem.PlayerScores;
            if (scores.Count < 2) return NetworkManager.ServerClientId;
            
            var playerIds = new List<ulong>(scores.Keys);
            ulong playerA = playerIds[0];
            ulong playerB = playerIds[1];
            
            if (scores[playerA] > scores[playerB])
                return playerA;
            else if (scores[playerB] > scores[playerA])
                return playerB;
            else
                return NetworkManager.ServerClientId; // Tie
        }
        
        #endregion
    }
    
    // Events for gameplay manager
    // public class MatchStartedEvent
    // {
    //     public int TotalTurns;
    //     public int StartingHandSize;
    // }
    
    // public class TurnStartedEvent
    // {
    //     public int TurnNumber;
    //     public int Energy;
    //     public int MaxEnergy;
    // }
    
    // public class EnergyUpdatedEvent
    // {
    //     public int AvailableEnergy;
    //     public int UsedEnergy;
    //     public int RemainingEnergy;
    // }
    
    // public class TurnTimerUpdatedEvent
    // {
    //     public float RemainingTime;
    // }
    
    // public class MatchCompletedEvent
    // {
    //     public ulong WinnerId;
    //     public Dictionary<ulong, int> PlayerScores;
    // }
}