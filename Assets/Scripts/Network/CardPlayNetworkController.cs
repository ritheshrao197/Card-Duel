using Unity.Netcode;
using CardDuel.UI.Events;

namespace CardDuel.Networking
{
    public class CardPlayNetworkController : NetworkBehaviour
    {
        public void RequestPlayCard(int cardId, int slotIndex)
        {
            if (!IsSpawned)
                return;

            SubmitPlayCardServerRpc(cardId, slotIndex);
        }

        [ServerRpc(RequireOwnership = false)]
        private void SubmitPlayCardServerRpc(
            int cardId,
            int slotIndex,
            ServerRpcParams rpcParams = default)
        {
            var clientId = rpcParams.Receive.SenderClientId;

            if (!ValidateCardPlay(clientId, cardId))
                return;

            CommitCardPlay(clientId, cardId, slotIndex);
        }

        private bool ValidateCardPlay(ulong clientId, int cardId)
        {
            // TODO:
            // - Is card in hand?
            // - Enough energy?
            // - Slot free?
            return true;
        }

        private void CommitCardPlay(ulong clientId, int cardId, int slotIndex)
        {
            BroadcastCardPlayedClientRpc(clientId, cardId, slotIndex);
        }

        [ClientRpc]
        private void BroadcastCardPlayedClientRpc(
            ulong clientId,
            int cardId,
            int slotIndex)
        {
            UIEventBus.Publish(new CardPlacedEvent
            {
                CardId = cardId,
                SlotIndex = slotIndex,
                OwnerClientId = clientId
            });
        }
    }
}
