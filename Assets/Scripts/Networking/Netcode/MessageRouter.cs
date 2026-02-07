using UnityEngine;
using CardDuel.Events;
using CardDuel.Utils;
using CardDuel.Networking.Netcode;
using Logger = CardDuel.Utils.Logger;
using CardDuel.Networking.Sync;
using CardDuel.Networking.Payloads;

namespace CardDuel.Networking
{
    public static class MessageRouter
    {
        // CLIENT → SERVER
        public static void Route(ulong clientId, NetworkMessage msg)
        {
            Logger.Log(
                LogCategory.Network,
                $"Routing action '{msg.action}' from {clientId}"
            );

            switch (msg.action)
            {
                case "test":
                    NetworkGameController.Instance.Broadcast(
                        new NetworkMessage
                        {
                            action = "turnStart",
                            payload = JsonUtility.ToJson(new TurnStartEvent
                            {
                                turnNumber = 1,
                                availableCost = 1
                            })
                        });
                    break;

                case "playerReady":
                    NetworkGameController.Instance.SetPlayerReady(clientId);
                    break;

                case "endTurn":
                    EventBus.Publish(new PlayerEndedTurnEvent
                    {
                        playerId = clientId.ToString()
                    });
                    break;

                case "leaveMatch":
                    NetworkGameController.Instance.HandleManualLeave(clientId);
                    break;
            }
        }

        // SERVER → CLIENT
        public static void RouteFromServer(string json)
        {
            var msg = JsonUtility.FromJson<NetworkMessage>(json);
            Logger.Log(
                           LogCategory.Network,
                           $"Routing action '{msg.action}'"
                       );
            switch (msg.action)
            {
                case "playerConnected":
                    {
                        var evt = JsonUtility.FromJson<PlayerConnectedEvent>(msg.payload);
                        EventBus.Publish(evt);
                        break;
                    }
                case "playerDisconnected":
                    {
                        var payload =
                            JsonUtility.FromJson<PlayerDisconnectedPayload>(msg.payload);

                        EventBus.Publish(new PlayerDisconnectedEvent
                        {
                            playerId = payload.playerId
                        });
                        break;
                    }
                case "playerConnectionState":
                    {
                        var payload =
                            JsonUtility.FromJson<PlayerConnectionStatePayload>(msg.payload);

                        EventBus.Publish(new PlayerConnectedEvent
                        {
                            playerCount = payload.connectedPlayers
                        });
                        break;
                    }

                case "reconnectCountdown":
                    {
                        var payload = JsonUtility.FromJson<ReconnectCountdownPayload>(msg.payload);
                        EventBus.Publish(new ReconnectCountdownEvent
                        {
                            playerId = payload.playerId,
                            remainingTime = payload.remainingTime
                        });
                        break;
                    }

                case "turnStart":
                    {
                        var evt = JsonUtility.FromJson<TurnStartEvent>(msg.payload);
                        EventBus.Publish(evt);
                        break;
                    }

                case "gameStart":
                    {
                        var evt = JsonUtility.FromJson<GameStartEvent>(msg.payload);
                        EventBus.Publish(evt);
                        break;
                    }

                case "revealSingleCard":
                    {
                        var evt = JsonUtility.FromJson<CardRevealedEvent>(msg.payload);
                        EventBus.Publish(evt);
                        break;
                    }

                case "fullStateSync":
                    {
                        var snapshot =
                            JsonUtility.FromJson<GameStateSnapshot>(msg.payload);

                        EventBus.Publish(new FullStateSyncEvent
                        {
                            snapshot = snapshot
                        });
                        break;
                    }

                case "gameEnd":
                    {
                        var evt = JsonUtility.FromJson<GameEndEvent>(msg.payload);
                        EventBus.Publish(evt);
                        break;
                    }
                case "spectatorMode":
                    {
                        EventBus.Publish(new SpectatorModeEvent());
                        break;
                    }
                case "pendingGameOver":
                    {
                        var payload = JsonUtility.FromJson<PendingGameOverPayload>(msg.payload);

                        EventBus.Publish(new PendingGameOverEvent
                        {
                            playerId = payload.playerId,
                            remainingTime = payload.remainingTime
                        });
                        break;
                    }
                // SERVER → CLIENT
                case "matchResumed":
                    {
                        EventBus.Publish(new MatchResumedEvent());
                        break;
                    }
                


            }

        }
       


    }
}
