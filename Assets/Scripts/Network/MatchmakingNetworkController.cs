using Unity.Netcode;
using CardDuel.UI.Events;

namespace CardDuel.Networking
{
    public class MatchmakingNetworkController : NetworkBehaviour
    {
        private NetworkVariable<bool> hostReady =
            new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private NetworkVariable<bool> clientReady =
            new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        public override void OnNetworkSpawn()
        {
            // no UI subscriptions here anymore
        }

        // ✅ PUBLIC intent method (called by Application layer)
        public void SubmitReady()
        {
            if (!IsSpawned)
                return;

            SubmitReadyServerRpc();
        }

        [ServerRpc(RequireOwnership = false)]
        private void SubmitReadyServerRpc(ServerRpcParams rpcParams = default)
        {
            if (rpcParams.Receive.SenderClientId == NetworkManager.ServerClientId)
                hostReady.Value = true;
            else
                clientReady.Value = true;

            if (hostReady.Value && clientReady.Value)
            {
                StartGameClientRpc();
            }
        }

        [ClientRpc]
        private void StartGameClientRpc()
        {
            UIEventBus.Publish(new BothPlayersReadyEvent());
        }
    }
}
