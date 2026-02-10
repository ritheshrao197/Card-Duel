using System.Collections.Generic;
using CardDuel.Domain.State;
using CardDuel.Domain.Commands;
using CardDuel.Domain.Events;
using CardDuel.Core;

namespace CardDuel.Application.UseCases
{
    /// <summary>
    /// Application service that orchestrates game flow
    /// Validates flow, not rules - rules live in domain
    /// </summary>
    public class GameApplicationService
    {
        private readonly MatchState _matchState;

        public GameApplicationService(MatchState matchState)
        {
            _matchState = matchState;
        }

        /// <summary>
        /// Handle any command and route to appropriate domain logic
        /// This is the traffic controller - it knows when, not how
        /// </summary>
        public IEnumerable<IEvent> HandleCommand(ICommand command)
        {
            var events = new List<IEvent>();

            switch (command)
            {
                case StartMatchCommand startCmd:
                    events.AddRange(HandleStartMatch(startCmd));
                    break;

                case PlayCardCommand playCmd:
                    events.AddRange(HandlePlayCard(playCmd));
                    break;

                case EndTurnCommand endCmd:
                    events.AddRange(HandleEndTurn(endCmd));
                    break;

                case ConcedeCommand concedeCmd:
                    events.AddRange(HandleConcede(concedeCmd));
                    break;

                case DrawCardCommand drawCmd:
                    events.AddRange(HandleDrawCard(drawCmd));
                    break;
            }

            return events;
        }

        private IEnumerable<IEvent> HandleStartMatch(StartMatchCommand command)
        {
            // Validate we can start a match
            if (_matchState.CurrentPhase != GamePhase.WaitingForPlayers)
                return new IEvent[] { new CommandRejectedEvent("Match already started") };

            _matchState.AddPlayer(command.PlayerId);
            return _matchState.StartGame();
        }

        private IEnumerable<IEvent> HandlePlayCard(PlayCardCommand command)
        {
            // Validate game phase
            if (_matchState.CurrentPhase != GamePhase.TurnInProgress)
                return new IEvent[] { new CommandRejectedEvent("Cannot play card outside of turn") };

            return _matchState.PlayCard(command.PlayerId, command.CardId);
        }

        private IEnumerable<IEvent> HandleEndTurn(EndTurnCommand command)
        {
            // Validate game phase
            if (_matchState.CurrentPhase != GamePhase.TurnInProgress)
                return new IEvent[] { new CommandRejectedEvent("Cannot end turn outside of turn phase") };

            return _matchState.EndPlayerTurn(command.PlayerId);
        }

        private IEnumerable<IEvent> HandleConcede(ConcedeCommand command)
        {
            // In a real implementation, this would end the game with the other player as winner
            return new IEvent[] { new MatchConcededEvent(command.PlayerId) };
        }

        private IEnumerable<IEvent> HandleDrawCard(DrawCardCommand command)
        {
            // Validate game phase and rules
            if (_matchState.CurrentPhase != GamePhase.TurnInProgress)
                return new IEvent[] { new CommandRejectedEvent("Cannot draw card outside of turn") };

            // In a real implementation, you'd check deck/hand limits
            return new IEvent[] { new CardDrawnEvent(command.PlayerId) };
        }
    }

    /// <summary>
    /// Command rejected event - for flow validation failures
    /// </summary>
    public class CommandRejectedEvent : IEvent
    {
        public string Reason { get; }

        public CommandRejectedEvent(string reason)
        {
            Reason = reason;
        }
    }

    /// <summary>
    /// Match conceded event
    /// </summary>
    public class MatchConcededEvent : IEvent
    {
        public ulong PlayerId { get; }

        public MatchConcededEvent(ulong playerId)
        {
            PlayerId = playerId;
        }
    }

    /// <summary>
    /// Card drawn event
    /// </summary>
    public class CardDrawnEvent : IEvent
    {
        public ulong PlayerId { get; }

        public CardDrawnEvent(ulong playerId)
        {
            PlayerId = playerId;
        }
    }
}