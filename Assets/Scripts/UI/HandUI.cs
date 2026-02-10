using UnityEngine;
using CardDuel.Core;
using CardDuel.Gameplay.Events;

public class HandUI : MonoBehaviour
{
    private void Awake()
    {
        EventBus.Subscribe<CardPlayedEvent>(OnCardPlayed);
    }

    private void OnCardPlayed(CardPlayedEvent evt)
    {
        Debug.Log($"Player {evt.PlayerId} played card {evt.CardId}");
    }
}
