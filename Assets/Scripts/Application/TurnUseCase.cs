using CardDuel.UI.Events;
using CardDuel.Networking;

namespace CardDuel.Application
{
    public class TurnUseCase
    {
        private readonly TurnLockNetworkController _turnLock;

        public TurnUseCase(TurnLockNetworkController turnLock)
        {
            _turnLock = turnLock;

            UIEventBus.Subscribe<EndTurnClickedEvent>(_ => EndTurn());
            UIEventBus.Subscribe<TurnTimerExpiredEvent>(_ => EndTurn());
        }

        private void EndTurn()
        {
            _turnLock.RequestEndTurn();

            UIEventBus.Publish(new LocalTurnLockedEvent());
            UIEventBus.Publish(new WaitingForOpponentEvent { IsWaiting = true });
        }
    }
}
