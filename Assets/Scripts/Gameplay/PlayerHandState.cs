
using System.Collections.Generic;

namespace CardDuel.Gameplay
{
    public class PlayerHandState
    {
        public List<CardData> Hand = new();
        public List<CardData> Drawn = new();

        public int TotalDrawnCost()
        {
            int cost = 0;
            foreach (var c in Drawn)
                cost += c.Cost;
            return cost;
        }
    }
}
