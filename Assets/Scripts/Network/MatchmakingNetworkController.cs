using Unity.Netcode;
using CardDuel.UI.Events;
using CardDuel.Utils;

namespace CardDuel.Networking
{
    public class MatchmakingNetworkController : NetworkBehaviour
    {
        private NetworkVariable<bool> hostReady = new();
        private NetworkVariable<bool> clientReady = new();

        // 🔑 PUBLIC INTENT METHOD (what UseCases call)
        public void SubmitReady()
        {
            if (!IsSpawned)
            {
                Log.Net("SubmitReady ignored (not spawned)");
                return;
            }

            Log.Net("SubmitReady called");
            SubmitReadyServerRpc();
        }

        // 🔑 SERVER AUTHORITY
        [ServerRpc(RequireOwnership = false)]
        private void SubmitReadyServerRpc(ServerRpcParams rpcParams = default)
        {
            ulong sender = rpcParams.Receive.SenderClientId;

            if (sender == NetworkManager.ServerClientId)
                hostReady.Value = true;
            else
                clientReady.Value = true;

            Log.Net($"Player ready | ClientId={sender}");

            if (hostReady.Value && clientReady.Value)
            {
                Log.Net("Both players ready → starting game");

                // SERVER-ONLY game start
                UIEventBus.Publish(new GameplayStartRequestedEvent());

                // CLIENT UI transition
                ShowGameplayClientRpc();
            }
        }

        [ClientRpc]
        private void ShowGameplayClientRpc()
        {
            UIEventBus.Publish(new GameplayPanelShownEvent());
            UIEventBus.Publish(new UIStateChangedEvent
            {
                State = UIState.Gameplay
            });
        }
    }
}
