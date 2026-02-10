using Unity.Netcode;
using CardDuel.Gameplay;
using CardDuel.UI.Events;
using CardDuel.Utils;

namespace CardDuel.Networking
{
    public class CardDrawNetworkController : NetworkBehaviour
    {
        [ClientRpc]
        public void DrawCardClientRpc(int cardId, ClientRpcParams rpcParams = default)
        {
            Log.Net($"DrawCardClientRpc received | CardId={cardId}");

            var card = CardDatabaseProvider.Get(cardId);
            if (card == null)
            {
                Log.Net($"CardData not found for {cardId}");
                return;
            }

            // 🔑 THIS WAS MISSING
            GameState.Instance.LocalHand.Hand.Add(card);

            // Optional immediate UI update
            UIEventBus.Publish(new CardAddedToHandEvent
            {
                CardId = cardId
            });
        }
    }
}