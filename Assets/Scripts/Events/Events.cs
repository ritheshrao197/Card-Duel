using CardDuel.Domain.Commands;
using CardDuel.Domain.Events;

namespace CardDuel.Gameplay.Events
{
    [System.Serializable]
    public struct TurnStartedEvent : IGameEvent
    {
        public ulong PlayerId;
    }

    [System.Serializable]
    public struct TurnEndedEvent : IGameEvent
    {
        public ulong PlayerId;
    }

    [System.Serializable]
    public struct EndTurnCommand : ICommand
    {
        public ulong PlayerId;
        ulong ICommand.PlayerId => PlayerId;
    }
    [System.Serializable]
    public struct DeckCountUpdatedEvent : IGameEvent
    {
        public int RemainingCards;
    }
    [System.Serializable]
    public struct ScoreUpdatedEvent : IGameEvent
    {
        public int LocalScore;
        public int OpponentScore;
    }
}
