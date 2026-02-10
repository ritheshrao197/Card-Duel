namespace CardDuel.UI.Events
{
    public struct TurnStartedEvent : IUIEvent
    {
        public int Turn;
        public int Energy;
        public int MaxEnergy;
    }
}
