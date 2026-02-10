using UnityEngine;
using Unity.Netcode;
using CardDuel.Gameplay.Commands;
using CardDuel.Networking;
using CardDuel.Gameplay.Events;

public class GameplayInputHandler : MonoBehaviour
{
    [SerializeField] private CommandSender sender;

    public void OnCardClicked(int cardId)
    {
        sender.Send(new PlayCardCommand
        {
            PlayerId = NetworkManager.Singleton.LocalClientId,
            CardId = cardId
        });
    }

    public void OnEndTurnClicked()
    {
        sender.Send(new EndTurnCommand
        {
            PlayerId = NetworkManager.Singleton.LocalClientId
        });
    }
}
