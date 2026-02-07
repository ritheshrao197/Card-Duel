namespace CardDuel.UI.Core
{
    public enum UIState
    {
        None,

        // Pre-game
        MainMenu,
        Matchmaking,
        Lobby,

        // In-game
        Gameplay,
        PendingGameOver,

        // Post-game
        GameOver,

        // Special
        Spectator
    }
}
