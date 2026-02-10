using System.Collections.Generic;
using CardDuel.Domain.Commands;
using CardDuel.Domain.Events;
using CardDuel.Gameplay.Commands;
using CardDuel.Gameplay.Events;

namespace CardDuel.Domain
{
    public class MatchState
    {
        private readonly ulong[] _players = { 0, 1 };
        private int _turnIndex;
        private readonly TurnState _turnState = new();

        public MatchState()
        {
            _turnIndex = 0;
            _turnState.StartTurn(_players[_turnIndex]);
        }

        public List<IGameEvent> Apply(ICommand command)
        {
            var events = new List<IGameEvent>();

            if (command.PlayerId != _turnState.CurrentPlayerId)
                return events;

            if (command is EndTurnCommand)
            {
                events.Add(new TurnEndedEvent
                {
                    PlayerId = _turnState.CurrentPlayerId
                });

                AdvanceTurn(events);
            }

            if (command is PlayCardCommand play)
            {
                events.Add(new CardPlayedEvent
                {
                    PlayerId = play.PlayerId,
                    CardId = play.CardId
                });
            }

            return events;
        }

        private void AdvanceTurn(List<IGameEvent> events)
        {
            _turnIndex = (_turnIndex + 1) % _players.Length;
            _turnState.StartTurn(_players[_turnIndex]);

            events.Add(new TurnStartedEvent
            {
                PlayerId = _turnState.CurrentPlayerId
            });
        }
    }
}
