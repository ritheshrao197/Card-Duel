using Unity.Netcode;
using CardDuel.UI.Events;
using CardDuel.Utils;
using System.Collections.Generic;

namespace CardDuel.Networking
{
    /// <summary>
    /// Consolidated network controller for matchmaking, lobby, and connection management
    /// Handles player ready states, game starting, and connection management
    /// </summary>
    public class MatchmakingNetworkController : JsonNetworkController
    {
        #region Private Fields

        private NetworkVariable<bool> _hostReady = new NetworkVariable<bool>();
        private NetworkVariable<bool> _clientReady = new NetworkVariable<bool>();
        private NetworkSceneController _sceneController;
        
        #endregion

        #region Lifecycle Methods

        protected override void RegisterMessageHandlers()
        {
            RegisterHandler("playerReady", HandlePlayerReadyMessage);
            RegisterHandler("gameStart", HandleGameStartMessage);
            RegisterHandler("connectionStatus", HandleConnectionStatusMessage);
        }

        protected override void Awake()
        {
            base.Awake();
            
            // Initialize network variables
            _hostReady = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
            _clientReady = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        }

        private void Start()
        {
            // Find scene controller
            _sceneController = FindObjectOfType<NetworkSceneController>();
        }

        #endregion

        #region Public API

        /// <summary>
        /// Submit that the local player is ready
        /// </summary>
        public void SubmitReady()
        {
            if (!IsSpawned)
            {
                Log.Net("SubmitReady ignored (not spawned)");
                return;
            }

            Log.Net("SubmitReady called");
            
            // Send ready message via JSON
            var readyMessage = new PlayerReadyMessage
            {
                playerId = GetPlayerId(NetworkManager.Singleton.LocalClientId)
            };

            SendToServer(readyMessage);
        }

        /// <summary>
        /// Load a scene for all connected players
        /// </summary>
        public void LoadSceneForAll(string sceneName)
        {
            if (!IsServer) return;

            Log.Net($"Loading scene '{sceneName}' for all players");

            if (_sceneController != null)
            {
                _sceneController.LoadSceneForAll(sceneName);
            }
            else
            {
                Log.Warn(LogCategory.Network, "NetworkSceneController not found");
            }
        }

        /// <summary>
        /// Check if both players are ready
        /// </summary>
        public bool AreBothPlayersReady()
        {
            return _hostReady.Value && _clientReady.Value;
        }

        #endregion

        #region Message Handlers

        private void HandlePlayerReadyMessage(string jsonMessage)
        {
            var message = NetworkMessageSerializer.Deserialize<PlayerReadyMessage>(jsonMessage);
            if (message == null) return;

            ulong sender = GetClientId(message.playerId);

            if (sender == NetworkManager.ServerClientId)
                _hostReady.Value = true;
            else
                _clientReady.Value = true;

            Log.Net($"Player ready | PlayerId={message.playerId} ClientId={sender}");

            if (AreBothPlayersReady())
            {
                Log.Net("Both players ready → starting game");
                
                // Create game start message
                var gameStartMessage = new GameStartMessage
                {
                    playerIds = new List<string> { "P1", "P2" },
                    totalTurns = 6
                };

                // SERVER-ONLY game start
                UIEventBus.Publish(new GameplayStartRequestedEvent());

                // Send game start message to all clients
                SendToAllClients(gameStartMessage);
            }
        }

        private void HandleGameStartMessage(string jsonMessage)
        {
            var message = NetworkMessageSerializer.Deserialize<GameStartMessage>(jsonMessage);
            if (message == null) return;

            Log.Net($"Game start message received | Players: {string.Join(", ", message.playerIds)} Turns: {message.totalTurns}");

            // CLIENT UI transition
            ShowGameplayClientRpc();
        }

        private void HandleConnectionStatusMessage(string jsonMessage)
        {
            // Handle connection status updates
            // This could include heartbeat, ping, etc.
        }

        #endregion

        #region RPC Methods

        [ClientRpc]
        private void ShowGameplayClientRpc()
        {
            UIEventBus.Publish(new GameplayPanelShownEvent());
            UIEventBus.Publish(new UIStateChangedEvent
            {
                State = UIState.Gameplay
            });
        }

        #endregion

        #region Server RPC Methods

        [ServerRpc(RequireOwnership = false)]
        private void SubmitReadyServerRpc(ServerRpcParams rpcParams = default)
        {
            ulong sender = rpcParams.Receive.SenderClientId;

            if (sender == NetworkManager.ServerClientId)
                _hostReady.Value = true;
            else
                _clientReady.Value = true;

            Log.Net($"Player ready | ClientId={sender}");

            if (_hostReady.Value && _clientReady.Value)
            {
                Log.Net("Both players ready → starting game");

                // SERVER-ONLY game start
                UIEventBus.Publish(new GameplayStartRequestedEvent());

                // CLIENT UI transition
                ShowGameplayClientRpc();
            }
        }

        #endregion

        #region Properties

        public bool HostReady => _hostReady.Value;
        public bool ClientReady => _clientReady.Value;

        #endregion
    }
}