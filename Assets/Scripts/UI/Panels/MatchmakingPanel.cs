using CardDuel.UI.Core;
using CardDuel.UI.Events;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CardDuel.UI.Panels
{
    public class MatchmakingPanel : UIPanel
    {
        [SerializeField] private Button readyButton;
        [SerializeField] private TextMeshProUGUI statusText;

        protected override void RegisterEvents()
        {
            readyButton.onClick.AddListener(OnReadyClicked);
            UIEventBus.Subscribe<BothPlayersReadyEvent>(OnBothReady);
        }

        protected override void UnregisterEvents()
        {
            readyButton.onClick.RemoveListener(OnReadyClicked);
            UIEventBus.Unsubscribe<BothPlayersReadyEvent>(OnBothReady);
        }

        private void OnReadyClicked()
        {
            readyButton.interactable = false;
            statusText.text = "Waiting for opponent...";

            UIEventBus.Publish(new PlayerReadyClickedEvent());
        }

        private void OnBothReady(BothPlayersReadyEvent evt)
        {
            UIEventBus.Publish(new UIStateChangedEvent
            {
                State = UIState.Gameplay
            });
        }
    }
}
