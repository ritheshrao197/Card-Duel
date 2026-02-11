using System;
using System.Collections.Generic;
using UnityEngine;

namespace CardDuel.Networking
{
    /// <summary>
    /// Base class for all JSON network messages
    /// ALL network messages must be sent as JSON strings only
    /// Every message must include an "action" field
    /// </summary>
    [Serializable]
    public class NetworkMessage
    {
        public string action;
        
        public NetworkMessage(string action)
        {
            this.action = action;
        }
    }

    /// <summary>
    /// Game start message
    /// { "action": "gameStart", "playerIds": ["P1", "P2"], "totalTurns": 6 }
    /// </summary>
    [Serializable]
    public class GameStartMessage : NetworkMessage
    {
        public List<string> playerIds;
        public int totalTurns;
        public int startingHandSize;
        public int deckSize;

        public GameStartMessage() : base("gameStart")
        {
            playerIds = new List<string>();
            totalTurns = 6;
            startingHandSize = 3;
            deckSize = 12;
        }
    }

    /// <summary>
    /// Sync board message (folded card count)
    /// { "action": "syncBoard", "opponentCardCount": 2 }
    /// </summary>
    [Serializable]
    public class SyncBoardMessage : NetworkMessage
    {
        public int opponentCardCount;
        public List<int> cardIds; // Optional: actual card IDs if needed

        public SyncBoardMessage() : base("syncBoard") 
        {
            cardIds = new List<int>();
        }
    }

    /// <summary>
    /// Reveal single card message
    /// { "action": "revealSingleCard", "playerId": "P1", "cardId": 5, "orderIndex": 0 }
    /// </summary>
    [Serializable]
    public class RevealSingleCardMessage : NetworkMessage
    {
        public string playerId;
        public int cardId;
        public int orderIndex;
        public int turnNumber;

        public RevealSingleCardMessage() : base("revealSingleCard") { }
    }

    /// <summary>
    /// End turn message
    /// { "action": "endTurn", "playerId": "P1" }
    /// </summary>
    [Serializable]
    public class EndTurnMessage : NetworkMessage
    {
        public string playerId;
        public int turnNumber;
        public List<int> foldedCardIds;

        public EndTurnMessage() : base("endTurn") 
        {
            foldedCardIds = new List<int>();
        }
    }

    /// <summary>
    /// Draw card message
    /// { "action": "drawCard", "playerId": "P1", "cardId": 5 }
    /// </summary>
    [Serializable]
    public class DrawCardMessage : NetworkMessage
    {
        public string playerId;
        public int cardId;
        public int turnNumber;

        public DrawCardMessage() : base("drawCard") { }
    }

    /// <summary>
    /// Play card message (for staging)
    /// { "action": "playCard", "playerId": "P1", "cardId": 5, "slotIndex": 0 }
    /// </summary>
    [Serializable]
    public class PlayCardMessage : NetworkMessage
    {
        public string playerId;
        public int cardId;
        public int slotIndex;
        public int turnNumber;

        public PlayCardMessage() : base("playCard") { }
    }

    /// <summary>
    /// Turn start message
    /// { "action": "turnStart", "turn": 1, "energy": 3, "maxEnergy": 6 }
    /// </summary>
    [Serializable]
    public class TurnStartMessage : NetworkMessage
    {
        public int turn;
        public int energy;
        public int maxEnergy;
        public Dictionary<string, int> playerScores;

        public TurnStartMessage() : base("turnStart") 
        {
            playerScores = new Dictionary<string, int>();
        }
    }

    /// <summary>
    /// Player ready message
    /// { "action": "playerReady", "playerId": "P1" }
    /// </summary>
    [Serializable]
    public class PlayerReadyMessage : NetworkMessage
    {
        public string playerId;

        public PlayerReadyMessage() : base("playerReady") { }
    }

    /// <summary>
    /// Score updated message
    /// { "action": "scoreUpdated", "playerId": "P1", "delta": 5, "newScore": 15 }
    /// </summary>
    [Serializable]
    public class ScoreUpdatedMessage : NetworkMessage
    {
        public string playerId;
        public int delta;
        public int newScore;
        public int opponentScore;

        public ScoreUpdatedMessage() : base("scoreUpdated") { }
    }

    /// <summary>
    /// Reveal sequence started message
    /// { "action": "revealSequenceStarted", "initiativePlayer": "P1", "cardCounts": {"P1": 2, "P2": 3} }
    /// </summary>
    [Serializable]
    public class RevealSequenceStartedMessage : NetworkMessage
    {
        public string initiativePlayer;
        public Dictionary<string, int> cardCounts;

        public RevealSequenceStartedMessage() : base("revealSequenceStarted") 
        {
            cardCounts = new Dictionary<string, int>();
        }
    }

    /// <summary>
    /// Match completed message
    /// { "action": "matchCompleted", "winnerId": "P1", "playerScores": {"P1": 25, "P2": 18} }
    /// </summary>
    [Serializable]
    public class MatchCompletedMessage : NetworkMessage
    {
        public string winnerId;
        public Dictionary<string, int> playerScores;

        public MatchCompletedMessage() : base("matchCompleted") 
        {
            playerScores = new Dictionary<string, int>();
        }
    }

    /// <summary>
    /// Turn timeout message
    /// { "action": "turnTimeout", "playerId": "P1", "turnNumber": 3 }
    /// </summary>
    [Serializable]
    public class TurnTimeoutMessage : NetworkMessage
    {
        public string playerId;
        public int turnNumber;

        public TurnTimeoutMessage() : base("turnTimeout") { }
    }

    /// <summary>
    /// Utility class for JSON serialization/deserialization of network messages
    /// Version 2.0 - Enhanced JSON-only networking implementation
    /// </summary>
    public static class NetworkMessageSerializer
    {
        /// <summary>
        /// Serialize a network message to JSON string
        /// </summary>
        public static string Serialize(NetworkMessage message)
        {
            try
            {
                return JsonUtility.ToJson(message);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to serialize message: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Deserialize a JSON string to a network message
        /// </summary>
        public static T Deserialize<T>(string json) where T : NetworkMessage
        {
            try
            {
                return JsonUtility.FromJson<T>(json);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to deserialize message: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Deserialize a JSON string to NetworkMessage base class
        /// </summary>
        public static NetworkMessage Deserialize(string json)
        {
            try
            {
                return JsonUtility.FromJson<NetworkMessage>(json);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to deserialize base message: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Get message type from action field
        /// </summary>
        public static Type GetMessageType(string action)
        {
            return action switch
            {
                "gameStart" => typeof(GameStartMessage),
                "syncBoard" => typeof(SyncBoardMessage),
                "revealSingleCard" => typeof(RevealSingleCardMessage),
                "endTurn" => typeof(EndTurnMessage),
                "drawCard" => typeof(DrawCardMessage),
                "playCard" => typeof(PlayCardMessage),
                "turnStart" => typeof(TurnStartMessage),
                "playerReady" => typeof(PlayerReadyMessage),
                "scoreUpdated" => typeof(ScoreUpdatedMessage),
                "revealSequenceStarted" => typeof(RevealSequenceStartedMessage),
                "matchCompleted" => typeof(MatchCompletedMessage),
                "turnTimeout" => typeof(TurnTimeoutMessage),
                _ => typeof(NetworkMessage)
            };
        }
        
        /// <summary>
        /// Create a message instance from action type
        /// </summary>
        public static NetworkMessage CreateMessage(string action)
        {
            return action switch
            {
                "gameStart" => new GameStartMessage(),
                "syncBoard" => new SyncBoardMessage(),
                "revealSingleCard" => new RevealSingleCardMessage(),
                "endTurn" => new EndTurnMessage(),
                "drawCard" => new DrawCardMessage(),
                "playCard" => new PlayCardMessage(),
                "turnStart" => new TurnStartMessage(),
                "playerReady" => new PlayerReadyMessage(),
                "scoreUpdated" => new ScoreUpdatedMessage(),
                "revealSequenceStarted" => new RevealSequenceStartedMessage(),
                "matchCompleted" => new MatchCompletedMessage(),
                "turnTimeout" => new TurnTimeoutMessage(),
                _ => new NetworkMessage(action)
            };
        }
    }
}