using UnityEngine;
using CardDuel.Networking.Netcode;
using TMPro;


namespace CardDuel.UI.DebugTest
{
    public class MultiplayerTestUI : MonoBehaviour
    {
        [Header("UI")]
        public TextMeshProUGUI statusText;

        private NetworkBootstrap _bootstrap;

        private void Awake()
        {
            _bootstrap = FindObjectOfType<NetworkBootstrap>();
        }

        // -------- NETWORK --------

        public void StartHost()
        {
            statusText.text = "Starting Host...";
            _bootstrap.StartHost();
        }

        public void StartClient()
        {
            statusText.text = "Starting Client...";
            _bootstrap.StartClient();
        }

        // -------- MATCHMAKING --------

        public void SetReady()
        {
            statusText.text = "Sending Ready...";
            NetworkClient.SendPlayerReady();
        }

        // -------- MESSAGING --------

        public void SendTest()
        {
            statusText.text = "Sending Test JSON...";
            NetworkClient.SendTest();
        }

        // -------- TURN CONTROL --------

        public void ForceTurn()
        {
            if (!IsServer())
            {
                statusText.text = "ForceTurn: Server only";
                return;
            }

            statusText.text = "Forcing Turn Start";
            NetworkGameController.Instance.StartTurn(
                NetworkGameController.Instance.GetCurrentTurn() + 1
            );
        }

        public void ForceTimeout()
        {
            if (!IsServer())
            {
                statusText.text = "Timeout: Server only";
                return;
            }

            statusText.text = "Forcing Turn Timeout";
            // NetworkGameController.Instance.ForceEndTurn();
        }

        private bool IsServer()
        {
            return NetworkGameController.Instance != null &&
                   NetworkGameController.Instance.IsServer;
        }
    }
}


namespace CardDuel.UI.Game
{
}
