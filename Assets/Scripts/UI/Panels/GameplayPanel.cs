using TMPro;
using CardDuel.UI.Core;
using CardDuel.UI.Events;
using UnityEngine;
using UnityEngine.UI;

namespace CardDuel.UI.Panels
{
    public class GameplayPanel : UIPanel
    {
        [SerializeField] private TextMeshProUGUI turnText;
        [SerializeField] private TextMeshProUGUI energyText;
        [SerializeField] private Image timerFill;
        [SerializeField] private Button endTurnButton;

        protected override void RegisterEvents()
        {
            UIEventBus.Subscribe<TurnStartedEvent>(OnTurnStarted);
            UIEventBus.Subscribe<TurnTimerTickEvent>(OnTimerTick);
            UIEventBus.Subscribe<LocalTurnLockedEvent>(OnTurnLocked);
        }

        protected override void UnregisterEvents()
        {
            UIEventBus.Unsubscribe<TurnStartedEvent>(OnTurnStarted);
            UIEventBus.Unsubscribe<TurnTimerTickEvent>(OnTimerTick);
            UIEventBus.Unsubscribe<LocalTurnLockedEvent>(OnTurnLocked);
        }

        private void OnTurnStarted(TurnStartedEvent evt)
        {
            turnText.text = $"Turn {evt.Turn}/6";
            energyText.text = $"{evt.Energy}/{evt.MaxEnergy}";
            endTurnButton.interactable = true;
        }

        private void OnTimerTick(TurnTimerTickEvent evt)
        {
            timerFill.fillAmount = evt.Remaining / 30f;
        }

        private void OnTurnLocked(LocalTurnLockedEvent evt)
        {
            endTurnButton.interactable = false;
        }
    }
}