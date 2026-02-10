using CardDuel.UI.Events;
using CardDuel.Networking;
using CardDuel.Gameplay;
using Unity.Netcode;
using CardDuel.Utils;

namespace CardDuel.Application
{
    public class GameplayStartUseCase
    {
        private readonly GameState _gameState;
        private readonly TurnTimerNetworkController _timer;
        private readonly DeckService _deck;
        private readonly CardDrawNetworkController _cardDraw;
        private readonly TurnPhaseNetworkController _turnPhase;



        public GameplayStartUseCase(
            GameState gameState,
            TurnTimerNetworkController timer,
            CardDrawNetworkController cardDraw,
            DeckService deck,
            TurnPhaseNetworkController turnPhase)
        {
            _gameState = gameState;
            _timer = timer;
            _cardDraw = cardDraw;
            _deck = deck;
            _turnPhase = turnPhase;

            UIEventBus.Subscribe<GameplayStartRequestedEvent>(_ => StartGame());
        }


        private void StartGame()
        {
            if (!_timer.IsServer)
            {
                Log.App("StartGame ignored (not host)");
                return;
            }

            Log.Game("Game start (host)");

            _gameState.Reset();

            foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
            {
                _deck.InitializeDeckForPlayer(clientId);
                Log.Game($"Deck initialized for client {clientId}");
            }

            StartRound();
        }

        private void StartRound()
        {
            int cardsToDraw = _gameState.CurrentRound == 1 ? 3 : 1;

            foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
            {
                for (int i = 0; i < cardsToDraw; i++)
                {
                    int cardId = _deck.Draw(clientId);

                    Log.Game($"Drew card {cardId} for client {clientId}");

                    var rpcParams = new ClientRpcParams
                    {
                        Send = new ClientRpcSendParams
                        {
                            TargetClientIds = new[] { clientId }
                        }
                    };

                    _cardDraw.DrawCardClientRpc(cardId, rpcParams);
                }
            }

            _timer.StartTurn();

            _turnPhase.StartTurnClientRpc(
      _gameState.CurrentRound,
      _gameState.EnergyCap,
      6
  );

        }


    }
}
