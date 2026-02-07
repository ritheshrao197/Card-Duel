using System;

namespace CardDuel.Networking.Payloads
{
    [Serializable]
    public class ReconnectCountdownPayload
    {
        public string playerId;
        public float remainingTime;
    }
}
