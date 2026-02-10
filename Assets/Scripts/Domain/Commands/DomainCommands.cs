namespace CardDuel.Domain.Commands
{
    /// <summary>
    /// Base interface for all commands
    /// Commands are intent, not behavior
    /// </summary>
    public interface ICommand
    {
        ulong PlayerId { get; }
    }

    /// <summary>
    /// Player wants to play a card
    /// </summary>
    public class PlayCardCommand : ICommand
    {
        public ulong PlayerId { get; }
        public int CardId { get; }

        public PlayCardCommand(ulong playerId, int cardId)
        {
            PlayerId = playerId;
            CardId = cardId;
        }
    }

    /// <summary>
    /// Player wants to end their turn
    /// </summary>
    public class EndTurnCommand : ICommand
    {
        public ulong PlayerId { get; }

        public EndTurnCommand(ulong playerId)
        {
            PlayerId = playerId;
        }
    }

    /// <summary>
    /// Player wants to concede the match
    /// </summary>
    public class ConcedeCommand : ICommand
    {
        public ulong PlayerId { get; }

        public ConcedeCommand(ulong playerId)
        {
            PlayerId = playerId;
        }
    }

    /// <summary>
    /// Start a new match
    /// </summary>
    public class StartMatchCommand : ICommand
    {
        public ulong PlayerId { get; }
        public int MaxTurns { get; }

        public StartMatchCommand(ulong playerId, int maxTurns = 6)
        {
            PlayerId = playerId;
            MaxTurns = maxTurns;
        }
    }

    /// <summary>
    /// Draw a card (if allowed by game rules)
    /// </summary>
    public class DrawCardCommand : ICommand
    {
        public ulong PlayerId { get; }

        public DrawCardCommand(ulong playerId)
        {
            PlayerId = playerId;
        }
    }
}