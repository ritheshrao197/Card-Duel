using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;
using CardDuel.Utils;
using CardDuel.Gameplay.Match;
using CardDuel.Gameplay.Turn;
using CardDuel.Core;
using CardDuel.Events;
using CardDuel.Networking.Sync;
using CardDuel.Networking.Payloads;
using Logger = CardDuel.Utils.Logger;
using System;

namespace CardDuel.Networking.Netcode
{
    public class NetworkGameController : NetworkBehaviour
    {
        public static NetworkGameController Instance;

        // ================= CONFIG =================
        private const float TURN_TIME = 30f;
        private const float DISCONNECT_GRACE_TIME = 20f;
        private const float COUNTDOWN_BROADCAST_INTERVAL = 1f;

        // ================= STATE =================
        private enum DisconnectState
        {
            None,
            PendingGameOver
        }

        private DisconnectState _disconnectState = DisconnectState.None;
        private float _disconnectTimer;
        private float _nextCountdownBroadcast;
        private string _disconnectedPlayerId;

        private MatchPhase _phase = MatchPhase.WaitingForPlayers;
        private ServerTurnTimer _turnTimer;
        private readonly GameState _gameState = new();

        private readonly Dictionary<ulong, bool> _playerReady = new();

        private int _playerIndex;
        private readonly string[] _playerIds = { "P1", "P2" };

        // ================= UNITY =================
        private void Awake()
        {
            Instance = this;
            _turnTimer = gameObject.AddComponent<ServerTurnTimer>();
        }

        public override void OnNetworkSpawn()
        {
            if (!IsServer) return;

            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;

            Logger.Log(LogCategory.Network, "Server started");
        }

        private void Update()
        {
            if (!IsServer || _disconnectState != DisconnectState.PendingGameOver)
                return;

            _disconnectTimer -= Time.deltaTime;

            if (Time.time >= _nextCountdownBroadcast)
            {
                _nextCountdownBroadcast = Time.time + COUNTDOWN_BROADCAST_INTERVAL;
                BroadcastPendingGameOver();
            }

            if (_disconnectTimer <= 0f)
            {
                FinalizeGameOver();
            }
        }

        // ================= CONNECTION =================
        private void OnClientConnected(ulong clientId)
        {
            RegisterPlayer(clientId);
            BroadcastPlayerConnectionState();
        }

        private void OnClientDisconnected(ulong clientId)
        {
            if (_phase != MatchPhase.InProgress)
                return;

            foreach (var p in _gameState.players.Values)
            {
                if (p.lastClientId != clientId)
                    continue;

                p.isConnected = false;
                _disconnectedPlayerId = p.playerId;

                Logger.Warn(LogCategory.Network,
                    $"Player {p.playerId} disconnected. Pending game over.");

                StartPendingGameOver();
                return;
            }
        }

        private void RegisterPlayer(ulong clientId)
        {
            // Reconnect
            foreach (var p in _gameState.players.Values)
            {
                if (!p.isConnected)
                {
                    p.isConnected = true;
                    p.lastClientId = clientId;

                    Logger.Log(LogCategory.Network,
                        $"Player {p.playerId} reconnected");

                    SendFullState(clientId);

                    if (_disconnectState == DisconnectState.PendingGameOver)
                        ResumeMatch();

                    return;
                }
            }

            // Spectator
            if (_gameState.players.Count >= 2)
            {
                SendFullState(clientId);
                SendSpectatorFlag(clientId);
                return;
            }

            // New player
            var playerState = new PlayerState
            {
                playerId = _playerIds[_playerIndex++],
                lastClientId = clientId,
                isConnected = true
            };

            _gameState.players[playerState.playerId] = playerState;

            Logger.Log(LogCategory.Network,
                $"Player {playerState.playerId} joined");
        }

        // ================= MATCH FLOW =================
        public void SetPlayerReady(ulong clientId)
        {
            _playerReady[clientId] = true;

            if (_playerReady.Count == 2 && AllPlayersReady())
                StartMatch();
        }

        private bool AllPlayersReady()
        {
            foreach (var ready in _playerReady.Values)
                if (!ready) return false;
            return true;
        }

        private void StartMatch()
        {
            _phase = MatchPhase.InProgress;

            Broadcast(new NetworkMessage
            {
                action = "gameStart",
                payload = JsonUtility.ToJson(new GameStartEvent { totalTurns = 6 })
            });

            StartTurn(1);
        }

        public void StartTurn(int turn)
        {
            _gameState.currentTurn = turn;

            Broadcast(new NetworkMessage
            {
                action = "turnStart",
                payload = JsonUtility.ToJson(new TurnStartEvent
                {
                    turnNumber = turn,
                    availableCost = turn
                })
            });

            _turnTimer.StartTimer(TURN_TIME);
        }

        // ================= DISCONNECT FLOW =================
        private void StartPendingGameOver()
        {
            _disconnectState = DisconnectState.PendingGameOver;
            _disconnectTimer = DISCONNECT_GRACE_TIME;
            _nextCountdownBroadcast = 0f;

            _turnTimer.Pause();

            BroadcastPendingGameOver();
            
        }

        private void ResumeMatch()
        {
            Logger.Log(LogCategory.Gameplay, "Match resumed after reconnect");

            _disconnectState = DisconnectState.None;
            _turnTimer.Resume();

            Broadcast(new NetworkMessage
            {
                action = "matchResumed",
                payload = "{}"
            });
        }

        private void FinalizeGameOver()
        {
            _disconnectState = DisconnectState.None;
            _phase = MatchPhase.Finished;

            var winner = GetOpponentById(_disconnectedPlayerId);

            Logger.Warn(LogCategory.Gameplay,
                $"Game over. Player {_disconnectedPlayerId} forfeited.");

            Broadcast(new NetworkMessage
            {
                action = "gameEnd",
                payload = JsonUtility.ToJson(new GameEndEvent
                {
                    winnerId = winner.playerId
                })
            });
        }

        // ================= SNAPSHOT =================
        public void SendFullState(ulong clientId)
        {
            var snapshot = BuildSnapshotForClient(clientId);

            SendToClient(clientId, new NetworkMessage
            {
                action = "fullStateSync",
                payload = JsonUtility.ToJson(snapshot)
            });
        }
        private GameStateSnapshot BuildSnapshotForClient(ulong clientId)
        {
            var snapshot = BuildSnapshot(); // existing shared state

            foreach (var p in _gameState.players.Values)
            {
                if (p.lastClientId == clientId)
                {
                    snapshot.localPlayerId = p.playerId;
                    return snapshot;
                }
            }

            // Spectator
            snapshot.localPlayerId = null;
            return snapshot;
        }


        private GameStateSnapshot BuildSnapshot()
        {
            var snapshot = new GameStateSnapshot
            {
                currentTurn = _gameState.currentTurn,
                phase = _phase.ToString(),
                remainingTurnTime = _turnTimer.RemainingTime
            };

            foreach (var p in _gameState.players.Values)
            {
                snapshot.scores[p.playerId] = p.score;
                snapshot.foldedCardCounts[p.playerId] = p.foldedCards.Count;
            }

            return snapshot;
        }

        // ================= BROADCASTS =================
        private void BroadcastPendingGameOver()
        {
            Broadcast(new NetworkMessage
            {
                action = "pendingGameOver",
                payload = JsonUtility.ToJson(new PendingGameOverPayload
                {
                    playerId = _disconnectedPlayerId,
                    remainingTime = _disconnectTimer
                })
            });
        }

        private void BroadcastPlayerConnectionState()
        {
            Broadcast(new NetworkMessage
            {
                action = "playerConnectionState",
                payload = JsonUtility.ToJson(new PlayerConnectionStatePayload
                {
                    connectedPlayers = CountConnectedPlayers()
                })
            });
        }

        // ================= NETWORK =================
        public void OnMessageFromClient(ulong clientId, string json)
        {
            MessageRouter.Route(clientId,
                JsonUtility.FromJson<NetworkMessage>(json));
        }

        public void Broadcast(NetworkMessage msg)
        {
            SendMessageClientRpc(JsonUtility.ToJson(msg));
        }

        private void SendToClient(ulong clientId, NetworkMessage msg)
        {
            var rpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new[] { clientId }
                }
            };

            SendMessageClientRpc(JsonUtility.ToJson(msg), rpcParams);
        }

        [ClientRpc]
        private void SendMessageClientRpc(
            string json,
            ClientRpcParams rpcParams = default)
        {
            MessageRouter.RouteFromServer(json);
        }

        // ================= HELPERS =================
        private int CountConnectedPlayers()
        {
            int count = 0;
            foreach (var p in _gameState.players.Values)
                if (p.isConnected) count++;
            return count;
        }

        private PlayerState GetOpponentById(string playerId)
        {
            foreach (var p in _gameState.players.Values)
                if (p.playerId != playerId)
                    return p;
            return null;
        }

        private void SendSpectatorFlag(ulong clientId)
        {
            SendToClient(clientId, new NetworkMessage
            {
                action = "spectatorMode",
                payload = "{}"
            });
        }

        public int GetCurrentTurn() => _gameState.currentTurn;

        public void HandleManualLeave(ulong clientId)
        {
            if (_phase != MatchPhase.InProgress)
                return;

            foreach (var p in _gameState.players.Values)
            {
                if (p.lastClientId == clientId)
                {
                    Logger.Warn(LogCategory.Gameplay,
                        $"Player {p.playerId} left match manually");

                    FinalizeGameOverForManualLeave(p.playerId);
                    return;
                }
            }
        }

        private void FinalizeGameOverForManualLeave(string leavingPlayerId)
        {
            _disconnectState = DisconnectState.None;
            _phase = MatchPhase.Finished;

            var winner = GetOpponentById(leavingPlayerId);

            Broadcast(new NetworkMessage
            {
                action = "gameEnd",
                payload = JsonUtility.ToJson(new GameEndEvent
                {
                    winnerId = winner.playerId
                })
            });
        }
    }
}
