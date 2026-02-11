using Unity.Netcode;
using CardDuel.UI.Events;
using CardDuel.Utils;
using CardDuel.Gameplay;

namespace CardDuel.Networking
{
    public class CardPlayNetworkController : JsonNetworkController
    {
        protected override void RegisterMessageHandlers()
        {
            RegisterHandler("playCard", HandlePlayCardMessage);
        }

        private void HandlePlayCardMessage(string jsonMessage)
        {
            var message = NetworkMessageSerializer.Deserialize<PlayCardMessage>(jsonMessage);
            if (message == null) return;

            ulong clientId = GetClientId(message.playerId);
            int cardId = message.cardId;
            int slotIndex = message.slotIndex;

            Log.Net($"PlayCard message received | Player={message.playerId} CardId={cardId} Slot={slotIndex}");

            if (!ValidateCardPlay(clientId, cardId, slotIndex))
                return;

            CommitCardPlay(clientId, cardId, slotIndex);
        }

        private bool ValidateCardPlay(ulong clientId, int cardId, int slotIndex = 0)
        {
            // Perform quick early exits for invalid conditions
            if (cardId <= 0)
            {
                Log.Warn(LogCategory.Network, $"Invalid card ID: {cardId}");
                return false;
            }
            
            // Get player's hand state
            var playerHand = GetPlayerHandState(clientId);
            if (playerHand == null)
            {
                Log.Warn(LogCategory.Network, $"Could not get hand for client: {clientId}");
                return false;
            }
            
            // Check if card is in player's hand
            var card = playerHand.Hand.Find(c => c.Id == cardId);
            if (card == null)
            {
                Log.Warn(LogCategory.Network, $"Card {cardId} not found in player's hand | Client: {clientId}");
                return false;
            }
            
            // Check if player has enough energy to play the card
            int availableEnergy = GetCurrentAvailableEnergy(clientId);
            if (card.Cost > availableEnergy)
            {
                Log.Warn(LogCategory.Network, $"Insufficient energy to play card {cardId} | Needed: {card.Cost}, Available: {availableEnergy}");
                return false;
            }
            
            // Check if slot is free (basic validation - assuming slot 0-2 for example)
            if (!IsSlotFree(slotIndex, clientId))
            {
                Log.Warn(LogCategory.Network, $"Slot {slotIndex} is not free for client: {clientId}");
                return false;
            }
            
            Log.Net($"Card validation passed | Card: {cardId} | Client: {clientId} | Slot: {slotIndex}");
            return true;
        }

        private PlayerHandState GetPlayerHandState(ulong clientId)
        {
            // For the local client, return the local hand
            if (clientId == NetworkManager.Singleton.LocalClientId)
            {
                return GameState.Instance.LocalHand;
            }
            
            // For other clients, we need to track their hand state
            // In a real implementation, we'd need to sync hand states across clients
            // For now, we'll return the local hand as a placeholder
            return GameState.Instance.LocalHand;
        }

        // Helper method to get current available energy for a player
        private int GetCurrentAvailableEnergy(ulong clientId)
        {
            // This is a simplified approach - in a real implementation, we'd need to track energy per player
            // For now, we'll return the energy cap as set in the game state
            // The actual available energy should be calculated based on played cards in the turn
            var localHand = GameState.Instance.LocalHand;
            int usedEnergy = localHand.TotalDrawnCost();
            int totalEnergy = GameState.Instance.EnergyCap;
            return totalEnergy - usedEnergy;
        }

        // Helper method to check if a slot is free
        private bool IsSlotFree(int slotIndex, ulong clientId)
        {
            // This is a placeholder - in reality, you'd need to check the board state
            // For now, we'll assume slots 0-2 are valid and check if they're within bounds
            // In a real implementation, we'd check if the slot is already occupied
            return slotIndex >= 0 && slotIndex < 3; // Assuming 3 slots for now
        }

        private void CommitCardPlay(ulong clientId, int cardId, int slotIndex)
        {
            var message = new PlayCardMessage
            {
                playerId = GetPlayerId(clientId),
                cardId = cardId,
                slotIndex = slotIndex
            };

            SendToAllClients(message);
        }

        private void HandleBroadcastCardPlayed(PlayCardMessage message)
        {
            ulong clientId = GetClientId(message.playerId);
            
            UIEventBus.Publish(new CardPlacedEvent
            {
                CardId = message.cardId,
                SlotIndex = message.slotIndex,
                OwnerClientId = clientId
            });
        }

        // Legacy method for backward compatibility
        public void RequestPlayCard(int cardId, int slotIndex)
        {
            if (!IsSpawned)
                return;

            SubmitPlayCardServerRpc(cardId, slotIndex);
        }

        // New JSON-based method
        public void RequestPlayCardJson(int cardId, int slotIndex)
        {
            if (!IsSpawned)
                return;

            var message = new PlayCardMessage
            {
                playerId = GetPlayerId(NetworkManager.Singleton.LocalClientId),
                cardId = cardId,
                slotIndex = slotIndex
            };

            SendToServer(message);
        }

        [ServerRpc(RequireOwnership = false)]
        private void SubmitPlayCardServerRpc(
            int cardId,
            int slotIndex,
            ServerRpcParams rpcParams = default)
        {
            var clientId = rpcParams.Receive.SenderClientId;

            if (!ValidateCardPlay(clientId, cardId, slotIndex))
                return;

            CommitCardPlay(clientId, cardId, slotIndex);
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
