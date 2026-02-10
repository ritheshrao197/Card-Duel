using CardDuel.Domain.Events;

namespace CardDuel.Gameplay.Events
{
    [System.Serializable]
    public struct MatchEndedEvent : IGameEvent
    {
        public ulong WinnerPlayerId;
    }
}
