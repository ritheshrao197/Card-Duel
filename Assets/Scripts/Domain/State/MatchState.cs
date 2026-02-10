using System;
using System.Collections.Generic;
using CardDuel.Core;

namespace CardDuel.Domain.State
{
    /// <summary>
    /// Game phase enumeration - pure domain logic
    /// </summary>
    public enum GamePhase
    {
        WaitingForPlayers,
        TurnInProgress,
        RevealingCards,
        TurnComplete,
        GameOver
    }

    /// <summary>
    /// Player role identification
    /// </summary>
    public enum PlayerRole
    {
        Player1,
        Player2
    }

    /// <summary>
    /// Core match state - deterministic, pure, testable
    /// This is the heart of the game that would survive if Unity disappeared
    /// </summary>
    public class MatchState
    {
        public GamePhase CurrentPhase { get; private set; }
        public int CurrentTurn { get; private set; }
        public int MaxTurns { get; }
        public Dictionary<ulong, PlayerState> PlayerStates { get; }
        public Dictionary<ulong, List<PlayedCard>> PlayedCards { get; }
        public ulong InitiativePlayerId { get; private set; }

        public MatchState(int maxTurns = 6)
        {
            MaxTurns = maxTurns;
            CurrentTurn = 1;
            CurrentPhase = GamePhase.WaitingForPlayers;
            PlayerStates = new Dictionary<ulong, PlayerState>();
            PlayedCards = new Dictionary<ulong, List<PlayedCard>>();
        }

        /// <summary>
        /// Add a player to the match
        /// </summary>
        public void AddPlayer(ulong playerId)
        {
            if (!PlayerStates.ContainsKey(playerId))
            {
                PlayerStates[playerId] = new PlayerState(playerId);
                PlayedCards[playerId] = new List<PlayedCard>();
            }
        }

        /// <summary>
        /// Start the game
        /// </summary>
        public IEnumerable<IEvent> StartGame()
        {
            CurrentPhase = GamePhase.TurnInProgress;
            var events = new List<IEvent>();
            
            events.Add(new GameStartedEvent(MaxTurns));
            
            // Start first turn
            var turnEvents = StartTurn(CurrentTurn);
            events.AddRange(turnEvents);
            
            return events;
        }

        /// <summary>
        /// Start a new turn
        /// </summary>
        public IEnumerable<IEvent> StartTurn(int turnNumber)
        {
            CurrentTurn = turnNumber;
            CurrentPhase = GamePhase.TurnInProgress;
            var events = new List<IEvent>();

            // Reset player states for this turn
            foreach (var playerState in PlayerStates.Values)
            {
                playerState.HasEndedTurn = false;
                playerState.AvailableCost = turnNumber; // Cost increases each turn
            }

            events.Add(new TurnStartedEvent(turnNumber));
            return events;
        }

        /// <summary>
        /// Player ends their turn
        /// </summary>
        public IEnumerable<IEvent> EndPlayerTurn(ulong playerId)
        {
            if (!PlayerStates.ContainsKey(playerId))
                return Array.Empty<IEvent>();

            PlayerStates[playerId].HasEndedTurn = true;
            var events = new List<IEvent> { new PlayerEndedTurnEvent(playerId) };

            // Check if all players have ended their turn
            bool allPlayersEnded = true;
            foreach (var playerState in PlayerStates.Values)
            {
                if (!playerState.HasEndedTurn)
                {
                    allPlayersEnded = false;
                    break;
                }
            }

            if (allPlayersEnded)
            {
                var revealEvents = StartRevealPhase();
                events.AddRange(revealEvents);
            }

            return events;
        }

        /// <summary>
        /// Start the reveal phase
        /// </summary>
        private IEnumerable<IEvent> StartRevealPhase()
        {
            CurrentPhase = GamePhase.RevealingCards;
            var events = new List<IEvent>();

            // Determine initiative based on scores
            DetermineInitiative();
            events.Add(new InitiativeDeterminedEvent(InitiativePlayerId));

            // Start alternating reveal sequence
            var revealEvents = StartRevealSequence();
            events.AddRange(revealEvents);

            return events;
        }

        /// <summary>
        /// Determine which player has initiative
        /// </summary>
        private void DetermineInitiative()
        {
            var players = new List<ulong>(PlayerStates.Keys);
            if (players.Count >= 2)
            {
                var player1State = PlayerStates[players[0]];
                var player2State = PlayerStates[players[1]];

                if (player1State.Score > player2State.Score)
                {
                    InitiativePlayerId = players[0];
                }
                else if (player2State.Score > player1State.Score)
                {
                    InitiativePlayerId = players[1];
                }
                else
                {
                    // Tie - randomly choose
                    InitiativePlayerId = UnityEngine.Random.Range(0, 2) == 0 ? players[0] : players[1];
                }
            }
        }

        /// <summary>
        /// Start the reveal sequence
        /// </summary>
        private IEnumerable<IEvent> StartRevealSequence()
        {
            var events = new List<IEvent>();
            var players = new List<ulong>(PlayerStates.Keys);
            var opponentPlayerId = players[0] == InitiativePlayerId ? players[1] : players[0];

            var initiativeCards = PlayedCards[InitiativePlayerId];
            var opponentCards = PlayedCards[opponentPlayerId];

            int maxCards = Math.Max(initiativeCards.Count, opponentCards.Count);

            for (int i = 0; i < maxCards; i++)
            {
                // Reveal initiative player's card if available
                if (i < initiativeCards.Count)
                {
                    var cardEvent = new CardRevealedEvent(InitiativePlayerId, initiativeCards[i].CardId, i);
                    events.Add(cardEvent);
                }

                // Reveal opponent's card if available
                if (i < opponentCards.Count)
                {
                    var cardEvent = new CardRevealedEvent(opponentPlayerId, opponentCards[i].CardId, i);
                    events.Add(cardEvent);
                }
            }

            events.Add(new RevealPhaseCompletedEvent());
            return events;
        }

        /// <summary>
        /// Play a card from a player's hand
        /// </summary>
        public IEnumerable<IEvent> PlayCard(ulong playerId, int cardId)
        {
            if (CurrentPhase != GamePhase.TurnInProgress)
                return Array.Empty<IEvent>();

            if (!PlayerStates.ContainsKey(playerId))
                return Array.Empty<IEvent>();

            var playerState = PlayerStates[playerId];
            if (playerState.AvailableCost <= 0)
                return Array.Empty<IEvent>();

            // In a real implementation, you'd validate the card exists in hand
            // For now, we'll just create a played card
            var playedCard = new PlayedCard(cardId, PlayedCards[playerId].Count);
            PlayedCards[playerId].Add(playedCard);

            playerState.AvailableCost--; // Simplified cost deduction
            
            return new IEvent[] { new CardPlayedEvent(playerId, cardId) };
        }

        /// <summary>
        /// Update a player's score
        /// </summary>
        public IEnumerable<IEvent> UpdatePlayerScore(ulong playerId, int scoreChange)
        {
            if (!PlayerStates.ContainsKey(playerId))
                return Array.Empty<IEvent>();

            PlayerStates[playerId].Score += scoreChange;
            return new IEvent[] { new ScoreUpdatedEvent(playerId, PlayerStates[playerId].Score) };
        }

        /// <summary>
        /// End the current turn
        /// </summary>
        public IEnumerable<IEvent> EndTurn()
        {
            CurrentPhase = GamePhase.TurnComplete;
            var events = new List<IEvent> { new TurnEndedEvent(CurrentTurn) };

            // Clear played cards for next turn
            foreach (var playerId in PlayerStates.Keys)
            {
                PlayedCards[playerId].Clear();
            }

            // Check if game is over
            if (CurrentTurn >= MaxTurns)
            {
                var gameOverEvents = EndGame();
                events.AddRange(gameOverEvents);
            }
            else
            {
                // Start next turn
                var nextTurnEvents = StartTurn(CurrentTurn + 1);
                events.AddRange(nextTurnEvents);
            }

            return events;
        }

        /// <summary>
        /// End the game and determine winner
        /// </summary>
        private IEnumerable<IEvent> EndGame()
        {
            CurrentPhase = GamePhase.GameOver;
            
            ulong winningPlayerId = 0;
            int highestScore = int.MinValue;

            foreach (var kvp in PlayerStates)
            {
                if (kvp.Value.Score > highestScore)
                {
                    highestScore = kvp.Value.Score;
                    winningPlayerId = kvp.Key;
                }
            }

            return new IEvent[] { new GameEndedEvent(winningPlayerId, highestScore) };
        }
    }

    /// <summary>
    /// Individual player state
    /// </summary>
    public class PlayerState
    {
        public ulong PlayerId { get; }
        public int Score { get; set; }
        public int AvailableCost { get; set; }
        public List<Card> Hand { get; }
        public bool HasEndedTurn { get; set; }

        public PlayerState(ulong playerId)
        {
            PlayerId = playerId;
            Score = 0;
            Hand = new List<Card>();
            HasEndedTurn = false;
            AvailableCost = 0;
        }
    }
}