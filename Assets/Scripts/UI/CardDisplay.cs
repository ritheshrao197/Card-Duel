using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
using CardDuel.Core;
using CardDuel.Gameplay.Events;

namespace CardDuel.UI.Components
{
    public class CardDisplay : MonoBehaviour
{
    [Header("UI References")]
    public Image CardArt;
    public TextMeshProUGUI CardNameText;
    public TextMeshProUGUI CostText;
    public TextMeshProUGUI PowerText;
    public TextMeshProUGUI AbilityText;
    public Button CardButton;
    
    private Card cardData;
    private bool isSelected = false;
    
    public void Initialize(Card card)
    {
        cardData = card;
        
        CardNameText.text = card.Name;
        CostText.text = card.Cost.ToString();
        PowerText.text = card.Power.ToString();
        
        if (card.Ability != null)
        {
            AbilityText.text = $"{card.Ability.Type}: {card.Ability.Value}";
        }
        else
        {
            AbilityText.text = "No Ability";
        }
        
        CardButton.onClick.AddListener(OnCardClicked);
    }
    
    private void OnCardClicked()
    {
        // Toggle selection
        isSelected = !isSelected;
        
        // Visual feedback for selection
        GetComponent<Image>().color = isSelected ? Color.yellow : Color.white;
        
        // Send event through the existing event system
        var cardPlayedEvent = new CardDuel.Gameplay.Events.CardPlayedEvent
        {
            PlayerId = NetworkManager.Singleton.LocalClientId,
            CardId = cardData.Id
        };
        
        EventBus.Publish(cardPlayedEvent);
    }
    
    public Card GetCardData()
    {
        return cardData;
    }
}
}