using System;
using Unity.Netcode;
using CardDuel.Domain.Commands;
using CardDuel.Core;
using UnityEngine;

namespace CardDuel.Infrastructure.Networking
{
    /// <summary>
    /// Server-authoritative network service
    /// Networking is transport, not logic
    /// </summary>
    public class NetworkService : NetworkBehaviour
    {
        public static NetworkService Instance { get; private set; }

        // Network variables for minimal state sync
        public NetworkVariable<int> CurrentTurn = new NetworkVariable<int>(1);
        public NetworkVariable<int> MaxTurns = new NetworkVariable<int>(6);
        public NetworkVariable<ulong> Player1Id = new NetworkVariable<ulong>(0);
        public NetworkVariable<ulong> Player2Id = new NetworkVariable<ulong>(0);

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            Instance = this;

            if (IsServer)
            {
                SetupServer();
            }
            else
            {
                SetupClient();
            }
        }

        private void SetupServer()
        {
            // Server listens for client commands
            EventBus.Subscribe<ClientCommandReceived>(OnClientCommand);
        }

        private void SetupClient()
        {
            // Clients listen for server events
            EventBus.Subscribe<ServerEventBroadcast>(OnServerEvent);
        }

        /// <summary>
        /// Client sends command to server
        /// </summary>
        public void SendCommandToServer(ICommand command)
        {
            if (IsServer)
            {
                // Direct processing on server
                ProcessCommand(command);
            }
            else
            {
                // Send to server
                var commandEvent = new ClientCommandReceived(command);
                EventBus.Publish(commandEvent);
                
                // Also send via network RPC
                SendCommandServerRpc(JsonUtility.ToJson(command));
            }
        }

        /// <summary>
        /// Server broadcasts events to all clients
        /// </summary>
        public void BroadcastEventToClients(IEvent evt)
        {
            var broadcastEvent = new ServerEventBroadcast(evt);
            EventBus.Publish(broadcastEvent);
            
            // Send via network
            BroadcastEventServerRpc(JsonUtility.ToJson(evt));
        }

        [ServerRpc(RequireOwnership = false)]
        private void SendCommandServerRpc(string commandJson)
        {
            try
            {
                // Deserialize and process command
                // In a real implementation, you'd have proper deserialization
                Debug.Log($"Server received command: {commandJson}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to process command: {ex.Message}");
            }
        }

        [ClientRpc]
        private void BroadcastEventClientRpc(string eventJson)
        {
            try
            {
                // Deserialize and publish event
                // In a real implementation, you'd have proper deserialization
                Debug.Log($"Client received event: {eventJson}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to process event: {ex.Message}");
            }
        }

        [ServerRpc]
        private void BroadcastEventServerRpc(string eventJson)
        {
            BroadcastEventClientRpc(eventJson);
        }

        private void OnClientCommand(ClientCommandReceived evt)
        {
            ProcessCommand(evt.Command);
        }

        private void OnServerEvent(ServerEventBroadcast evt)
        {
            // Client received event from server
            EventBus.Publish(evt.Event);
        }

        private void ProcessCommand(ICommand command)
        {
            // This would integrate with the application layer
            // For now, just log
            Debug.Log($"Processing command: {command.GetType().Name} from player {command.PlayerId}");
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            if (Instance == this)
                Instance = null;
        }
    }

    /// <summary>
    /// Event for client sending command to server
    /// </summary>
    public class ClientCommandReceived : IEvent
    {
        public ICommand Command { get; }

        public ClientCommandReceived(ICommand command)
        {
            Command = command;
        }
    }

    /// <summary>
    /// Event for server broadcasting to clients
    /// </summary>
    public class ServerEventBroadcast : IEvent
    {
        public IEvent Event { get; }

        public ServerEventBroadcast(IEvent evt)
        {
            Event = evt;
        }
    }
}