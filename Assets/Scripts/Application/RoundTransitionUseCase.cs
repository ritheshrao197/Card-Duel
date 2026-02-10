using CardDuel.Gameplay;
using CardDuel.UI.Events;

namespace CardDuel.Application
{
    public class RoundTransitionUseCase
    {
        private readonly GameState _game;
        private readonly PlayerHandState _hand;

        public RoundTransitionUseCase(GameState game, PlayerHandState hand)
        {
            _game = game;
            _hand = hand;

            UIEventBus.Subscribe<RevealFinishedEvent>(_ => NextRound());
        }

        private void NextRound()
        {
            _game.NextRound();

            DrawCards(1);

            UIEventBus.Publish(new TurnStartedEvent
            {
                Turn = _game.CurrentRound,
                Energy = _game.EnergyCap,
                MaxEnergy = 6
            });
        }

        private void DrawCards(int count)
        {
            for (int i = 0; i < count; i++)
                _hand.Hand.Add(DrawFromDeck());
        }

        private CardData DrawFromDeck()
        {
            return new CardData(); // deck logic
        }
    }
}
