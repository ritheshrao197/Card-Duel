using UnityEngine;
using CardDuel.UI.Core;
using CardDuel.Core;
using CardDuel.Gameplay.Events;

namespace CardDuel.UI.Gameplay
{
    public class GameplayPanel : UIPanel
    {
        public override UIState State => UIState.Gameplay;

        [SerializeField] private UIManager uiManager;

        protected override void OnShow()
        {
            EventBus.Subscribe<MatchEndedEvent>(OnMatchEnded);
        }

        protected override void OnHide()
        {
            // Optional: unsubscribe if you add that support
        }

        private void OnMatchEnded(MatchEndedEvent evt)
        {
            uiManager.Show(UIState.GameOver);
        }
    }
}
