using CardDuel.UI.Events;
using CardDuel.Networking;
using CardDuel.Gameplay;
using CardDuel.Utils;
using Unity.Netcode;

namespace Assets.Scripts.Application
{
    public class GameplayUseCase
    {
        private readonly GameSessionNetworkController _gameSession;
        private readonly CardPlayNetworkController _cardPlay;
        private readonly CardDrawNetworkController _cardDraw;
        private readonly DeckService _deck;

        // Local, non-committal selection state (per player)
        private int _currentEnergy;
        private readonly PlayerHandState _hand;
        private readonly GameState _game;

        public GameplayUseCase(
            GameSessionNetworkController gameSession,
            CardPlayNetworkController cardPlay,
            CardDrawNetworkController cardDraw,
            DeckService deck,
            PlayerHandState hand,
            GameState game)
        {
            _gameSession = gameSession;
            _cardPlay = cardPlay;
            _cardDraw = cardDraw;
            _deck = deck;
            _hand = hand;
            _game = game;

            Log.App("GameplayUseCase initialized");

            // Card selection events
            UIEventBus.Subscribe<CardSelectedEvent>(OnCardSelected);
            UIEventBus.Subscribe<CardDeselectedEvent>(OnCardDeselected);
            
            // Turn events
            UIEventBus.Subscribe<CardDuel.UI.Events.TurnStartedEvent>(OnTurnStarted);
            UIEventBus.Subscribe<EndTurnClickedEvent>(OnEndTurnClicked);
            UIEventBus.Subscribe<TurnTimerExpiredEvent>(OnTurnTimerExpired);
            
            // Card draw events
            _cardDraw.OnCardDrawn += OnCardDrawn;
            
            // Round transition events
            UIEventBus.Subscribe<RevealFinishedEvent>(OnRevealFinished);
            UIEventBus.Subscribe<GameplayStartRequestedEvent>(OnGameplayStartRequested);
        }


        private void OnTurnStarted(CardDuel.UI.Events.TurnStartedEvent evt)
        {
            _currentEnergy = evt.Energy;

            Log.Game(
                $"TurnStarted | Turn={evt.Turn} | Energy={evt.Energy} | Selection cleared");
        }

        private void OnTurnTimerExpired(TurnTimerExpiredEvent evt)
        {
            Log.App("Turn timer expired, ending turn");
            EndTurn();
        }

        private void OnGameplayStartRequested(GameplayStartRequestedEvent evt)
        {
            if (!_gameSession.IsServer)
            {
                Log.App("StartGame ignored (not host)");
                return;
            }

            Log.Game("Game start (host)");

            _game.ResetMatch(); 
            
            foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
            {
                _deck.InitializeDeckForPlayer(clientId);
                Log.Game($"Deck initialized for client {clientId}");
            }

            StartRound(initial: true);
        }

        private void StartRound(bool initial = false)
        {
            int cardsToDraw = initial ? 3 : 1;

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

            _gameSession.StartTurn();
        }

        private void OnCardSelected(CardSelectedEvent evt)
        {
            Log.App("CardSelectedEvent received | CardId=" + evt.CardId);

            var hand = GameState.Instance.LocalHand;

            // Use O(1) lookup instead of linear search
            CardData card = hand.GetCardFromHand(evt.CardId);
            
            if (card == null)
            {
                Log.Warn(LogCategory.Gameplay,
                    "Card not found in LocalHand | CardId=" + evt.CardId);
                return;
            }

            // Energy cost validation
            int currentCost = hand.TotalDrawnCost();
            if (currentCost + card.Cost > _currentEnergy)
            {
                Log.Game($"Selection rejected - energy exceeded | " +
                    $"Current: {currentCost}, Card: {card.Cost}, Available: {_currentEnergy}");
                return;
            }

            hand.RemoveFromHand(card);
            hand.AddToDrawn(card);

            Log.Game($"Card selected | {card.Name} | Cost: {card.Cost} | " +
                $"Total drawn cost: {hand.TotalDrawnCost()}");

            UIEventBus.Publish(new CardMovedEvent
            {
                CardId = evt.CardId,
                ToDrawnArea = true
            });
        }

        private void OnCardDrawn(int cardId)
        {
            // Validate card ID
            if (cardId <= 0)
            {
                Log.Error($"Invalid card ID received | CardId={cardId}");
                return;
            }

            var card = CardDatabaseProvider.Get(cardId);
            if (card == null)
            {
                Log.Error($"Card not found in database | CardId={cardId}");
                return;
            }

            // Validate card data
            if (string.IsNullOrEmpty(card.Name))
            {
                Log.Warn(LogCategory.Application, $"Card has invalid data | CardId={cardId}");
                return;
            }

            _hand.AddToHand(card);
            Log.Game($"Card added to hand | {card.Name} (ID: {card.Id}) | " +
                $"Hand size: {_hand.Hand.Count}");
            UIEventBus.Publish(new HandStateChangedEvent());
        }



        private void OnCardDeselected(CardDeselectedEvent evt)
        {
            Log.App("CardDeselectedEvent received | CardId=" + evt.CardId);

            // Use O(1) lookup instead of linear search
            CardData card = _hand.GetCardFromDrawn(evt.CardId);
                    
            if (card == null)
            {
                Log.Warn(LogCategory.Gameplay,
                    "Card not found in drawn pile | CardId=" + evt.CardId);
                return;
            }
        
            UnselectCard(card);
        }

        private void UnselectCard(CardData card)
        {
            if (_hand.RemoveFromDrawn(card))
            {
                _hand.AddToHand(card);
                
                Log.Game($"Card unselected | {card.Name} | " +
                    $"New total drawn cost: {_hand.TotalDrawnCost()}");

                UIEventBus.Publish(new HandStateChangedEvent());
            }
            else
            {
                Log.Warn(LogCategory.Gameplay, 
                    $"Failed to unselect card | {card.Name} | Card not in drawn pile");
            }
        }


        private void OnEndTurnClicked(EndTurnClickedEvent evt)
        {
            Log.App($"EndTurnClicked | Cards={_hand.Drawn.Count}");
            EndTurn();
        }

        private void EndTurn()
        {
            foreach (var card in _hand.Drawn)
            {
                _cardPlay.RequestPlayCard(card.Id, slotIndex: 0);
            }

            _gameSession.RequestEndTurn();
            
            UIEventBus.Publish(new LocalTurnLockedEvent());
            UIEventBus.Publish(new WaitingForOpponentEvent { IsWaiting = true });
        }

        private void OnRevealFinished(RevealFinishedEvent evt)
        {
            _game.NextRound();
            
            // Draw one card for next round
            int cardId = _deck.Draw(NetworkManager.Singleton.LocalClientId);
            var rpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new[] { NetworkManager.Singleton.LocalClientId }
                }
            };
            _cardDraw.DrawCardClientRpc(cardId, rpcParams);

            UIEventBus.Publish(new CardDuel.UI.Events.TurnStartedEvent
            {
                Turn = _game.CurrentRound,
                Energy = _game.EnergyCap,
                MaxEnergy = 6
            });
        }

    }
}
