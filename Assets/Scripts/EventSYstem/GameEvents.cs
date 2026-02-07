using CardDuel.Cards;
using CardDuel.Networking.Sync;

namespace CardDuel.Events
{
    public struct GameStartEvent
    {
        public int totalTurns;
    }

    public struct TurnStartEvent
    {
        public int turnNumber;
        public int availableCost;
    }

    public struct PlayerEndedTurnEvent
    {
        public string playerId;
    }
    
    public struct PlayerConnectedEvent
    {
        public int playerCount;
    }
    public struct PlayerDisconnectedEvent
    {
        public string playerId;
    }
    public struct ReconnectCountdownEvent
    {
        public string playerId;
        public float remainingTime;
    }
    public struct SpectatorModeEvent { }

    public struct CardRevealedEvent
    {
        public string playerId;
        public CardInstance card;
    }

    public struct ScoreUpdatedEvent
    {
        public int playerScore;
        public int opponentScore;
    }
    public struct FullStateSyncEvent
    {
        public GameStateSnapshot snapshot;
    }
    public struct GameEndEvent
    {
        public string winnerId;
    }
    public struct PendingGameOverEvent
    {
        public string playerId;
        public float remainingTime;
    }
      
    public struct MatchResumedEvent { }
    // UI → Gameplay intent
    public struct EndTurnPressedEvent { }
}
