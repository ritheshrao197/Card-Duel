using UnityEngine;
using CardDuel.Application;
using CardDuel.Networking;
using CardDuel.Gameplay;

namespace CardDuel.Bootstrap
{
    public class GameBootstrap : MonoBehaviour
    {
        private GameFlowController _controller;
        [SerializeField] private CardDatabase cardDatabase;

        private void OnEnable()
        {
            var cards = CardJsonLoader.LoadCardsFromResources("cricket_full_deck_24");

            var runtimeDb = new RuntimeCardDatabase(cards);
            CardDatabaseProvider.Initialize(runtimeDb);

            Debug.Log($"Loaded {cards.Count} cards from JSON");
            var netcode = FindObjectOfType<NetcodeBootstrapper>();
            var matchmakingNet = FindObjectOfType<MatchmakingNetworkController>();
            var turnNet = FindObjectOfType<TurnLockNetworkController>();
            var sceneController = FindObjectOfType<NetworkSceneController>();
            var cardPlayNetworkController = FindObjectOfType<CardPlayNetworkController>();
            var CardDrawNetworkController = FindObjectOfType<CardDrawNetworkController>();
            var turnPhaseNetworkController = FindObjectOfType<TurnPhaseNetworkController>();

            var matchmaking = new MatchmakingUseCase(netcode, matchmakingNet);
            var reconnect = new ReconnectUseCase();

            // Core services
            var gameState = new GameState();
            var deckService = new DeckService();
            var hand = new PlayerHandState();

            var gameplay = new GameplayUseCase(turnNet, cardPlayNetworkController,hand, gameState);

            _controller = new GameFlowController(matchmaking, gameplay, reconnect);

            // Network controllers
            var timer = FindObjectOfType<TurnTimerNetworkController>();

            // Use cases
            new GameplayStartUseCase(gameState, timer, CardDrawNetworkController, deckService, turnPhaseNetworkController);
        }

        private void OnDestroy()
        {
            _controller.Dispose();
        }
    }
}