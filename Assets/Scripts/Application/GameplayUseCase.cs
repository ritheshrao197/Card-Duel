using CardDuel.UI.Events;
using CardDuel.Networking;
using CardDuel.Gameplay;
using CardDuel.Utils;
using System.Collections.Generic;

namespace CardDuel.Application
{
    public class GameplayUseCase
    {
        private readonly TurnLockNetworkController _turnLock;
        private readonly CardPlayNetworkController _cardPlay;

        // Local, non-committal selection state (per player)
        private readonly HashSet<int> _selectedCards = new();
        private int _currentEnergy;
        private readonly PlayerHandState _hand;
        private readonly GameState _game;

        public GameplayUseCase(
     TurnLockNetworkController turnLock,
     CardPlayNetworkController cardPlay,
     PlayerHandState hand,
     GameState game)
        {
            _turnLock = turnLock;
            _cardPlay = cardPlay;
            _hand = hand;
            _game = game;

            Log.App("GameplayUseCase initialized");

            UIEventBus.Subscribe<CardSelectedEvent>(OnCardSelected);
            UIEventBus.Subscribe<CardDeselectedEvent>(OnCardDeselected);
            UIEventBus.Subscribe<TurnStartedEvent>(OnTurnStarted);
            UIEventBus.Subscribe<EndTurnClickedEvent>(OnEndTurnClicked);
        }


        private void OnTurnStarted(TurnStartedEvent evt)
        {
            _currentEnergy = evt.Energy;
            _selectedCards.Clear();

            Log.Game(
                $"TurnStarted | Turn={evt.Turn} | Energy={evt.Energy} | Selection cleared");
        }

        private void OnCardSelected(CardSelectedEvent evt)
        {
            var card = _hand.Hand.Find(c => c.Id == evt.CardId);
            if (card == null)
                return;

            if (_hand.TotalDrawnCost() + card.Cost > _game.EnergyCap)
            {
                Log.Game("Selection rejected (energy exceeded)");
                return;
            }

            SelectCard(card);
        }
        private void SelectCard(CardData card)
        {
            _hand.Hand.Remove(card);
            _hand.Drawn.Add(card);

            Log.Game($"Card selected | {card.Name}");

            UIEventBus.Publish(new HandStateChangedEvent());
        }

        private void OnCardDeselected(CardDeselectedEvent evt)
        {
            var card = _hand.Drawn.Find(c => c.Id == evt.CardId);
            if (card == null)
                return;

            UnselectCard(card);
        }

        private void UnselectCard(CardData card)
        {
            _hand.Drawn.Remove(card);
            _hand.Hand.Add(card);

            Log.Game($"Card unselected | {card.Name}");

            UIEventBus.Publish(new HandStateChangedEvent());
        }


        private void OnEndTurnClicked(EndTurnClickedEvent evt)
        {
            Log.App($"EndTurnClicked | Cards={_hand.Drawn.Count}");

            foreach (var card in _hand.Drawn)
            {
                _cardPlay.RequestPlayCard(card.Id, slotIndex: 0);
            }

            _turnLock.RequestEndTurn();
        }

    }
}
