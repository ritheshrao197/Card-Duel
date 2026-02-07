using System;
using CardDuel.Cards;
using CardDuel.Core;
using CardDuel.Events;
using CardDuel.Networking;
using CardDuel.Networking.Netcode;
using CardDuel.Networking.Payloads;
using UnityEngine;
namespace CardDuel.Gameplay.Reveal
{
    public class RevealManager
    {
        public void RevealCard(
            CardInstance card,
            PlayerState owner,
            PlayerState opponent)
        {
            CardResolver.ResolveCard(card, owner, opponent);

            EventBus.Publish(new CardRevealedEvent
            {
                playerId = owner.playerId,
                card = card
            });

            EventBus.Publish(new ScoreUpdatedEvent
            {
                playerScore = owner.score,
                opponentScore = opponent.score
            });
        }
        // public void ExecuteReveal(RevealQueue queue)
        // {
        //     while (queue.HasNext())
        //     {
        //         var (owner, card) = queue.Next();

        //         CardResolver.ResolveCard(card, owner, GetOpponent(owner));

        //         NetworkGameController.Instance.Broadcast(
        //             new NetworkMessage
        //             {
        //                 action = "revealSingleCard",
        //                 payload = JsonUtility.ToJson(new RevealCardPayload
        //                 {
        //                     playerId = owner.playerId,
        //                     cardId = card.definition.id,
        //                     orderIndex = card.orderIndex
        //                 })
        //             });
        //     }
        // }


    }
}