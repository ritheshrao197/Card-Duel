using CardDuel.UI.Events;

public struct CardPlayRequestedEvent : IUIEvent
{
    public int CardId;
    public int SlotIndex;
}