
using Unity.Netcode;
using CardDuel.Domain.Commands;
using CardDuel.Gameplay.Commands;
using CardDuel.Networking.Serialization;

namespace CardDuel.Networking
{
    public class CommandSender : NetworkBehaviour
    {
        [ServerRpc(RequireOwnership = false)]
        public void SendCommandServerRpc(string json, string type)
        {
            var command = CommandSerializer.Deserialize(json, type);
            if (command == null) return;

            NetworkContext.MatchService.Handle(command);
        }

        // Convenience helpers
        public void Send(ICommand command)
        {
            var json = CommandSerializer.Serialize(command);
            SendCommandServerRpc(json, command.GetType().Name);
        }
    }
}