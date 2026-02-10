using Unity.Netcode;
using UnityEngine;
using CardDuel.UI.Events;
using CardDuel.Utils;

namespace CardDuel.Networking
{
    public class TurnTimerNetworkController : NetworkBehaviour
    {
        private float _remaining;
        private bool _running;

        public void StartTurn()
        {
            if (!IsServer) return;

            _remaining = 30f;
            _running = true;

            Log.Net("Turn timer started");
        }

        private void Update()
        {
            if (!IsServer || !_running)
                return;

            _remaining -= Time.deltaTime;

            BroadcastTickClientRpc(_remaining);

            if (_remaining <= 0f)
            {
                _running = false;
                Log.Net("Turn timer expired → auto end");
                AutoEndTurnClientRpc();
            }
        }

        [ClientRpc]
        private void BroadcastTickClientRpc(float remaining)
        {
            UIEventBus.Publish(new TurnTimerTickEvent { Remaining = remaining });
        }

        [ClientRpc]
        private void AutoEndTurnClientRpc()
        {
            UIEventBus.Publish(new EndTurnClickedEvent());
        }
    }
}
