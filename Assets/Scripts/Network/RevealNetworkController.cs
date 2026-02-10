using Unity.Netcode;
using System.Collections;
using System.Collections.Generic;
using CardDuel.UI.Events;
using UnityEngine;
using CardDuel.Gameplay;

namespace CardDuel.Networking
{
    public class RevealNetworkController : NetworkBehaviour
    {
        private readonly Dictionary<ulong, int> _scores = new();

        public void StartReveal(
            ulong firstPlayer,
            List<int> playerACardIds,
            List<int> playerBCardIds,
            ulong playerAId,
            ulong playerBId)
        {
            if (!IsServer) return;

            if (!_scores.ContainsKey(playerAId)) _scores[playerAId] = 0;
            if (!_scores.ContainsKey(playerBId)) _scores[playerBId] = 0;

            StartCoroutine(RevealRoutine(
                firstPlayer,
                playerACardIds,
                playerBCardIds,
                playerAId,
                playerBId));
        }

        private IEnumerator RevealRoutine(
            ulong initiativePlayer,
            List<int> a,
            List<int> b,
            ulong aId,
            ulong bId)
        {
            int index = 0;
            ulong current = initiativePlayer;

            while (index < a.Count || index < b.Count)
            {
                if (current == aId && index < a.Count)
                    ResolveCard(aId, a[index++]);

                else if (current == bId && index < b.Count)
                    ResolveCard(bId, b[index++]);

                current = current == aId ? bId : aId;

                yield return new WaitForSeconds(0.8f);
            }

            RevealFinishedClientRpc();
        }

        private void ResolveCard(ulong owner, int cardId)
        {
            // Host resolves logic using its own database
            var card = CardDatabaseProvider.Get(cardId);

            int delta = card.Power;

            if (card.Ability != null && card.Ability.Type == "GainRuns")
                delta += card.Ability.Value;

            _scores[owner] += delta;

            RevealCardClientRpc(owner, cardId, delta, _scores[owner]);
        }

        [ClientRpc]
        private void RevealCardClientRpc(
            ulong owner,
            int cardId,
            int delta,
            int newScore)
        {
            UIEventBus.Publish(new CardRevealEvent
            {
                OwnerClientId = owner,
                CardId = cardId
            });

            UIEventBus.Publish(new ScoreUpdatedEvent
            {
                PlayerId = owner,
                Delta = delta,
                NewScore = newScore
            });
        }

        [ClientRpc]
        private void RevealFinishedClientRpc()
        {
            UIEventBus.Publish(new RevealFinishedEvent());
        }
    }
}
