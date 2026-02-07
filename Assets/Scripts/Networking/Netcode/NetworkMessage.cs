using System;

namespace CardDuel.Networking
{
    [Serializable]
    public class NetworkMessage
    {
        public string action;
        public string payload; // JSON string
    }
}