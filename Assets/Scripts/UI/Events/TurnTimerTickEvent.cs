namespace CardDuel.UI.Events
{
    public struct TurnTimerTickEvent : IUIEvent
    {
        public float Remaining;
    }

    public struct RevealPhaseStartedEvent : IUIEvent
    {
        public bool PlayerHasInitiative;
    }

    public struct CardResolvedEvent : IUIEvent
    {
        public string CardName;
        public int ScoreDelta;
    }
    public struct GameEndedEvent : IUIEvent
    {
        public bool IsVictory;
        public int PlayerScore;
        public int OpponentScore;
    }
    public struct WaitingForOpponentEvent : IUIEvent
    {
        public bool IsWaiting;
    }
}
