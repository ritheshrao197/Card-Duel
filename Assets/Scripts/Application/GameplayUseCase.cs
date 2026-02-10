using CardDuel.UI.Events;
using CardDuel.Networking;

namespace CardDuel.Application
{
    public class GameplayUseCase
    {
        private readonly TurnNetworkController _turnNetwork;
        private readonly NetworkSceneController _sceneController;

        public GameplayUseCase(
            TurnNetworkController turnNetwork,
            NetworkSceneController sceneController)
        {
            _turnNetwork = turnNetwork;
            _sceneController = sceneController;
        }

        public void OnEndTurnClicked(EndTurnClickedEvent evt)
        {
            _turnNetwork.RequestEndTurn();

            UIEventBus.Publish(new WaitingForOpponentEvent
            {
                IsWaiting = true
            });
        }

        public void StartGameplay()
        {
            _sceneController.LoadSceneForAll("GameplayScene");
        }
    }
}