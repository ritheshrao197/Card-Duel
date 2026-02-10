using CardDuel.UI.Core;
using CardDuel.UI.Events;
using CardDuel.Core;

public class MainMenuPanel : UIPanel
{
    public override UIState State => UIState.MainMenu;

    public void OnHostClicked()
    {
        UIEventBus.Publish(new NetworkUIIntent.HostRequested());
    }

    public void OnJoinClicked()
    {
        UIEventBus.Publish(new NetworkUIIntent.ClientRequested());
    }
}
