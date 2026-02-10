
using UnityEngine;
using System.Collections.Generic;

namespace CardDuel.Gameplay
{
    public static class CardJsonLoader
    {
        public static List<CardData> LoadCardsFromResources(string fileName)
        {
            TextAsset jsonAsset = Resources.Load<TextAsset>(fileName);

            if (jsonAsset == null)
            {
                Debug.LogError($"Card JSON not found in Resources: {fileName}");
                return null;
            }

            // JsonUtility needs a wrapper
            string wrappedJson = $"{{\"cards\":{jsonAsset.text}}}";
            CardJsonList jsonList = JsonUtility.FromJson<CardJsonList>(wrappedJson);

            var result = new List<CardData>();

            foreach (var json in jsonList.cards)
            {
                result.Add(new CardData
                {
                    Id = json.id,
                    Name = json.cardName,
                    Cost = json.cost,
                    Power = json.power,
                    Ability = new CardAbility
                    {
                        Type = json.ability.type,
                        Value = json.ability.value
                    }
                });
            }

            return result;
        }
    }
}
