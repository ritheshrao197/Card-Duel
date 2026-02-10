using System.Collections.Generic;
using UnityEngine;

namespace CardDuel.Gameplay
{
    [CreateAssetMenu(menuName = "Card Duel/Card Database")]
    public class CardDatabase : ScriptableObject
    {
        [SerializeField] private List<CardData> cards;

        private Dictionary<int, CardData> _lookup;

        public IReadOnlyList<CardData> AllCards => cards;

        private void OnEnable()
        {
            _lookup = new Dictionary<int, CardData>();
            foreach (var card in cards)
                _lookup[card.Id] = card;
        }

        public CardData GetById(int id)
        {
            return _lookup[id];
        }
    }
}
