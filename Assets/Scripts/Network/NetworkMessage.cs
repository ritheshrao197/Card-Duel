using System;
using System.Collections.Generic;
using UnityEngine;

namespace CardDuel.Networking
{
    /// <summary>
    /// Base class for all JSON network messages
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

        public GameStartMessage() : base("gameStart")
        {
            playerIds = new List<string>();
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

        public SyncBoardMessage() : base("syncBoard") { }
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

        public EndTurnMessage() : base("endTurn") { }
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

        public DrawCardMessage() : base("drawCard") { }
    }

    /// <summary>
    /// Play card message
    /// { "action": "playCard", "playerId": "P1", "cardId": 5, "slotIndex": 0 }
    /// </summary>
    [Serializable]
    public class PlayCardMessage : NetworkMessage
    {
        public string playerId;
        public int cardId;
        public int slotIndex;

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

        public TurnStartMessage() : base("turnStart") { }
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
    /// Utility class for JSON serialization/deserialization of network messages
    /// Version 1.0 - JSON-only networking implementation
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
                _ => typeof(NetworkMessage)
            };
        }
    }
}