using CardDuel.UI.Events;
using CardDuel.Networking;
using CardDuel.Utils;
using Assets.Scripts.Application;

namespace Assets.Scripts.Application
{
    /// <summary>
    /// Unified manager for all game flow and state transitions
    /// Consolidates matchmaking, reconnection, and game phase management
    /// Optimized consolidated version 3.0
    /// </summary>
    public class GameFlowManager
    {
        private readonly NetcodeBootstrapper _netcode;
        private readonly MatchmakingNetworkController _networkMatchmaking;
        private readonly GameplayUseCase _gameplay;

        public GameFlowManager(
            NetcodeBootstrapper netcode,
            MatchmakingNetworkController networkMatchmaking,
            GameplayUseCase gameplay)
        {
            _netcode = netcode;
            _networkMatchmaking = networkMatchmaking;
            _gameplay = gameplay;

            RegisterEvents();
            Log.App("GameFlowManager initialized");
        }

        private void RegisterEvents()
        {
            // Matchmaking events
            UIEventBus.Subscribe<ConnectRequestedEvent>(OnConnectRequested);
            UIEventBus.Subscribe<PlayerReadyClickedEvent>(OnPlayerReady);
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



        public void Dispose()
        {
            // Unsubscribe from all events
            UIEventBus.Unsubscribe<ConnectRequestedEvent>(OnConnectRequested);
            UIEventBus.Unsubscribe<PlayerReadyClickedEvent>(OnPlayerReady);
            
            Log.App("GameFlowManager disposed");
        }
    }
}