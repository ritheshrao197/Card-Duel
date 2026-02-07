using System.Collections.Generic;
using CardDuel.Cards;

namespace CardDuel.Core
{
    public class GameState
    {
        public int currentTurn;
        public Dictionary<string, PlayerState> players = new();
    }
}
