using Unity.Netcode;
using UnityEngine;

namespace CardDuel.Networking
{
    public class ConnectionWatcher : MonoBehaviour
    {
        private void OnEnable()
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }

        private void OnDisable()
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }

        private void OnClientConnected(ulong clientId)
        {
            SessionRegistry.Register(clientId);
        }

        private void OnClientDisconnected(ulong clientId)
        {
            SessionRegistry.MarkDisconnected(clientId);
        }
    }
}
