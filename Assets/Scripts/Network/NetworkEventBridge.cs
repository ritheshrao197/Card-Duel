using UnityEngine;
using Unity.Netcode;
using CardDuel.UI.Core;
using CardDuel.UI.Events;
using CardDuel.Core;

namespace CardDuel.Networking
{
    public class NetworkEventBridge : MonoBehaviour
    {
        private void Awake()
        {
            UIEventBus.Subscribe<NetworkUIIntent.HostRequested>(_ => StartHost());
            UIEventBus.Subscribe<NetworkUIIntent.ClientRequested>(_ => StartClient());
            UIEventBus.Subscribe<NetworkUIIntent.DisconnectRequested>(_ => Disconnect());

            NetworkManager.Singleton.OnServerStarted += OnServerStarted;
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }

        private void StartHost()
        {
            NetworkManager.Singleton.StartHost();
        }

        private void StartClient()
        {
            NetworkManager.Singleton.StartClient();
        }

        private void Disconnect()
        {
            NetworkManager.Singleton.Shutdown();
        }

        private void OnServerStarted()
        {
            EventBus.Publish(new NetworkStartedEvent());
        }

        private void OnClientConnected(ulong clientId)
        {
            if (clientId == NetworkManager.Singleton.LocalClientId)
                EventBus.Publish(new ConnectedToServerEvent());
        }

        private void OnClientDisconnected(ulong clientId)
        {
            EventBus.Publish(new DisconnectedFromServerEvent
            {
                ClientId = clientId
            });
        }
    }
}
