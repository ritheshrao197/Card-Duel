using UnityEngine;
using Unity.Netcode;
using CardDuel.UI.Events;

namespace CardDuel.Networking
{
    public class NetcodeBootstrapper : MonoBehaviour
    {
        public void StartConnection(NetworkRole role)
        {
            if (NetworkManager.Singleton == null)
            {
                Debug.LogError("NetworkManager not found");
                return;
            }

            if (role == NetworkRole.Host)
                NetworkManager.Singleton.StartHost();
            else
                NetworkManager.Singleton.StartClient();
        }
    }
}
