using UnityEngine;
using CardDuel.Networking;
using CardDuel.Utils;

namespace CardDuel.Testing
{
    /// <summary>
    /// Test script to validate JSON network message functionality
    /// </summary>
    public class NetworkMessageTest : MonoBehaviour
    {
        [Header("Test Configuration")]
        [SerializeField] private bool runTestsOnStart = true;
        [SerializeField] private float testDelay = 1f;

        private void Start()
        {
            if (runTestsOnStart)
            {
                Invoke(nameof(RunAllTests), testDelay);
            }
        }

        private void RunAllTests()
        {
            Log.App("Starting JSON Network Message Tests");
            
            TestMessageSerialization();
            TestMessageDeserialization();
            TestGameStartMessage();
            TestDrawCardMessage();
            TestPlayCardMessage();
            TestEndTurnMessage();
            
            Log.App("JSON Network Message Tests Completed");
        }

        private void TestMessageSerialization()
        {
            Log.App("Testing message serialization...");
            
            // Test GameStartMessage
            var gameStartMsg = new GameStartMessage
            {
                playerIds = new System.Collections.Generic.List<string> { "P1", "P2" },
                totalTurns = 6
            };
            
            string json = NetworkMessageSerializer.Serialize(gameStartMsg);
            Log.App($"Serialized GameStartMessage: {json}");
            
            // Test basic network message
            var baseMsg = new NetworkMessage("testAction");
            string baseJson = NetworkMessageSerializer.Serialize(baseMsg);
            Log.App($"Serialized base message: {baseJson}");
        }

        private void TestMessageDeserialization()
        {
            Log.App("Testing message deserialization...");
            
            // Test deserializing game start message
            string json = "{\"action\":\"gameStart\",\"playerIds\":[\"P1\",\"P2\"],\"totalTurns\":6}";
            var msg = NetworkMessageSerializer.Deserialize<GameStartMessage>(json);
            
            if (msg != null)
            {
                Log.App($"Deserialized GameStartMessage: Action={msg.action}, Players={string.Join(", ", msg.playerIds)}, Turns={msg.totalTurns}");
            }
            else
            {
                Log.Error("Failed to deserialize GameStartMessage");
            }
            
            // Test base message deserialization
            string baseJson = "{\"action\":\"testAction\"}";
            var baseMsg = NetworkMessageSerializer.Deserialize(baseJson);
            
            if (baseMsg != null)
            {
                Log.App($"Deserialized base message: Action={baseMsg.action}");
            }
            else
            {
                Log.Error("Failed to deserialize base message");
            }
        }

        private void TestGameStartMessage()
        {
            Log.App("Testing GameStartMessage...");
            
            var msg = new GameStartMessage
            {
                playerIds = new System.Collections.Generic.List<string> { "P1", "P2", "P3" },
                totalTurns = 8
            };
            
            string json = NetworkMessageSerializer.Serialize(msg);
            Log.App($"GameStart JSON: {json}");
            
            var deserialized = NetworkMessageSerializer.Deserialize<GameStartMessage>(json);
            if (deserialized != null)
            {
                Log.App($"✓ GameStartMessage test passed");
            }
            else
            {
                Log.Error("✗ GameStartMessage test failed");
            }
        }

        private void TestDrawCardMessage()
        {
            Log.App("Testing DrawCardMessage...");
            
            var msg = new DrawCardMessage
            {
                playerId = "P1",
                cardId = 42
            };
            
            string json = NetworkMessageSerializer.Serialize(msg);
            Log.App($"DrawCard JSON: {json}");
            
            var deserialized = NetworkMessageSerializer.Deserialize<DrawCardMessage>(json);
            if (deserialized != null && deserialized.playerId == "P1" && deserialized.cardId == 42)
            {
                Log.App($"✓ DrawCardMessage test passed");
            }
            else
            {
                Log.Error("✗ DrawCardMessage test failed");
            }
        }

        private void TestPlayCardMessage()
        {
            Log.App("Testing PlayCardMessage...");
            
            var msg = new PlayCardMessage
            {
                playerId = "P2",
                cardId = 15,
                slotIndex = 2
            };
            
            string json = NetworkMessageSerializer.Serialize(msg);
            Log.App($"PlayCard JSON: {json}");
            
            var deserialized = NetworkMessageSerializer.Deserialize<PlayCardMessage>(json);
            if (deserialized != null && deserialized.playerId == "P2" && deserialized.cardId == 15 && deserialized.slotIndex == 2)
            {
                Log.App($"✓ PlayCardMessage test passed");
            }
            else
            {
                Log.Error("✗ PlayCardMessage test failed");
            }
        }

        private void TestEndTurnMessage()
        {
            Log.App("Testing EndTurnMessage...");
            
            var msg = new EndTurnMessage
            {
                playerId = "P1"
            };
            
            string json = NetworkMessageSerializer.Serialize(msg);
            Log.App($"EndTurn JSON: {json}");
            
            var deserialized = NetworkMessageSerializer.Deserialize<EndTurnMessage>(json);
            if (deserialized != null && deserialized.playerId == "P1")
            {
                Log.App($"✓ EndTurnMessage test passed");
            }
            else
            {
                Log.Error("✗ EndTurnMessage test failed");
            }
        }

        [ContextMenu("Run Tests")]
        public void RunTestsManual()
        {
            RunAllTests();
        }

        [ContextMenu("Test Serialization")]
        public void TestSerializationOnly()
        {
            TestMessageSerialization();
        }

        [ContextMenu("Test Deserialization")]
        public void TestDeserializationOnly()
        {
            TestMessageDeserialization();
        }
    }
}