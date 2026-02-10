using CardDuel.UI.Events;
using CardDuel.Networking;

namespace CardDuel.Application
{
    public class MatchmakingUseCase
    {
        private readonly NetcodeBootstrapper _netcode;
        private readonly MatchmakingNetworkController _networkMatchmaking;

        public MatchmakingUseCase(
            NetcodeBootstrapper netcode,
            MatchmakingNetworkController networkMatchmaking)
        {
            _netcode = netcode;
            _networkMatchmaking = networkMatchmaking;
        }

        public void OnConnectRequested(ConnectRequestedEvent evt)
        {
            _netcode.StartConnection(evt.Role);

            UIEventBus.Publish(new UIStateChangedEvent
            {
                State = UIState.Matchmaking
            });
        }

        public void OnPlayerReady(PlayerReadyClickedEvent evt)
        {
            _networkMatchmaking.SubmitReady();
        }
    }
}
