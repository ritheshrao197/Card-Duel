using System;

namespace CardDuel.Networking.Payloads
{
    [Serializable]
    public class PlayerConnectionStatePayload
    {
        public int connectedPlayers;
    }
}