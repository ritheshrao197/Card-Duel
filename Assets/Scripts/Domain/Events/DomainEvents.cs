using CardDuel.Core;

namespace CardDuel.Domain.Events
{
    /// <summary>
    /// Game started event
    /// </summary>
    public class GameStartedEvent : IEvent
    {
        public int TotalTurns { get; }

        public GameStartedEvent(int totalTurns)
        {
            TotalTurns = totalTurns;
        }
    }

    /// <summary>
    /// Turn started event
    /// </summary>
    public class TurnStartedEvent : IEvent
    {
        public int TurnNumber { get; }

        public TurnStartedEvent(int turnNumber)
        {
            TurnNumber = turnNumber;
        }
    }

    /// <summary>
    /// Player ended their turn event
    /// </summary>
    public class PlayerEndedTurnEvent : IEvent
    {
        public ulong PlayerId { get; }

        public PlayerEndedTurnEvent(ulong playerId)
        {
            PlayerId = playerId;
        }
    }

    /// <summary>
    /// Initiative determined event
    /// </summary>
    public class InitiativeDeterminedEvent : IEvent
    {
        public ulong PlayerId { get; }

        public InitiativeDeterminedEvent(ulong playerId)
        {
            PlayerId = playerId;
        }
    }

    /// <summary>
    /// Card revealed event
    /// </summary>
    public class CardRevealedEvent : IEvent
    {
        public ulong PlayerId { get; }
        public int CardId { get; }
        public int OrderIndex { get; }

        public CardRevealedEvent(ulong playerId, int cardId, int orderIndex)
        {
            PlayerId = playerId;
            CardId = cardId;
            OrderIndex = orderIndex;
        }
    }

    /// <summary>
    /// Reveal phase completed event
    /// </summary>
    public class RevealPhaseCompletedEvent : IEvent
    {
    }

    /// <summary>
    /// Card played event
    /// </summary>
    public class CardPlayedEvent : IEvent
    {
        public ulong PlayerId { get; }
        public int CardId { get; }

        public CardPlayedEvent(ulong playerId, int cardId)
        {
            PlayerId = playerId;
            CardId = cardId;
        }
    }

    /// <summary>
    /// Score updated event
    /// </summary>
    public class ScoreUpdatedEvent : IEvent
    {
        public ulong PlayerId { get; }
        public int NewScore { get; }

        public ScoreUpdatedEvent(ulong playerId, int newScore)
        {
            PlayerId = playerId;
            NewScore = newScore;
        }
    }

    /// <summary>
    /// Turn ended event
    /// </summary>
    public class TurnEndedEvent : IEvent
    {
        public int TurnNumber { get; }

        public TurnEndedEvent(int turnNumber)
        {
            TurnNumber = turnNumber;
        }
    }

    /// <summary>
    /// Game ended event
    /// </summary>
    public class GameEndedEvent : IEvent
    {
        public ulong WinningPlayerId { get; }
        public int WinningScore { get; }

        public GameEndedEvent(ulong winningPlayerId, int winningScore)
        {
            WinningPlayerId = winningPlayerId;
            WinningScore = winningScore;
        }
    }
}