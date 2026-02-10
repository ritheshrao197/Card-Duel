using Unity.Netcode;
using CardDuel.UI.Events;
using CardDuel.Utils;

namespace CardDuel.Networking
{
    public class TurnPhaseNetworkController : NetworkBehaviour
    {
        [ClientRpc]
        public void StartTurnClientRpc(int turn, int energy, int maxEnergy)
        {
            Log.Net($"StartTurnClientRpc | Turn={turn} Energy={energy}");

            UIEventBus.Publish(new TurnStartedEvent
            {
                Turn = turn,
                Energy = energy,
                MaxEnergy = maxEnergy
            });
        }
    }
}
