using CardDuel.Events;

namespace CardDuel.Gameplay.Turn
{
    public class TurnManager
    {
        private int _currentTurn = 0;

        public void StartTurn()
        {
            _currentTurn++;

            EventBus.Publish(new TurnStartEvent
            {
                turnNumber = _currentTurn,
                availableCost = _currentTurn
            });
        }

        public void EndTurn(string playerId)
        {
            EventBus.Publish(new PlayerEndedTurnEvent
            {
                playerId = playerId
            });
        }
    }
}
