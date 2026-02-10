namespace CardDuel.UI.Events
{
    public struct CardRevealEvent : IUIEvent
    {
        public ulong OwnerClientId;
        public int CardId;
    }

    public struct ScoreUpdatedEvent : IUIEvent
    {
        public ulong PlayerId;
        public int NewScore;
        public int Delta;
    }

    public struct RevealFinishedEvent : IUIEvent { }
}
