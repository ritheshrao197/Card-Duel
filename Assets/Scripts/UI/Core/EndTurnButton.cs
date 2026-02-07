using UnityEngine;
using CardDuel.Events;
namespace CardDuel.UI.Input
{
    public class EndTurnButton : MonoBehaviour
    {
        public void OnClick()
        {
            EventBus.Publish(new EndTurnPressedEvent());
        }
    }
}
