using System;

namespace CardDuel.Networking.Payloads
{
    [Serializable]
    public class RevealCardPayload
    {
        public string playerId;
        public int cardId;
        public int orderIndex;
    }
}