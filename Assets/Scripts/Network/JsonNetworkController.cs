using Unity.Netcode;
using CardDuel.Utils;
using System;
using System.Collections.Generic;

namespace CardDuel.Networking
{
    /// <summary>
    /// Base class for network controllers that handle JSON messages
    /// </summary>
    public abstract class JsonNetworkController : NetworkBehaviour
    {
        // Message routing dictionary
        private Dictionary<string, Action<string>> _messageHandlers;

        protected virtual void Awake()
        {
            _messageHandlers = new Dictionary<string, Action<string>>();
            RegisterMessageHandlers();
        }

        /// <summary>
        /// Register all message handlers for this controller
        /// </summary>
        protected abstract void RegisterMessageHandlers();

        /// <summary>
        /// Register a handler for a specific message action
        /// </summary>
        protected void RegisterHandler(string action, Action<string> handler)
        {
            _messageHandlers[action] = handler;
        }

        /// <summary>
        /// Handle incoming JSON message
        /// </summary>
        protected void HandleMessage(string jsonMessage)
        {
            if (string.IsNullOrEmpty(jsonMessage))
            {
                Log.Warn(LogCategory.Network, "Received empty JSON message");
                return;
            }

            try
            {
                // Parse the base message to get the action
                var baseMessage = NetworkMessageSerializer.Deserialize(jsonMessage);
                if (baseMessage == null)
                {
                    Log.Warn(LogCategory.Network, $"Failed to parse message: {jsonMessage}");
                    return;
                }

                // Route to appropriate handler
                if (_messageHandlers.TryGetValue(baseMessage.action, out Action<string> handler))
                {
                    handler(jsonMessage);
                }
                else
                {
                    Log.Warn(LogCategory.Network, $"No handler registered for action: {baseMessage.action}");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Error handling message: {ex.Message}");
            }
        }

        /// <summary>
        /// Send JSON message to all clients
        /// </summary>
        protected void SendToAllClients(NetworkMessage message)
        {
            string json = NetworkMessageSerializer.Serialize(message);
            if (json != null)
            {
                SendJsonToAllClientsClientRpc(json);
            }
        }

        /// <summary>
        /// Send JSON message to specific client
        /// </summary>
        protected void SendToClient(ulong clientId, NetworkMessage message)
        {
            string json = NetworkMessageSerializer.Serialize(message);
            if (json != null)
            {
                var rpcParams = new ClientRpcParams
                {
                    Send = new ClientRpcSendParams
                    {
                        TargetClientIds = new ulong[] { clientId }
                    }
                };
                SendJsonToClientClientRpc(json, rpcParams);
            }
        }

        /// <summary>
        /// Send JSON message to server
        /// </summary>
        protected void SendToServer(NetworkMessage message)
        {
            string json = NetworkMessageSerializer.Serialize(message);
            if (json != null)
            {
                SendJsonToServerServerRpc(json);
            }
        }

        // RPC methods for message distribution
        [ClientRpc]
        private void SendJsonToAllClientsClientRpc(string jsonMessage)
        {
            HandleMessage(jsonMessage);
        }

        [ClientRpc]
        private void SendJsonToClientClientRpc(string jsonMessage, ClientRpcParams clientRpcParams = default)
        {
            HandleMessage(jsonMessage);
        }

        [ServerRpc(RequireOwnership = false)]
        private void SendJsonToServerServerRpc(string jsonMessage, ServerRpcParams serverRpcParams = default)
        {
            HandleMessage(jsonMessage);
        }

        /// <summary>
        /// Utility method to get player ID from client ID
        /// </summary>
        protected string GetPlayerId(ulong clientId)
        {
            return clientId == NetworkManager.ServerClientId ? "P1" : "P2";
        }

        /// <summary>
        /// Utility method to get client ID from player ID
        /// </summary>
        protected ulong GetClientId(string playerId)
        {
            return playerId == "P1" ? NetworkManager.ServerClientId : GetFirstNonServerClientId();
        }

        private ulong GetFirstNonServerClientId()
        {
            foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
            {
                if (clientId != NetworkManager.ServerClientId)
                {
                    return clientId;
                }
            }
            return NetworkManager.ServerClientId; // fallback
        }
    }
}