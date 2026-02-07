using System;

namespace CardDuel.Networking.Payloads
{
    [Serializable]
    public class LeaveMatchPayload
    {
        public string reason = "manual";
    }
}
