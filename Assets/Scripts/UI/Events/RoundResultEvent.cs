namespace CardDuel.UI.Events
{
    public struct RoundResultEvent : IUIEvent
    {
        public int PlayerAScore;
        public int PlayerBScore;
    }
}
