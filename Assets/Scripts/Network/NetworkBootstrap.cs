using UnityEngine;
using Unity.Netcode;
using CardDuel.Server;

namespace CardDuel.Networking
{
    public class NetworkBootstrap : NetworkBehaviour
    {
        private MatchService matchService;

        public override void OnNetworkSpawn()
        {
            if (!IsServer) return;

            var relay = GetComponent<NetworkEventRelay>();
            var timer = GetComponent<TurnTimerNetwork>();

            matchService = new MatchService(relay, timer);
            NetworkContext.MatchService = matchService;
        }
    }
}
