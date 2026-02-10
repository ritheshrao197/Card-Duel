using UnityEngine;
using Unity.Netcode;
using CardDuel.Gameplay.Commands;
using CardDuel.Gameplay.Events;

namespace CardDuel.Networking
{
    public class TurnTimerNetwork : NetworkBehaviour
    {
        public NetworkVariable<float> RemainingTime =
            new(0f, NetworkVariableReadPermission.Everyone);

        private const float TURN_DURATION = 30f;

        public void StartTurn()
        {
            if (!IsServer) return;
            RemainingTime.Value = TURN_DURATION;
        }

        private void Update()
        {
            if (!IsServer) return;

            RemainingTime.Value -= Time.deltaTime;

            if (RemainingTime.Value <= 0f)
            {
                RemainingTime.Value = 0f;

                NetworkContext.MatchService.Handle(
                    new EndTurnCommand
                    {
                        PlayerId = NetworkManager.ServerClientId // ✅ correct
                    });
            }
        }
    }
}
