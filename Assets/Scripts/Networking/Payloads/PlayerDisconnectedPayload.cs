using System;

namespace CardDuel.Networking.Payloads
{
    [Serializable]
    public class PlayerDisconnectedPayload
    {
        public string playerId;
    }
}