using TMPro;
using CardDuel.UI.Core;
using CardDuel.UI.Events;
using UnityEngine;
using UnityEngine.UI;
using CardDuel.Utils;

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

            // 🔑 Button listener
            endTurnButton.onClick.AddListener(OnEndTurnClicked);

            Log.UI("GameplayPanel events registered", this);
        }

        protected override void UnregisterEvents()
        {
            UIEventBus.Unsubscribe<TurnStartedEvent>(OnTurnStarted);
            UIEventBus.Unsubscribe<TurnTimerTickEvent>(OnTimerTick);
            UIEventBus.Unsubscribe<LocalTurnLockedEvent>(OnTurnLocked);

            endTurnButton.onClick.RemoveListener(OnEndTurnClicked);

            Log.UI("GameplayPanel events unregistered", this);
        }

        private void OnTurnStarted(TurnStartedEvent evt)
        {
            turnText.text = $"Turn {evt.Turn}/6";
            energyText.text = $"{evt.Energy}/{evt.MaxEnergy}";
            endTurnButton.interactable = true;

            Log.UI(
                $"TurnStarted UI update | Turn={evt.Turn} | Energy={evt.Energy}/{evt.MaxEnergy}",
                this);
        }

        private void OnTimerTick(TurnTimerTickEvent evt)
        {
            timerFill.fillAmount = evt.Remaining / 30f;
        }

        private void OnTurnLocked(LocalTurnLockedEvent evt)
        {
            endTurnButton.interactable = false;

            Log.UI("End Turn button locked", this);
        }

        private void OnEndTurnClicked()
        {
            Log.UI("End Turn button clicked", this);

            endTurnButton.interactable = false; // immediate UI feedback

            UIEventBus.Publish(new EndTurnClickedEvent());
        }
    }
}
