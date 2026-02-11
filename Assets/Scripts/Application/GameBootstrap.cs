using UnityEngine;
using CardDuel.Networking;
using CardDuel.Gameplay;
using Assets.Scripts.Application;

namespace Assets.Scripts.Application
{
    public class GameBootstrap : MonoBehaviour
    {
        private GameFlowManager _flowManager;
        [SerializeField] private CardDatabase _cardDatabase;

        private void OnEnable()
        {
            var cards = CardJsonLoader.LoadCardsFromResources("cricket_full_deck_24");

            var runtimeDb = new RuntimeCardDatabase(cards);
            CardDatabaseProvider.Initialize(runtimeDb);

            Debug.Log($"Loaded {cards.Count} cards from JSON");
            
            // Find all required network components
            var netcode = FindObjectOfType<NetcodeBootstrapper>();
            var matchmakingNet = FindObjectOfType<MatchmakingNetworkController>();
            var gameSessionController = FindObjectOfType<GameSessionNetworkController>();
            var cardPlayNetworkController = FindObjectOfType<CardPlayNetworkController>();
            var cardDrawNetworkController = FindObjectOfType<CardDrawNetworkController>();

            // Initialize core services
            var gameState = new GameState();
            var deckService = new DeckService();
            var handState = new PlayerHandState();

            // Create consolidated gameplay use case
            var gameplay = new GameplayUseCase(
                gameSessionController,
                cardPlayNetworkController,
                cardDrawNetworkController,
                deckService,
                handState,
                gameState
            );

            // Create unified flow manager
            _flowManager = new GameFlowManager(netcode, matchmakingNet, gameplay);
        }

        private void OnDestroy()
        {
            _flowManager?.Dispose();
        }
    }
}