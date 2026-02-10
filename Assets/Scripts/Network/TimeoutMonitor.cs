using Unity.Netcode;
using UnityEngine;
using CardDuel.UI.Events;

namespace CardDuel.Networking
{
    public class TimeoutMonitor : MonoBehaviour
    {
        private void Awake()
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }

        private void OnClientDisconnected(ulong clientId)
        {
            if (!NetworkManager.Singleton.IsServer)
                return;

            if (clientId != NetworkManager.ServerClientId)
            {
                UIEventBus.Publish(new OpponentTimedOutEvent());
            }
        }
    }
}
