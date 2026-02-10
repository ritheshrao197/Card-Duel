

using System.Collections.Generic;

namespace CardDuel.Networking
{
    public static class SessionRegistry
    {
        private static readonly Dictionary<ulong, PlayerSessionState> Sessions = new();

        public static void Register(ulong clientId)
        {
            if (!Sessions.ContainsKey(clientId))
                Sessions[clientId] = new PlayerSessionState { IsConnected = true };
        }

        public static void MarkDisconnected(ulong clientId)
        {
            if (Sessions.TryGetValue(clientId, out var state))
                state.IsConnected = false;
        }

        public static PlayerSessionState Get(ulong clientId)
        {
            return Sessions.TryGetValue(clientId, out var state) ? state : null;
        }
    }
}