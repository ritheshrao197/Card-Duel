using System;

namespace CardDuel.Core.Events
{
    public static class GameEvents
{
    // Game Flow Events
    public static event Action GameStart;
    public static event Action<int> TurnStart; // turn number
    public static event Action<ulong> PlayerEndedTurn; // player id
    public static event Action AllPlayersReady;
    public static event Action<ulong, int, int> RevealCard; // player id, card id, order index
    public static event Action<ulong, int> ScoreUpdated; // player id, new score
    public static event Action TurnEnd;
    public static event Action<int, ulong> GameEnd; // winning player id
    
    // Card System Events
    public static event Action<ulong, int> CardPlayed; // player id, card id
    public static event Action<ulong, int> CardSelected; // player id, card id
    public static event Action<ulong, int> CardDeselected; // player id, card id
    
    // Network Events
    public static event Action<ulong> PlayerConnected;
    public static event Action<ulong> PlayerDisconnected;
    public static event Action<ulong> PlayerReconnected;
    
    // Triggers
    public static void TriggerGameStart() => GameStart?.Invoke();
    public static void TriggerTurnStart(int turn) => TurnStart?.Invoke(turn);
    public static void TriggerPlayerEndedTurn(ulong playerId) => PlayerEndedTurn?.Invoke(playerId);
    public static void TriggerAllPlayersReady() => AllPlayersReady?.Invoke();
    public static void TriggerRevealCard(ulong playerId, int cardId, int orderIndex) => 
        RevealCard?.Invoke(playerId, cardId, orderIndex);
    public static void TriggerScoreUpdated(ulong playerId, int newScore) => 
        ScoreUpdated?.Invoke(playerId, newScore);
    public static void TriggerTurnEnd() => TurnEnd?.Invoke();
    public static void TriggerGameEnd(int turnNumber, ulong winningPlayerId) => 
        GameEnd?.Invoke(turnNumber, winningPlayerId);
    public static void TriggerCardPlayed(ulong playerId, int cardId) => 
        CardPlayed?.Invoke(playerId, cardId);
    public static void TriggerCardSelected(ulong playerId, int cardId) => 
        CardSelected?.Invoke(playerId, cardId);
    public static void TriggerCardDeselected(ulong playerId, int cardId) => 
        CardDeselected?.Invoke(playerId, cardId);
    public static void TriggerPlayerConnected(ulong playerId) => 
        PlayerConnected?.Invoke(playerId);
    public static void TriggerPlayerDisconnected(ulong playerId) => 
        PlayerDisconnected?.Invoke(playerId);
    public static void TriggerPlayerReconnected(ulong playerId) => 
        PlayerReconnected?.Invoke(playerId);
}
}