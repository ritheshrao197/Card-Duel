using System.Collections.Generic;
using UnityEngine;
using CardDuel.Domain.Models;

namespace CardDuel.Gameplay.Systems
{
    public class CardAbilitySystem
{
    public static void ProcessCardAbility(Card card, PlayerState playerState, PlayerState opponentState, GameState gameState)
    {
        if (card.Ability == null) return;
        
        switch (card.Ability.Type)
        {
            case "GainPoints":
                playerState.Score += card.Ability.Value;
                GameEvents.TriggerScoreUpdated(playerState.PlayerId, playerState.Score);
                break;
                
            case "StealPoints":
                int pointsToSteal = Mathf.Min(opponentState.Score, card.Ability.Value);
                playerState.Score += pointsToSteal;
                opponentState.Score -= pointsToSteal;
                GameEvents.TriggerScoreUpdated(playerState.PlayerId, playerState.Score);
                GameEvents.TriggerScoreUpdated(opponentState.PlayerId, opponentState.Score);
                break;
                
            case "DoublePower":
                playerState.Score += card.Power * 2; // Double the card's power
                GameEvents.TriggerScoreUpdated(playerState.PlayerId, playerState.Score);
                break;
                
            case "DrawExtraCard":
                // Draw extra card for the player
                // Implementation depends on deck/hand management
                break;
                
            case "DiscardOpponentRandomCard":
                // Remove random card from opponent's hand
                break;
                
            case "DestroyOpponentCardInPlay":
                // Cancel opponent's revealed card
                break;
                
            default:
                Debug.LogWarning($"Unknown ability type: {card.Ability.Type}");
                break;
        }
    }
}
}