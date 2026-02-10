using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using CardDuel.UI.Events;

namespace CardDuel.Networking
{
    public class NetworkSceneController : MonoBehaviour
    {
        private bool _isRegistered;

        private void Update()
        {
            TryRegister();
        }

        private void OnDisable()
        {
            Unregister();
        }

        private void TryRegister()
        {
            if (_isRegistered)
                return;

            if (NetworkManager.Singleton == null)
                return;

            if (!NetworkManager.Singleton.IsListening)
                return;

            if (NetworkManager.Singleton.SceneManager == null)
                return;

            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnSceneLoaded;
            _isRegistered = true;
        }

        private void Unregister()
        {
            if (!_isRegistered)
                return;

            if (NetworkManager.Singleton == null)
                return;

            if (NetworkManager.Singleton.SceneManager == null)
                return;

            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= OnSceneLoaded;
            _isRegistered = false;
        }

        public void LoadSceneForAll(string sceneName)
        {
            if (NetworkManager.Singleton == null)
                return;

            if (!NetworkManager.Singleton.IsServer)
                return;

            NetworkManager.Singleton.SceneManager.LoadScene(
                sceneName,
                LoadSceneMode.Single
            );

            UIEventBus.Publish(new SceneLoadStartedEvent
            {
                SceneName = sceneName
            });
        }

        private void OnSceneLoaded(
            string sceneName,
            LoadSceneMode mode,
            System.Collections.Generic.List<ulong> clientsCompleted,
            System.Collections.Generic.List<ulong> clientsTimedOut)
        {
            UIEventBus.Publish(new SceneLoadCompletedEvent
            {
                SceneName = sceneName
            });
        }
    }
}