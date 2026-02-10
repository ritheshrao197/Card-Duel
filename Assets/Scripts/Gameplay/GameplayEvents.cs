namespace CardDuel.UI.Events
{
    public struct CardSelectedEvent : IUIEvent
    {
        public int CardId;
    }

    public struct CardDeselectedEvent : IUIEvent
    {
        public int CardId;
    }
}
