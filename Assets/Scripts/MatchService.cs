using CardDuel.Domain;
using CardDuel.Domain.Commands;
using CardDuel.Networking;
using Unity.Netcode;

namespace CardDuel.Server
{
    public class MatchService
    {
        private readonly MatchState _matchState = new();
        private readonly NetworkEventRelay _relay;

        public MatchService(NetworkEventRelay relay)
        {
            _relay = relay;
        }

        public void Handle(ICommand command)
        {
            var events = _matchState.Apply(command);
            foreach (var evt in events)
                _relay.Broadcast(evt);
        }
    }
}
