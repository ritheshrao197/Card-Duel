using UnityEngine;
using CardDuel.UI.Core;
using CardDuel.UI.Events;
using Unity.VisualScripting;

namespace CardDuel.UI.Management
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private UIPanel matchmakingPanel;
        [SerializeField] private UIPanel gameplayPanel;
        [SerializeField] private UIPanel revealPanel;
        [SerializeField] private UIPanel resultsPanel;
        [SerializeField] private UIPanel mainMenuPanel;


        private void OnEnable()
        {
            UIEventBus.Subscribe<UIStateChangedEvent>(OnStateChanged);
        }

        private void OnDisable()
        {
            UIEventBus.Unsubscribe<UIStateChangedEvent>(OnStateChanged);
        }
        private void Start()
        {
            UIEventBus.Publish(new UIStateChangedEvent
            {
                State = UIState.MainMenu
            });

        }

        private void OnStateChanged(UIStateChangedEvent evt)
        {
            mainMenuPanel.Hide();
            matchmakingPanel.Hide();
            gameplayPanel.Hide();
            revealPanel.Hide();
            resultsPanel.Hide();

            switch (evt.State)
            {
                case UIState.MainMenu:
                    mainMenuPanel.Show();
                    break;
                case UIState.Matchmaking:
                    matchmakingPanel.Show();
                    break;
                case UIState.Gameplay:
                    gameplayPanel.Show();
                    UIEventBus.Publish(new GameplayPanelShownEvent());
                    break;
                case UIState.Reveal:
                    revealPanel.Show();
                    break;
                case UIState.Results:
                    resultsPanel.Show();
                    break;
             

            }
        }
    }
}
