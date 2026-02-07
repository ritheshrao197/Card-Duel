using UnityEngine;
using CardDuel.Networking.Netcode;
using CardDuel.Networking.Payloads;
using CardDuel.Networking;
namespace CardDuel.UI.Game
{
    public class LeaveMatchButtonUI : MonoBehaviour
    {
        public void OnLeaveMatchClicked()
        {
            NetworkClient.Send(new NetworkMessage
            {
                action = "leaveMatch",
                payload = JsonUtility.ToJson(new LeaveMatchPayload())
            });
        }
    }
}
