using Unity.Netcode;
using UnityEngine;
using CardDuel.UI.Events;
using CardDuel.Utils;
using System.Collections;

namespace CardDuel.Networking
{
    /// <summary>
    /// Monitors network connections and handles reconnection logic
    /// Manages session persistence and player state recovery
    /// </summary>
    public class ConnectionWatcher : MonoBehaviour
    {
        [Header("Reconnection Settings")]
        [SerializeField] private float reconnectionTimeout = 30f;
        [SerializeField] private int maxReconnectionAttempts = 3;
        
        private int _reconnectionAttempts = 0;
        private bool _isReconnecting = false;
        private ulong _disconnectedClientId = 0;
        private Coroutine _reconnectionCoroutine;
        
        private void OnEnable()
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
            NetworkManager.Singleton.OnServerStarted += OnServerStarted;
            NetworkManager.Singleton.OnServerStopped += OnServerStopped;
        }

        private void OnDisable()
        {
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
                NetworkManager.Singleton.OnServerStarted -= OnServerStarted;
                NetworkManager.Singleton.OnServerStopped -= OnServerStopped;
            }
            
            if (_reconnectionCoroutine != null)
            {
                StopCoroutine(_reconnectionCoroutine);
            }
        }

        private void OnClientConnected(ulong clientId)
        {
            Log.Net($"Client connected: {clientId}");
            SessionRegistry.Register(clientId);
            
            if (_isReconnecting && clientId == _disconnectedClientId)
            {
                // Player successfully reconnected
                HandleSuccessfulReconnection(clientId);
            }
            
            // Notify UI
            UIEventBus.Publish(new PlayerConnectedEvent { ClientId = clientId });
        }

        private void OnClientDisconnected(ulong clientId)
        {
            Log.Net($"Client disconnected: {clientId}");
            SessionRegistry.MarkDisconnected(clientId);
            
            // If this is during an active game, start reconnection process
            if (IsGameInProgress())
            {
                StartReconnectionProcess(clientId);
            }
            
            // Notify UI
            UIEventBus.Publish(new PlayerDisconnectedEvent { ClientId = clientId });
        }
        
        private void OnServerStarted()
        {
            Log.Net("Server started");
            SessionRegistry.Initialize();
        }
        
        private void OnServerStopped()
        {
            Log.Net("Server stopped");
            SessionRegistry.Clear();
        }
        
        private bool IsGameInProgress()
        {
            // Check if we're in the middle of a match
            // This would check game state, turn progress, etc.
            return GameState.Instance?.CurrentRound > 0;
        }
        
        private void StartReconnectionProcess(ulong clientId)
        {
            if (_isReconnecting) return;
            
            _isReconnecting = true;
            _disconnectedClientId = clientId;
            _reconnectionAttempts = 0;
            
            Log.Net($"Starting reconnection process for client {clientId}");
            
            // Notify UI that reconnection is in progress
            UIEventBus.Publish(new ReconnectionStartedEvent 
            { 
                ClientId = clientId, 
                Timeout = reconnectionTimeout 
            });
            
            // Start reconnection timeout coroutine
            _reconnectionCoroutine = StartCoroutine(ReconnectionTimeoutCoroutine());
        }
        
        private void HandleSuccessfulReconnection(ulong clientId)
        {
            _isReconnecting = false;
            _reconnectionAttempts = 0;
            _disconnectedClientId = 0;
            
            if (_reconnectionCoroutine != null)
            {
                StopCoroutine(_reconnectionCoroutine);
                _reconnectionCoroutine = null;
            }
            
            Log.Net($"Client {clientId} successfully reconnected");
            
            // Restore player state
            RestorePlayerState(clientId);
            
            // Notify UI
            UIEventBus.Publish(new ReconnectionSuccessEvent { ClientId = clientId });
        }
        
        private IEnumerator ReconnectionTimeoutCoroutine()
        {
            float timeElapsed = 0f;
            
            while (timeElapsed < reconnectionTimeout && _isReconnecting)
            {
                timeElapsed += Time.deltaTime;
                
                // Update UI with remaining time
                UIEventBus.Publish(new ReconnectionProgressEvent 
                { 
                    RemainingTime = reconnectionTimeout - timeElapsed,
                    Attempts = _reconnectionAttempts 
                });
                
                yield return null;
            }
            
            if (_isReconnecting)
            {
                // Reconnection timeout - handle failure
                HandleReconnectionFailure();
            }
        }
        
        private void HandleReconnectionFailure()
        {
            _isReconnecting = false;
            _reconnectionAttempts++;
            
            Log.Net($"Reconnection failed for client {_disconnectedClientId} (attempt {_reconnectionAttempts})");
            
            if (_reconnectionAttempts < maxReconnectionAttempts)
            {
                // Try again
                UIEventBus.Publish(new ReconnectionRetryEvent 
                { 
                    ClientId = _disconnectedClientId, 
                    AttemptsRemaining = maxReconnectionAttempts - _reconnectionAttempts 
                });
                
                // Restart reconnection process
                StartReconnectionProcess(_disconnectedClientId);
            }
            else
            {
                // Max attempts reached - end game
                Log.Net($"Max reconnection attempts reached for client {_disconnectedClientId}");
                EndGameDueToDisconnection();
            }
        }
        
        private void RestorePlayerState(ulong clientId)
        {
            // Restore player's game state
            // This would include:
            // - Current hand cards
            // - Score
            // - Turn state
            // - Staged cards (if any)
            
            Log.Net($"Restoring state for client {clientId}");
            
            // TODO: Implement actual state restoration
            // This would sync the player's current game state from the server
        }
        
        private void EndGameDueToDisconnection()
        {
            Log.Net("Ending game due to disconnection");
            
            // End the current match
            UIEventBus.Publish(new GameInterruptedEvent 
            { 
                Reason = "Player disconnected" 
            });
            
            // Return to main menu or matchmaking
            UIEventBus.Publish(new UIStateChangedEvent 
            { 
                State = UIState.MainMenu 
            });
        }
        
        /// <summary>
        /// Manual reconnection attempt
        /// </summary>
        public void AttemptReconnection(string ipAddress)
        {
            if (!_isReconnecting) return;
            
            Log.Net($"Attempting manual reconnection to {ipAddress}");
            
            // This would trigger the actual reconnection logic
            // Implementation depends on your network setup
        }
    }
    
    // Events for connection handling
    public class PlayerConnectedEvent : IUIEvent
    {
        public ulong ClientId;
    }
    
    public class PlayerDisconnectedEvent : IUIEvent
    {
        public ulong ClientId;
    }
    
    public class ReconnectionStartedEvent : IUIEvent
    {
        public ulong ClientId;
        public float Timeout;
    }
    
    public class ReconnectionProgressEvent : IUIEvent
    {
        public float RemainingTime;
        public int Attempts;
    }
    
    public class ReconnectionSuccessEvent : IUIEvent
    {
        public ulong ClientId;
    }
    
    public class ReconnectionRetryEvent : IUIEvent
    {
        public ulong ClientId;
        public int AttemptsRemaining;
    }
    
    public class GameInterruptedEvent : IUIEvent
    {
        public string Reason;
    }
}
