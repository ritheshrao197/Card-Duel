using System.Collections.Generic;
using UnityEngine;
using CardDuel.Domain.Models;

namespace CardDuel.Domain.Data
{
    [CreateAssetMenu(fileName = "CardDatabase", menuName = "ScriptableObjects/CardDatabase")]
    public class CardDatabase : ScriptableObject
{
    [SerializeField] private List<Card> cardList = new List<Card>();
    
    public Card GetCardById(int id)
    {
        return cardList.Find(card => card.Id == id);
    }
    
    public void LoadFromJson(string jsonPath)
    {
        // Load card definitions from JSON file
        TextAsset textAsset = Resources.Load<TextAsset>(jsonPath);
        if (textAsset != null)
        {
            CardDataWrapper wrapper = JsonUtility.FromJson<CardDataWrapper>(textAsset.text);
            cardList.Clear();
            cardList.AddRange(wrapper.cards);
        }
    }
    
    [System.Serializable]
    private class CardDataWrapper
    {
        public Card[] cards;
    }
}
}