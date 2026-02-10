using System.Collections.Generic;

namespace CardDuel.Gameplay
{
    public class RuntimeCardDatabase
    {
        private readonly Dictionary<int, CardData> _lookup = new();
        private readonly List<CardData> _allCards = new();

        public RuntimeCardDatabase(List<CardData> cards)
        {
            foreach (var card in cards)
            {
                _lookup[card.Id] = card;
                _allCards.Add(card);
            }
        }

        public CardData GetById(int id) => _lookup[id];

        public IReadOnlyList<CardData> AllCards => _allCards;
    }
}
