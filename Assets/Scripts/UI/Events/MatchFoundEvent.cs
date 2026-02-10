namespace CardDuel.UI.Events
{
    public struct MatchFoundEvent : IUIEvent
    {
        public string PlayerA;
        public string PlayerB;
    }
}
