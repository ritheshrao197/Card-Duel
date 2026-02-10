namespace CardDuel.Gameplay
{
    public class GameState
    {
        public static GameState Instance { get; private set; }
        public int CurrentRound { get; private set; } = 1;

        public int EnergyCap => CurrentRound;

        public PlayerHandState LocalHand { get; } = new();

        public GameState()
        {
            Instance = this;
        }
       
        public void Reset()
        {
            CurrentRound = 1;
        }

        public void NextRound()
        {
            CurrentRound++;
        }
    }
}
