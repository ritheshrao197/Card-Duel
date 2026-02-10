namespace CardDuel.UI.Events
{
    public interface IUIEvent { }
}
namespace CardDuel.UI.Events
{
    public struct CardAddedToHandEvent : IUIEvent
    {
        public int CardId;

        public ulong TargetClientId ;
    }
    
    public struct GameplayPanelShownEvent : IUIEvent { }
    public struct HandStateChangedEvent : IUIEvent { }

    public struct CardMovedEvent : IUIEvent
    {
        public int CardId;
        public bool ToDrawnArea;

        public CardMovedEvent(int cardId, bool toDrawnArea) : this()
        {
            CardId = cardId;
            ToDrawnArea = toDrawnArea;
        }
    }
}
