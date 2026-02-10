
using CardDuel.Domain.Events;

namespace CardDuel.Gameplay.Events
{
    [System.Serializable]
    public struct CardPlayedEvent : IGameEvent
    {
        public ulong PlayerId;
        public int CardId;
    }
}