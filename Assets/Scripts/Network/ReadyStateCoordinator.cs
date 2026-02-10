using CardDuel.UI.Events;

namespace CardDuel.Networking
{
    public class ReadyStateCoordinator
    {
        private bool _localReady;
        private bool _remoteReady;

        public ReadyStateCoordinator()
        {
            UIEventBus.Subscribe<PlayerReadyClickedEvent>(_ => OnLocalReady());
            // Subscribe to network message: OnRemoteReady()
        }

        private void OnLocalReady()
        {
            _localReady = true;
            TryStartGame();
            // Send ready JSON to server
        }

        public void OnRemoteReady()
        {
            _remoteReady = true;
            TryStartGame();
        }

        private void TryStartGame()
        {
            if (_localReady && _remoteReady)
            {
                UIEventBus.Publish(new BothPlayersReadyEvent());
            }
        }
    }
}
