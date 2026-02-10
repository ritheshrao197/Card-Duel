using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using CardDuel.UI.Events;

namespace CardDuel.Networking
{
    public class NetworkSceneController : MonoBehaviour
    {
        private void OnEnable()
        {
            if (NetworkManager.Singleton == null)
                return;

            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnSceneLoaded;
        }

        private void OnDisable()
        {
            if (NetworkManager.Singleton == null)
                return;

            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= OnSceneLoaded;
        }

        public void LoadSceneForAll(string sceneName)
        {
            if (!NetworkManager.Singleton || !NetworkManager.Singleton.IsServer)
                return;

            UIEventBus.Publish(new SceneLoadStartedEvent
            {
                SceneName = sceneName
            });

            NetworkManager.Singleton.SceneManager.LoadScene(
                sceneName,
                LoadSceneMode.Single
            );
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
