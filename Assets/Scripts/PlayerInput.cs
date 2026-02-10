using UnityEngine;
using Unity.Netcode;
using CardDuel.Gameplay.Commands;
using CardDuel.Networking;

public class PlayerInput : MonoBehaviour
{
    [SerializeField] private CommandSender sender;

    public void PlayCard(int cardId)
    {
        sender.Send(new PlayCardCommand
        {
            PlayerId = NetworkManager.Singleton.LocalClientId,
            CardId = cardId
        });
    }
}
