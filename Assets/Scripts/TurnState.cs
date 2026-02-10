namespace CardDuel.Domain
{
    public class TurnState
    {
        public ulong CurrentPlayerId { get; private set; }
        public TurnPhase Phase { get; private set; }

        public void StartTurn(ulong playerId)
        {
            CurrentPlayerId = playerId;
            Phase = TurnPhase.Start;
        }

        public void AdvancePhase()
        {
            Phase = Phase switch
            {
                TurnPhase.Start => TurnPhase.Main,
                TurnPhase.Main => TurnPhase.End,
                TurnPhase.End => TurnPhase.None,
                _ => TurnPhase.None
            };
        }
    }
}
