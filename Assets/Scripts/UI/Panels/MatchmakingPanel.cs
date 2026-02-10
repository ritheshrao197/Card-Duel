using CardDuel.UI.Core;
using CardDuel.Networking.Events;
using CardDuel.Core;

namespace CardDuel.UI.Panels
{
    public class MatchmakingPanel : UIPanel
    {
        public override UIState State => UIState.Matchmaking;

        protected override void OnShow()
        {
            EventBus.Subscribe<ConnectedToServerEvent>(_ =>
            {
                uiManager.Show(UIState.Gameplay);
            });
        }
    }
}
