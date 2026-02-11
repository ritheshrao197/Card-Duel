using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Runtime validation script to check game systems are working correctly
/// Attach this to a GameObject in your main scene to run validation on startup
/// </summary>
public class GameValidator : MonoBehaviour
{
    [Header("Validation Settings")]
    [SerializeField] private bool runValidationOnStart = true;
    [SerializeField] private bool showDetailedLogs = true;
    
    private List<string> validationResults = new List<string>();
    
    void Start()
    {
        if (runValidationOnStart)
        {
            StartCoroutine(RunValidation());
        }
    }
    
    private IEnumerator RunValidation()
    {
        Log("=== Card Duel Game Validation ===");
        
        // Test 1: Card System
        yield return TestCardSystem();
        
        // Test 2: Ability System
        yield return TestAbilitySystem();
        
        // Test 3: Match Structure
        yield return TestMatchStructure();
        
        // Test 4: Network Messages
        yield return TestNetworkMessages();
        
        // Test 5: Game State
        yield return TestGameState();
        
        // Summary
        Log("=== Validation Summary ===");
        foreach (string result in validationResults)
        {
            Log(result);
        }
        
        Log($"Validation complete: {validationResults.Count} tests executed");
    }
    
    private IEnumerator TestCardSystem()
    {
        Log("Testing Card System...");
        
        try
        {
            // Test card creation
            var card = new CardDuel.Gameplay.CardData(1, "Test Card", 2, 3);
            if (card.Id != 1 || card.Name != "Test Card" || card.Cost != 2 || card.Power != 3)
            {
                validationResults.Add("❌ Card creation failed");
                yield break;
            }
            
            // Test card ability
            card.Ability = new CardDuel.Gameplay.CardAbility("GainPoints", 2);
            if (card.Ability.Type != "GainPoints" || card.Ability.Value != 2)
            {
                validationResults.Add("❌ Card ability creation failed");
                yield break;
            }
            
            validationResults.Add("✅ Card system working");
        }
        catch (System.Exception e)
        {
            validationResults.Add($"❌ Card system error: {e.Message}");
        }
        
        yield return null;
    }
    
    private IEnumerator TestAbilitySystem()
    {
        Log("Testing Ability System...");
        
        try
        {
            // Test ability execution
            var ability = new CardDuel.Gameplay.CardAbility("GainPoints", 3);
            int result = CardDuel.Gameplay.CardAbilityManager.ExecuteAbilitySimple(ability, 5, 10, 8);
            
            if (result != 8) // 5 base + 3 gain
            {
                validationResults.Add("❌ Ability execution failed");
                yield break;
            }
            
            // Test ability type conversion
            var abilityType = CardDuel.Gameplay.CardAbilityManager.GetAbilityType("StealPoints");
            if (abilityType != CardDuel.Gameplay.CardAbilityType.StealPoints)
            {
                validationResults.Add("❌ Ability type conversion failed");
                yield break;
            }
            
            validationResults.Add("✅ Ability system working");
        }
        catch (System.Exception e)
        {
            validationResults.Add($"❌ Ability system error: {e.Message}");
        }
        
        yield return null;
    }
    
    private IEnumerator TestMatchStructure()
    {
        Log("Testing Match Structure...");
        
        try
        {
            // Test energy calculation
            int energyTurn1 = CardDuel.Gameplay.MatchStructure.GetEnergyForTurn(1);
            int energyTurn3 = CardDuel.Gameplay.MatchStructure.GetEnergyForTurn(3);
            int energyTurn6 = CardDuel.Gameplay.MatchStructure.GetEnergyForTurn(6);
            
            if (energyTurn1 != 1 || energyTurn3 != 3 || energyTurn6 != 6)
            {
                validationResults.Add("❌ Energy calculation failed");
                yield break;
            }
            
            // Test match completion
            bool completeTurn5 = CardDuel.Gameplay.MatchStructure.IsMatchComplete(5);
            bool completeTurn6 = CardDuel.Gameplay.MatchStructure.IsMatchComplete(6);
            
            if (completeTurn5 || !completeTurn6)
            {
                validationResults.Add("❌ Match completion logic failed");
                yield break;
            }
            
            validationResults.Add("✅ Match structure working");
        }
        catch (System.Exception e)
        {
            validationResults.Add($"❌ Match structure error: {e.Message}");
        }
        
        yield return null;
    }
    
    private IEnumerator TestNetworkMessages()
    {
        Log("Testing Network Messages...");
        
        try
        {
            // Test message serialization
            var gameStartMsg = new CardDuel.Networking.GameStartMessage
            {
                playerIds = new List<string> { "P1", "P2" },
                totalTurns = 6
            };
            
            string json = CardDuel.Networking.NetworkMessageSerializer.Serialize(gameStartMsg);
            if (string.IsNullOrEmpty(json))
            {
                validationResults.Add("❌ Message serialization failed");
                yield break;
            }
            
            // Test message deserialization
            var deserialized = CardDuel.Networking.NetworkMessageSerializer.Deserialize<CardDuel.Networking.GameStartMessage>(json);
            if (deserialized == null || deserialized.totalTurns != 6)
            {
                validationResults.Add("❌ Message deserialization failed");
                yield break;
            }
            
            validationResults.Add("✅ Network messages working");
        }
        catch (System.Exception e)
        {
            validationResults.Add($"❌ Network messages error: {e.Message}");
        }
        
        yield return null;
    }
    
    private IEnumerator TestGameState()
    {
        Log("Testing Game State...");
        
        try
        {
            // Test game state initialization
            var gameState = CardDuel.Gameplay.GameState.Instance;
            if (gameState == null)
            {
                validationResults.Add("❌ Game state not initialized");
                yield break;
            }
            
            // Test turn management
            int initialTurn = gameState.CurrentRound;
            gameState.NextRound();
            int nextTurn = gameState.CurrentRound;
            
            if (nextTurn != initialTurn + 1)
            {
                validationResults.Add("❌ Turn management failed");
                yield break;
            }
            
            validationResults.Add("✅ Game state working");
        }
        catch (System.Exception e)
        {
            validationResults.Add($"❌ Game state error: {e.Message}");
        }
        
        yield return null;
    }
    
    private void Log(string message)
    {
        if (showDetailedLogs)
        {
            Debug.Log($"[GameValidator] {message}");
        }
    }
}