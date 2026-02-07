using System.Collections.Generic;

namespace CardDuel.Networking.Sync
{
    [System.Serializable]
    public class GameStateSnapshot
    {
        public int currentTurn;
        public string phase;
        public float remainingTurnTime;
        public string localPlayerId;

        public Dictionary<string, int> scores = new();
        public Dictionary<string, int> foldedCardCounts = new();
    }
}
