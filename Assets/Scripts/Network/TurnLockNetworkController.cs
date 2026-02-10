using Unity.Netcode;
using CardDuel.UI.Events;
using CardDuel.Utils;

namespace CardDuel.Networking
{
    public class TurnLockNetworkController : NetworkBehaviour
    {
        private NetworkVariable<bool> hostEnded =
            new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private NetworkVariable<bool> clientEnded =
            new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        public void RequestEndTurn()
        {
            if (!IsSpawned) return;
            SubmitEndTurnServerRpc();
        }

        [ServerRpc(RequireOwnership = false)]
        private void SubmitEndTurnServerRpc(ServerRpcParams rpcParams = default)
        {
            ulong sender = rpcParams.Receive.SenderClientId;

            Log.Net($"EndTurn received from {sender}");

            if (sender == NetworkManager.ServerClientId)
                hostEnded.Value = true;
            else
                clientEnded.Value = true;

            NotifyWaitingClientRpc();

            if (hostEnded.Value && clientEnded.Value)
            {
                Log.Net("Both players ended turn");
                StartRevealClientRpc();
                ResetForNextTurn();
            }
        }

        [ClientRpc]
        private void NotifyWaitingClientRpc()
        {
            UIEventBus.Publish(new WaitingForOpponentEvent { IsWaiting = true });
            UIEventBus.Publish(new LocalTurnLockedEvent());
        }

        [ClientRpc]
        private void StartRevealClientRpc()
        {
            UIEventBus.Publish(new WaitingForOpponentEvent { IsWaiting = false });
            UIEventBus.Publish(new RevealPhaseStartedEvent());
        }

        private void ResetForNextTurn()
        {
            hostEnded.Value = false;
            clientEnded.Value = false;
        }
    }
}
