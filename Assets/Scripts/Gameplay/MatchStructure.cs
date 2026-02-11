using System;
using UnityEngine;

namespace CardDuel.Gameplay
{
    /// <summary>
    /// Defines the core match structure and rules
    /// </summary>
    public static class MatchStructure
    {
        public const int TOTAL_TURNS = 6;
        public const int STARTING_HAND_SIZE = 3;
        public const int DECK_SIZE = 12;
        public const int BASE_ENERGY = 1;
        public const int MAX_ENERGY = 6;
        public const float TURN_TIME_LIMIT = 30f; // 30 seconds per turn
        
        /// <summary>
        /// Calculate energy available for a given turn
        /// </summary>
        public static int GetEnergyForTurn(int turnNumber)
        {
            if (turnNumber < 1) return BASE_ENERGY;
            if (turnNumber > TOTAL_TURNS) return MAX_ENERGY;
            
            return Math.Min(turnNumber, MAX_ENERGY);
        }
        
        /// <summary>
        /// Calculate cards to draw at the beginning of a turn.
        /// Spec: starting hand is 3 cards, then draw +1 at the
        /// start of each turn. The starting hand is dealt once
        /// at match start, so per-turn draw is always 1.
        /// </summary>
        public static int GetCardsToDraw(int turnNumber)
        {
            return 1;
        }
        
        /// <summary>
        /// Check if match is complete
        /// </summary>
        public static bool IsMatchComplete(int currentTurn)
        {
            // Match consists of exactly TOTAL_TURNS turns
            return currentTurn >= TOTAL_TURNS;
        }
        
        /// <summary>
        /// Get remaining turns in match
        /// </summary>
        public static int GetRemainingTurns(int currentTurn)
        {
            return Math.Max(0, TOTAL_TURNS - currentTurn + 1);
        }
    }
}