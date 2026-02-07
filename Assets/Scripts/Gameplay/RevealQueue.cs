using System.Collections.Generic;
using CardDuel.Cards;
using CardDuel.Core;
namespace CardDuel.Gameplay.Reveal
{
    public class RevealQueue
    {
        public Queue<(PlayerState, CardInstance)> queue = new();

        public void Enqueue(PlayerState player, CardInstance card)
        {
            queue.Enqueue((player, card));
        }

        public bool HasNext() => queue.Count > 0;

        public (PlayerState, CardInstance) Next() => queue.Dequeue();
    }
}

