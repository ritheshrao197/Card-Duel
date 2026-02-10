using Unity.Netcode;
using CardDuel.UI.Events;

namespace CardDuel.Networking
{
    public class TurnNetworkController : NetworkBehaviour
    {
        private NetworkVariable<int> currentTurn =
            new(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        // ✅ PUBLIC intent method
        public void RequestEndTurn()
        {
            if (!IsSpawned)
                return;

            SubmitEndTurnServerRpc();
        }

        [ServerRpc(RequireOwnership = false)]
        private void SubmitEndTurnServerRpc()
        {
            currentTurn.Value++;

            BroadcastTurnClientRpc(currentTurn.Value);
        }

        [ClientRpc]
        private void BroadcastTurnClientRpc(int turn)
        {
            UIEventBus.Publish(new TurnStartedEvent
            {
                Turn = turn,
                Energy = turn,
                MaxEnergy = 6
            });
        }
    }
}
