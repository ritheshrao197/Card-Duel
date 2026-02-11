using System;
using UnityEngine;

namespace CardDuel.Gameplay
{
    /// <summary>
    /// Defines different card ability types for the existing system
    /// </summary>
    public enum CardAbilityType
    {
        None,
        GainPoints,
        StealPoints,
        DoublePower,
        Disruption
    }
    
    /// <summary>
    /// Manages card ability execution and effects using existing CardAbility
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
                case "disruption":
                case "discardopponentrandomcard":
                case "destroyopponentcardinplay": return CardAbilityType.Disruption;
                default: return CardAbilityType.None;
            }
        }
        
        /// <summary>
        /// Execute a card's ability and return the resulting score change
        /// </summary>
        public static int ExecuteAbility(CardAbility ability, int basePower, int currentScore, int opponentScore)
        {
            if (ability == null || string.IsNullOrEmpty(ability.Type))
            {
                return basePower;
            }
            
            var abilityType = GetAbilityType(ability.Type);
            
            switch (abilityType)
            {
                case CardAbilityType.GainPoints:
                    return basePower + ability.Value;
                    
                case CardAbilityType.StealPoints:
                    // Steal points from opponent and add to player's score
                    int stolenPoints = Math.Min(ability.Value, opponentScore);
                    return basePower + stolenPoints;
                    
                case CardAbilityType.DoublePower:
                    return basePower * Math.Max(1, ability.Value);
                    
                case CardAbilityType.Disruption:
                    // Disruption effect handled separately
                    return basePower;
                    
                default:
                    return basePower;
            }
        }
        
        /// <summary>
        /// Apply disruption effect to opponent
        /// </summary>
        public static int ApplyDisruption(int disruptionValue, int opponentCardCount)
        {
            return Math.Max(0, opponentCardCount - disruptionValue);
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
                case CardAbilityType.Disruption:
                    return $"Remove {ability.Value} opponent's cards";
                default:
                    return $"Special effect: {ability.Type}";
            }
        }
    }
}