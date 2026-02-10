using CardDuel.UI.Events;

namespace CardDuel.Application
{
    public class ReconnectUseCase
    {
        public void OnOpponentDisconnected()
        {
            UIEventBus.Publish(new OpponentTimedOutEvent());
        }

        public void OnClientReconnected()
        {
            UIEventBus.Publish(new UIStateChangedEvent
            {
                State = UIState.Gameplay
            });
        }
    }
}