using Unity.Netcode;
using CardDuel.Gameplay;
using CardDuel.Utils;
using CardDuel.UI.Events;
using System;

namespace CardDuel.Networking
{
    public class TurnLockNetworkController : JsonNetworkController
    {
        protected override void RegisterMessageHandlers()
        {
            RegisterHandler("endTurn", HandleEndTurnMessage);
        }

        private void HandleEndTurnMessage(string jsonMessage)
        {
            var message = NetworkMessageSerializer.Deserialize<EndTurnMessage>(jsonMessage);
            if (message == null) return;

            ulong clientId = GetClientId(message.playerId);
            Log.Net($"EndTurn message received from {message.playerId}");

            GameState.Instance.TurnEndedPlayers.Add(clientId);

            // Notify local player that their turn is locked
            var lockMessage = new NetworkMessage("turnLocked")
            {
                action = "turnLocked"
            };
            SendToClient(clientId, lockMessage);

            if (GameState.Instance.TurnEndedPlayers.Count ==
                NetworkManager.Singleton.ConnectedClientsIds.Count)
            {
                Log.Net("Both players ended turn");
                StartReveal();
            }
        }

        private void HandleTurnLockedMessage(string jsonMessage)
        {
            UIEventBus.Publish(new LocalTurnLockedEvent());
        }

        private void StartReveal()
        {
            // Send reveal message to all clients
            var revealMessage = new NetworkMessage("revealStart")
            {
                action = "revealStart"
            };
            SendToAllClients(revealMessage);
            
            ResolveRound();
        }

        private void HandleRevealStartMessage(string jsonMessage)
        {
            Log.Net("Reveal phase started");
            UIEventBus.Publish(new UIStateChangedEvent
            {
                State = UIState.Reveal
            });
        }

        private void ResolveRound()
        {
            foreach (var kvp in GameState.Instance.PlayedCards)
            {
                int total = 0;

                foreach (var card in kvp.Value)
                    total += card.Power;

                GameState.Instance.Scores[kvp.Key] += total;
            }

            int scoreA = GetScore(0);
            int scoreB = GetScore(1);
            
            var resultMessage = new NetworkMessage("roundResult")
            {
                action = "roundResult"
            };
            // In a real implementation, you would include scores in the message
            SendToAllClients(resultMessage);

            GameState.Instance.NextRound();
        }

        private void HandleRoundResultMessage(string jsonMessage)
        {
            // In a real implementation, parse scores from message
            UIEventBus.Publish(new RoundResultEvent
            {
                PlayerAScore = 0, // Get from message
                PlayerBScore = 0  // Get from message
            });
        }

        private int GetScore(ulong clientId)
        {
            if (GameState.Instance.Scores.TryGetValue(clientId, out int score))
                return score;

            return 0;
        }

        // Legacy method for backward compatibility
        public void RequestEndTurn()
        {
            if (!IsOwner && !IsClient)
                return;

            RequestEndTurnServerRpc();
        }

        // New JSON-based method
        public void RequestEndTurnJson()
        {
            if (!IsSpawned) return;

            var message = new EndTurnMessage
            {
                playerId = GetPlayerId(NetworkManager.Singleton.LocalClientId)
            };

            SendToServer(message);
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestEndTurnServerRpc(ServerRpcParams rpcParams = default)
        {
            ulong clientId = rpcParams.Receive.SenderClientId;

            Log.Net($"EndTurn received from {clientId}");

            GameState.Instance.TurnEndedPlayers.Add(clientId);

            // Notify local player that their turn is locked
            EndTurnLockedClientRpc(clientId);

            if (GameState.Instance.TurnEndedPlayers.Count ==
                NetworkManager.Singleton.ConnectedClientsIds.Count)
            {
                Log.Net("Both players ended turn");

                StartReveal();
            }
        }

        [ClientRpc]
        private void EndTurnLockedClientRpc(ulong clientId)
        {
            if (NetworkManager.Singleton.LocalClientId == clientId)
            {
                UIEventBus.Publish(new LocalTurnLockedEvent());
            }
        }

        [ClientRpc]
        private void RevealClientRpc()
        {
            Log.Net("Reveal phase started");

            UIEventBus.Publish(new UIStateChangedEvent
            {
                State = UIState.Reveal
            });
        }

        [ClientRpc]
        private void SendResultClientRpc(int scoreA, int scoreB)
        {
            UIEventBus.Publish(new RoundResultEvent
            {
                PlayerAScore = scoreA,
                PlayerBScore = scoreB
            });
        }
    }
}
