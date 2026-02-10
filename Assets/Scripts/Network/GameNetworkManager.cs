using UnityEngine;
using Unity.Netcode;
using System;

namespace CardDuel.Networking
{
    public class GameNetworkManager : MonoBehaviour
    {
        public static GameNetworkManager Instance { get; private set; }

        public bool IsServer => NetworkManager.Singleton.IsServer;
        public bool IsClient => NetworkManager.Singleton.IsClient;
        public bool IsHost => NetworkManager.Singleton.IsHost;

        public ulong LocalClientId =>
            NetworkManager.Singleton.LocalClientId;

        // --------------------
        // Events (UI-friendly)
        // --------------------

        public event Action OnServerStarted;
        public event Action OnClientConnected;
        public event Action<ulong> OnClientDisconnected;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnEnable()
        {
            var nm = NetworkManager.Singleton;

            nm.OnServerStarted += HandleServerStarted;
            nm.OnClientConnectedCallback += HandleClientConnected;
            nm.OnClientDisconnectCallback += HandleClientDisconnected;
        }

        private void OnDisable()
        {
            if (NetworkManager.Singleton == null) return;

            var nm = NetworkManager.Singleton;

            nm.OnServerStarted -= HandleServerStarted;
            nm.OnClientConnectedCallback -= HandleClientConnected;
            nm.OnClientDisconnectCallback -= HandleClientDisconnected;
        }

        // --------------------
        // Public API (UI calls)
        // --------------------

        public void StartHost()
        {
            Debug.Log("[Network] Starting Host");
            NetworkManager.Singleton.StartHost();
        }

        public void StartServer()
        {
            Debug.Log("[Network] Starting Server");
            NetworkManager.Singleton.StartServer();
        }

        public void StartClient()
        {
            Debug.Log("[Network] Starting Client");
            NetworkManager.Singleton.StartClient();
        }

        public void Shutdown()
        {
            if (!NetworkManager.Singleton.IsListening) return;

            Debug.Log("[Network] Shutdown");
            NetworkManager.Singleton.Shutdown();
        }

        // --------------------
        // NGO callbacks
        // --------------------

        private void HandleServerStarted()
        {
            Debug.Log("[Network] Server Started");
            OnServerStarted?.Invoke();
        }

        private void HandleClientConnected(ulong clientId)
        {
            Debug.Log($"[Network] Client Connected: {clientId}");

            if (clientId == LocalClientId)
                OnClientConnected?.Invoke();
        }

        private void HandleClientDisconnected(ulong clientId)
        {
            Debug.Log($"[Network] Client Disconnected: {clientId}");
            OnClientDisconnected?.Invoke(clientId);
        }
    }
}
