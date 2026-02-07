using UnityEngine;
using Unity.Netcode;
using CardDuel.Utils;

namespace CardDuel.Networking.Netcode
{
    public class NetworkBootstrap : MonoBehaviour
    {
        public void StartHost()
        {
            Utils.Logger.Log(LogCategory.Network, "Starting Host");
            NetworkManager.Singleton.StartHost();
        }

        public void StartClient()
        {
            Utils.Logger.Log(LogCategory.Network, "Starting Client");
            NetworkManager.Singleton.StartClient();
        }
    }
}
