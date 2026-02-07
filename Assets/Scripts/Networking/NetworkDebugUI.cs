using UnityEngine;

namespace CardDuel.Networking.Netcode
{
    public class NetworkDebugUI : MonoBehaviour
    {
        public void StartHost()
        {
            NetworkBootstrap bootstrap =
                FindObjectOfType<NetworkBootstrap>();
            bootstrap.StartHost();
        }

        public void StartClient()
        {
            NetworkBootstrap bootstrap =
                FindObjectOfType<NetworkBootstrap>();
            bootstrap.StartClient();
        }
    }
}
