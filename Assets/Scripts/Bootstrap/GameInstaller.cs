using UnityEngine;
using CardDuel.Domain.State;
using CardDuel.Application.UseCases;
using CardDuel.Core;

namespace CardDuel.Bootstrap
{
    /// <summary>
    /// Game installer - composition root
    /// Sets up all dependencies and wiring
    /// </summary>
    public class GameInstaller : MonoBehaviour
    {
        [Header("Game Configuration")]
        public int MaxTurns = 6;
        public int StartingHandSize = 3;

        // Core game components
        private MatchState _matchState;
        private GameApplicationService _applicationService;

        void Awake()
        {
            InitializeGame();
        }

        private void InitializeGame()
        {
            // 1. Create domain state (pure C#, no Unity dependencies)
            _matchState = new MatchState(MaxTurns);
            
            // 2. Create application service
            _applicationService = new GameApplicationService(_matchState);
            
            // 3. Subscribe to command events from UI/Network
            EventBus.Subscribe<ClientCommandReceived>(OnCommandReceived);
            
            // 4. Subscribe to domain events for broadcasting
            SubscribeToDomainEvents();
            
            Debug.Log("Game initialized with domain-first architecture");
        }

        private void OnCommandReceived(ClientCommandReceived evt)
        {
            // Handle command through application layer
            var events = _applicationService.HandleCommand(evt.Command);
            
            // Publish all resulting events
            foreach (var domainEvent in events)
            {
                EventBus.Publish(domainEvent);
            }
        }

        private void SubscribeToDomainEvents()
        {
            // Subscribe to all domain events that need network broadcasting
            EventBus.Subscribe<GameStartedEvent>(BroadcastEvent);
            EventBus.Subscribe<TurnStartedEvent>(BroadcastEvent);
            EventBus.Subscribe<PlayerEndedTurnEvent>(BroadcastEvent);
            EventBus.Subscribe<CardPlayedEvent>(BroadcastEvent);
            EventBus.Subscribe<ScoreUpdatedEvent>(BroadcastEvent);
            EventBus.Subscribe<TurnEndedEvent>(BroadcastEvent);
            EventBus.Subscribe<GameEndedEvent>(BroadcastEvent);
        }

        private void BroadcastEvent<T>(T evt) where T : IEvent
        {
            // In a real implementation, this would send events through NetworkService
            Debug.Log($"Broadcasting event: {typeof(T).Name}");
        }

        // Public accessors for other systems
        public MatchState GetMatchState() => _matchState;
        public GameApplicationService GetApplicationService() => _applicationService;

        void OnDestroy()
        {
            // Clean up subscriptions
            EventBus.Unsubscribe<ClientCommandReceived>(OnCommandReceived);
            EventBus.Clear(); // Clear all handlers
        }
    }
}