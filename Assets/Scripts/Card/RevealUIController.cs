using CardDuel.Gameplay;
using CardDuel.UI.Events;
using UnityEngine;

public class RevealUIController : MonoBehaviour
{
    [SerializeField] private CardDatabase cardDatabase;

    private void OnEnable()
    {
        UIEventBus.Subscribe<CardRevealEvent>(OnReveal);
    }

    private void OnReveal(CardRevealEvent evt)
    {
        var card = cardDatabase.GetById(evt.CardId);

        // animate flip
        // show name, power, ability
    }
}
