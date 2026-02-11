using System;
using System.Collections.Generic;
using UnityEngine;

namespace CardDuel.Gameplay
{
    /// <summary>
    /// Defines different card ability types for the card game
    /// </summary>
    public enum CardAbilityType
    {
        None,
        GainPoints,           // Add value to player score
        StealPoints,          // Take value from opponent and add to own score
        DoublePower,          // Multiply card power
        DrawExtraCard,        // Player draws extra cards
        DiscardOpponentRandomCard, // Remove random card(s) from opponent's hand
        DestroyOpponentCardInPlay  // Cancel opponent's revealed card(s) before resolution
    }
    
    /// <summary>
    /// Result of ability execution containing score changes and effects
    /// </summary>
    public class AbilityExecutionResult
    {
        public int ScoreChange;           // Points added to executing player
        public int OpponentScoreChange;   // Points added to opponent (usually negative)
        public int CardsDrawn;            // Extra cards drawn
        public List<int> DiscardedCardIds; // Cards removed from opponent
        public List<int> DestroyedCardIds; // Cards destroyed from opponent's play
        
        public AbilityExecutionResult()
        {
            DiscardedCardIds = new List<int>();
            DestroyedCardIds = new List<int>();
        }
    }
    
    /// <summary>
    /// Manages card ability execution and effects
    /// </summary>
    public static class CardAbilityManager
    {
        /// <summary>
        /// Convert string ability type to enum
        /// </summary>
        public static CardAbilityType GetAbilityType(string abilityType)
        {
            switch (abilityType?.ToLower())
            {
                case "gainpoints": return CardAbilityType.GainPoints;
                case "stealpoints": return CardAbilityType.StealPoints;
                case "doublepower": return CardAbilityType.DoublePower;
                case "drawextracard": return CardAbilityType.DrawExtraCard;
                case "discardopponentrandomcard": return CardAbilityType.DiscardOpponentRandomCard;
                case "destroyopponentcardinplay": return CardAbilityType.DestroyOpponentCardInPlay;
                default: return CardAbilityType.None;
            }
        }
        
        /// <summary>
        /// Execute a card's ability and return detailed results
        /// </summary>
        public static AbilityExecutionResult ExecuteAbility(CardAbility ability, int basePower, 
            int currentScore, int opponentScore, int opponentHandCount = 0, int opponentPlayCount = 0)
        {
            var result = new AbilityExecutionResult();
            
            if (ability == null || string.IsNullOrEmpty(ability.Type))
            {
                result.ScoreChange = basePower;
                return result;
            }
            
            var abilityType = GetAbilityType(ability.Type);
            
            switch (abilityType)
            {
                case CardAbilityType.GainPoints:
                    result.ScoreChange = basePower + ability.Value;
                    break;
                    
                case CardAbilityType.StealPoints:
                    // Steal points from opponent and add to player's score
                    int stolenPoints = Math.Min(ability.Value, opponentScore);
                    result.ScoreChange = basePower + stolenPoints;
                    result.OpponentScoreChange = -stolenPoints;
                    break;
                    
                case CardAbilityType.DoublePower:
                    result.ScoreChange = basePower * Math.Max(1, ability.Value);
                    break;
                    
                case CardAbilityType.DrawExtraCard:
                    result.ScoreChange = basePower;
                    result.CardsDrawn = ability.Value;
                    break;
                    
                case CardAbilityType.DiscardOpponentRandomCard:
                    result.ScoreChange = basePower;
                    // Mark cards for discard (actual discard handled by gameplay system)
                    for (int i = 0; i < Math.Min(ability.Value, opponentHandCount); i++)
                    {
                        result.DiscardedCardIds.Add(-1); // Placeholder, actual IDs assigned by system
                    }
                    break;
                    
                case CardAbilityType.DestroyOpponentCardInPlay:
                    result.ScoreChange = basePower;
                    // Mark cards for destruction (actual destruction handled by gameplay system)
                    for (int i = 0; i < Math.Min(ability.Value, opponentPlayCount); i++)
                    {
                        result.DestroyedCardIds.Add(-1); // Placeholder, actual IDs assigned by system
                    }
                    break;
                    
                default:
                    result.ScoreChange = basePower;
                    break;
            }
            
            return result;
        }
        
        /// <summary>
        /// Simple execution for basic score calculation
        /// </summary>
        public static int ExecuteAbilitySimple(CardAbility ability, int basePower, int currentScore, int opponentScore)
        {
            var result = ExecuteAbility(ability, basePower, currentScore, opponentScore);
            return result.ScoreChange;
        }
        
        /// <summary>
        /// Get ability description for UI display
        /// </summary>
        public static string GetAbilityDescription(CardAbility ability)
        {
            if (ability == null || string.IsNullOrEmpty(ability.Type))
                return "No special ability";
                
            var abilityType = GetAbilityType(ability.Type);
            
            switch (abilityType)
            {
                case CardAbilityType.GainPoints:
                    return $"+{ability.Value} bonus points";
                case CardAbilityType.StealPoints:
                    return $"Steal {ability.Value} points from opponent";
                case CardAbilityType.DoublePower:
                    return $"x{Math.Max(1, ability.Value)} power multiplier";
                case CardAbilityType.DrawExtraCard:
                    return $"Draw {ability.Value} extra card(s)";
                case CardAbilityType.DiscardOpponentRandomCard:
                    return $"Discard {ability.Value} random opponent card(s)";
                case CardAbilityType.DestroyOpponentCardInPlay:
                    return $"Destroy {ability.Value} opponent card(s) in play";
                default:
                    return $"Special effect: {ability.Type}";
            }
        }
        
        /// <summary>
        /// Check if ability has disruptive effect on opponent
        /// </summary>
        public static bool HasDisruptiveEffect(CardAbility ability)
        {
            if (ability == null) return false;
            
            var abilityType = GetAbilityType(ability.Type);
            return abilityType == CardAbilityType.DiscardOpponentRandomCard ||
                   abilityType == CardAbilityType.DestroyOpponentCardInPlay;
        }
    }
}