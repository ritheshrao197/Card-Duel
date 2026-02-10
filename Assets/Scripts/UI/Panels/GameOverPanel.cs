using UnityEngine;
using TMPro;
using CardDuel.UI.Core;
using CardDuel.Core;
using CardDuel.Gameplay.Events;
using Unity.Netcode;

namespace CardDuel.UI.GameOver
{
    public class GameOverPanel : UIPanel
    {
        public override UIState State => UIState.GameOver;

        [SerializeField] private UIManager uiManager;
        [SerializeField] private TextMeshProUGUI resultText;

        protected override void OnShow()
        {
            EventBus.Subscribe<MatchEndedEvent>(OnMatchEnded);
        }

        private void OnMatchEnded(MatchEndedEvent evt)
        {
            var localId = NetworkManager.Singleton.LocalClientId;
            resultText.text = evt.WinnerPlayerId == localId
                ? "YOU WIN!"
                : "YOU LOSE!";
        }

        public void OnPlayAgainClicked()
        {
            uiManager.Show(UIState.Matchmaking);
        }

        public void OnExitClicked()
        {
            uiManager.Show(UIState.MainMenu);
        }
    }
}
