using System;
using UnityEngine;
namespace CardDuel.Cards 
{
[CreateAssetMenu(menuName = "Card Duel/Card")]
public class CardDefinition : ScriptableObject
{
    public int id;
    public string cardName;
    public int cost;
    public int power;
    public AbilityDefinition ability;
}
}