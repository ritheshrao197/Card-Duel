using TMPro;
using UnityEngine.UI;
using CardDuel.Events;
using CardDuel.Networking.Netcode;
using CardDuel.UI.Core;
using UnityEngine;

namespace CardDuel.UI.Matchmaking
{
    public class MatchmakingPanel : UIPanel
    {
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private TextMeshProUGUI playerCountText;
        [SerializeField] private Button readyButton;

        private NetworkBootstrap _bootstrap;
        private bool _isReady;

        private void Awake()
        {
            _bootstrap = FindObjectOfType<NetworkBootstrap>();
            readyButton.interactable = false;
        }

        public override void SubscribeToEvents()
        {
            EventBus.Subscribe<PlayerConnectedEvent>(OnPlayerConnected);
            EventBus.Subscribe<GameStartEvent>(OnGameStart);
        }

        public override void UnsubscribeFromEvents()
        {
            EventBus.Unsubscribe<PlayerConnectedEvent>(OnPlayerConnected);
            EventBus.Unsubscribe<GameStartEvent>(OnGameStart);
        }

        public void StartHost()
        {
            statusText.text = "Starting host…";
            _bootstrap.StartHost();
        }

        public void StartClient()
        {
            statusText.text = "Connecting to host…";
            _bootstrap.StartClient();
        }

        public void SetReady()
        {
            if (_isReady) return;

            _isReady = true;
            readyButton.interactable = false;
            statusText.text = "Ready. Waiting for opponent…";

            NetworkClient.SendPlayerReady();
        }

        private void OnPlayerConnected(PlayerConnectedEvent e)
        {
            playerCountText.text = $"Players: {e.playerCount}/2";

            readyButton.interactable = e.playerCount == 2;
            statusText.text = e.playerCount == 2
                ? "Both players connected. Press Ready."
                : "Waiting for another player…";
        }

        private void OnGameStart(GameStartEvent e)
        {
            UIManager.Instance.SwitchTo(UIState.Gameplay);
        }
    }
}
