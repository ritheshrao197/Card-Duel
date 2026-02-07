using Unity.Netcode;
using CardDuel.Utils;

namespace CardDuel.Networking.Netcode
{
    public class NetworkPlayer : NetworkBehaviour
    {
        public override void OnNetworkSpawn()
        {
            Logger.Log(
                LogCategory.Network,
                $"NetworkPlayer spawned. OwnerClientId={OwnerClientId}"
            );

            if (!IsOwner) return;
            NetworkClient.Initialize(this);
        }

        [ServerRpc]
        public void SendMessageServerRpc(string json)
        {
            NetworkGameController.Instance
                .OnMessageFromClient(OwnerClientId, json);
        }
    }
}