using TMPro;
using CardDuel.Events;
using CardDuel.UI.Core;
using UnityEngine;


namespace CardDuel.UI.Game
{
    public class SpectatorPanel : UIPanel
    {
        [SerializeField] private TextMeshProUGUI infoText;

        public override void SubscribeToEvents()
        {
            EventBus.Subscribe<SpectatorModeEvent>(OnSpectator);
        }

        public override void UnsubscribeFromEvents()
        {
            EventBus.Unsubscribe<SpectatorModeEvent>(OnSpectator);
        }

        private void OnSpectator(SpectatorModeEvent e)
        {
            infoText.text = "Spectating ongoing match";
            UIManager.Instance.SwitchTo(UIState.Spectator);
        }
    }
}
