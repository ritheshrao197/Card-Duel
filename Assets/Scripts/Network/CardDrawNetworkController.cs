using Unity.Netcode;
using CardDuel.Gameplay;
using CardDuel.UI.Events;
using CardDuel.Utils;
using System;

namespace CardDuel.Networking
{
    public class CardDrawNetworkController : JsonNetworkController
    {
        public event Action<int> OnCardDrawn;

        protected override void RegisterMessageHandlers()
        {
            RegisterHandler("drawCard", HandleDrawCardMessage);
        }

        private void HandleDrawCardMessage(string jsonMessage)
        {
            var message = NetworkMessageSerializer.Deserialize<DrawCardMessage>(jsonMessage);
            if (message == null) return;

            int cardId = message.cardId;
            Log.Net($"DrawCard message received | CardId={cardId} | Player={message.playerId}");

            // Early exit for invalid card IDs
            if (cardId <= 0)
            {
                Log.Warn(LogCategory.Network, "Invalid CardId received | CardId=" + cardId);
                return;
            }
            
            var card = CardDatabaseProvider.Get(cardId);
            if (card == null)
            {
                Log.Warn(LogCategory.Network, "CardData not found | CardId=" + cardId);
                return;
            }

            // ✅ Update LOCAL player hand (each client updates its own state)
            GameState.Instance.LocalHand.Hand.Add(card);

            // ✅ Notify UI
            UIEventBus.Publish(new CardAddedToHandEvent
            {
                CardId = cardId
            });

            // Optional hook (debug / effects)
            OnCardDrawn?.Invoke(cardId);
        }

        // Legacy method for backward compatibility
        [ClientRpc]
        public void DrawCardClientRpc(int cardId, ClientRpcParams clientRpcParams = default)
        {
            // Convert to JSON message format
            var message = new DrawCardMessage
            {
                playerId = GetPlayerId(NetworkManager.Singleton.LocalClientId),
                cardId = cardId
            };
            
            HandleDrawCardMessage(NetworkMessageSerializer.Serialize(message));
        }

        // New JSON-based method
        public void RequestDrawCard(int cardId)
        {
            if (!IsSpawned) return;

            var message = new DrawCardMessage
            {
                playerId = GetPlayerId(NetworkManager.Singleton.LocalClientId),
                cardId = cardId
            };

            SendToServer(message);
        }
    }
}
// using Unity.Netcode;
// using CardDuel.Gameplay;
// using CardDuel.UI.Events;
// using CardDuel.Utils;
// using System;

// namespace CardDuel.Networking
// {
//     public class CardDrawNetworkController : NetworkBehaviour
//     {
//         public event Action<int> OnCardDrawn;

//         [ClientRpc] 
//                public void DrawCardClientRpc(int cardId)
//         {
//             Log.Net($"DrawCardClientRpc received | CardId={cardId}");
//             OnCardDrawn?.Invoke(cardId);
//         }

//         public void DrawCardClientRpc(int cardId, ClientRpcParams rpcParams = default)
//         {
//             Log.Net($"DrawCardClientRpc received | CardId={cardId}");

//             var card = CardDatabaseProvider.Get(cardId);
//             if (card == null)
//             {
//                 Log.Net($"CardData not found for {cardId}");
//                 return;
//             }

//             // 🔑 THIS WAS MISSING
//             GameState.Instance.LocalHand.Hand.Add(card);

//             // Optional immediate UI update
//             UIEventBus.Publish(new CardAddedToHandEvent
//             {
//                 CardId = cardId
//             });
//         }
//     }
// }