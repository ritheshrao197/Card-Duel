namespace CardDuel.UI.Events
{
    public struct CardPlacedEvent : IUIEvent
    {
        public int CardId;
        public int SlotIndex;
        public ulong OwnerClientId;
    }
}
