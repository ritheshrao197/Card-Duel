using CardDuel.UI.Core;
using CardDuel.UI.Events;


namespace CardDuel.UI.Panels
{
    public class NetworkWaitingOverlay : UIPanel
    {
        protected override void RegisterEvents()
        {
            UIEventBus.Subscribe<WaitingForOpponentEvent>(OnWaiting);
        }

        protected override void UnregisterEvents()
        {
            UIEventBus.Unsubscribe<WaitingForOpponentEvent>(OnWaiting);
        }

        private void OnWaiting(WaitingForOpponentEvent evt)
        {
            if (evt.IsWaiting) Show();
            else Hide();
        }
    }
}
