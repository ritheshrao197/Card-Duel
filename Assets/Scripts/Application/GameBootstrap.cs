using UnityEngine;
using CardDuel.Application;
using CardDuel.Networking;

namespace CardDuel.Bootstrap
{
    public class GameBootstrap : MonoBehaviour
    {
        private GameFlowController _controller;

        private void Awake()
        {
            var netcode = FindObjectOfType<NetcodeBootstrapper>();
            var matchmakingNet = FindObjectOfType<MatchmakingNetworkController>();
            var turnNet = FindObjectOfType<TurnNetworkController>();
            var sceneController = FindObjectOfType<NetworkSceneController>();

            var matchmaking = new MatchmakingUseCase(netcode, matchmakingNet);
            var gameplay = new GameplayUseCase(turnNet, sceneController);
            var reconnect = new ReconnectUseCase();

            _controller = new GameFlowController(matchmaking, gameplay, reconnect);
        }

        private void OnDestroy()
        {
            _controller.Dispose();
        }
    }
}
