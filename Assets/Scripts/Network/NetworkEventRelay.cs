using CardDuel.Core;
using CardDuel.Domain.Events;
using CardDuel.Networking.Serialization;
using Unity.Netcode;

namespace CardDuel.Networking
{
    public class NetworkEventRelay : NetworkBehaviour
    {
        public void Broadcast(IGameEvent evt)
        {
            var json = EventSerializer.Serialize(evt);
            PublishEventClientRpc(json, evt.GetType().Name);
        }

        [ClientRpc]
        private void PublishEventClientRpc(string json, string type)
        {
            var evt = EventSerializer.Deserialize(json, type);
            if (evt != null)
                EventBus.Publish(evt);
        }
    }
}