using CardDuel.UI.Core;
using CardDuel.UI.Events;
using UnityEngine;
using UnityEngine.UI;


namespace CardDuel.UI.Panels
{
    public class MainMenuPanel : UIPanel
    {
        [SerializeField] private Button hostButton;
        [SerializeField] private Button joinButton;

        protected override void RegisterEvents()
        {
            hostButton.onClick.AddListener(OnHostClicked);
            joinButton.onClick.AddListener(OnJoinClicked);
        }

        protected override void UnregisterEvents()
        {
            hostButton.onClick.RemoveListener(OnHostClicked);
            joinButton.onClick.RemoveListener(OnJoinClicked);
        }

        private void OnHostClicked()
        {
            UIEventBus.Publish(new ConnectRequestedEvent
            {
                Role = NetworkRole.Host
            });

            UIEventBus.Publish(new UIStateChangedEvent
            {
                State = UIState.Matchmaking
            });
        }

        private void OnJoinClicked()
        {
            UIEventBus.Publish(new ConnectRequestedEvent
            {
                Role = NetworkRole.Client
            });

            UIEventBus.Publish(new UIStateChangedEvent
            {
                State = UIState.Matchmaking
            });
        }
    }
}
