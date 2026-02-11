using Unity.Netcode;
using CardDuel.UI.Events;
using CardDuel.Utils;

namespace CardDuel.Networking
{
    public class TurnPhaseNetworkController : JsonNetworkController
    {
        protected override void RegisterMessageHandlers()
        {
            RegisterHandler("turnStart", HandleTurnStartMessage);
        }

        private void HandleTurnStartMessage(string jsonMessage)
        {
            var message = NetworkMessageSerializer.Deserialize<TurnStartMessage>(jsonMessage);
            if (message == null) return;

            Log.Net($"TurnStart message | Turn={message.turn} Energy={message.energy}");

            UIEventBus.Publish(new TurnStartedEvent
            {
                Turn = message.turn,
                Energy = message.energy,
                MaxEnergy = message.maxEnergy
            });
        }

        // Legacy method for backward compatibility
        [ClientRpc]
        public void StartTurnClientRpc(int turn, int energy, int maxEnergy)
        {
            // Convert to JSON message
            var message = new TurnStartMessage
            {
                turn = turn,
                energy = energy,
                maxEnergy = maxEnergy
            };
            
            HandleTurnStartMessage(NetworkMessageSerializer.Serialize(message));
        }

        // New JSON-based method
        public void BroadcastTurnStart(int turn, int energy, int maxEnergy)
        {
            var message = new TurnStartMessage
            {
                turn = turn,
                energy = energy,
                maxEnergy = maxEnergy
            };

            SendToAllClients(message);
        }
    }
}
