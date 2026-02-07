using CardDuel.Core;
using CardDuel.Events;
using UnityEngine;

namespace CardDuel.Client
{
    public class ClientGameStateApplier : MonoBehaviour
    {
        private void OnEnable()
        {
            EventBus.Subscribe<FullStateSyncEvent>(OnFullStateSync);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<FullStateSyncEvent>(OnFullStateSync);
        }

        private void OnFullStateSync(FullStateSyncEvent e)
        {
            LocalPlayerContext.PlayerId = e.snapshot.localPlayerId;
        }
    }
}
