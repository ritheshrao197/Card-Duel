using System;
using CardDuel.Events;
using UnityEngine;

namespace CardDuel.UI
{
    public class TurnDebugUI : MonoBehaviour
    {
        private void OnEnable()
        {
            EventBus.Subscribe<TurnStartEvent>(OnTurnStart);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<TurnStartEvent>(OnTurnStart);
        }

        private void OnTurnStart(TurnStartEvent e)
        {
            Debug.Log($"[CLIENT] Turn {e.currentTurn} started. Cost={e.availableCost}");
        }
    }
}
