using System;

namespace CardDuel.Networking.Payloads
{
    [Serializable]
    public class PendingGameOverPayload
    {
        public string playerId;
        public float remainingTime;
    }
}
