using CardDuel.UI.Events;

namespace CardDuel.Application
{
    public class GameFlowController
    {
        private readonly MatchmakingUseCase _matchmaking;
        private readonly GameplayUseCase _gameplay;
        private readonly ReconnectUseCase _reconnect;

        public GameFlowController(
            MatchmakingUseCase matchmaking,
            GameplayUseCase gameplay,
            ReconnectUseCase reconnect)
        {
            _matchmaking = matchmaking;
            _gameplay = gameplay;
            _reconnect = reconnect;

            RegisterUIEvents();
        }

        private void RegisterUIEvents()
        {
            UIEventBus.Subscribe<ConnectRequestedEvent>(_matchmaking.OnConnectRequested);
            UIEventBus.Subscribe<PlayerReadyClickedEvent>(_matchmaking.OnPlayerReady);
            UIEventBus.Subscribe<EndTurnClickedEvent>(_gameplay.OnEndTurnClicked);
        }

        public void Dispose()
        {
            UIEventBus.Unsubscribe<ConnectRequestedEvent>(_matchmaking.OnConnectRequested);
            UIEventBus.Unsubscribe<PlayerReadyClickedEvent>(_matchmaking.OnPlayerReady);
            UIEventBus.Unsubscribe<EndTurnClickedEvent>(_gameplay.OnEndTurnClicked);
        }
    }
}