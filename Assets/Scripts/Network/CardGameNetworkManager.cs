using Unity.Netcode;
using UnityEngine;

namespace CardDuel.Networking.Managers
{
    public class CardGameNetworkManager : NetworkManager
{
    public static CardGameNetworkManager Instance { get; private set; }
    
    [Header("Game Settings")]
    public int TotalTurns = 6;
    public int StartingHandSize = 3;
    public float TurnTimeLimit = 30f;
    
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        Instance = this;
    }
    
    public void StartMatchmaking()
    {
        // Host or join lobby
        if (NetworkManager.Singleton.IsHost)
        {
            NetworkManager.Singleton.StartHost();
        }
        else if (NetworkManager.Singleton.IsServer)
        {
            NetworkManager.Singleton.StartServer();
        }
        else
        {
            NetworkManager.Singleton.StartClient();
        }
    }
    
    public void HandleReconnection()
    {
        // Logic to restore game state for reconnecting players
    }
}
}