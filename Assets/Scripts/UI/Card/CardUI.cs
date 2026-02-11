using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CardDuel.Gameplay;
using CardDuel.UI.Events;
using CardDuel.Utils;

namespace CardDuel.UI.Card
{
    public class CardUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI costText;
        [SerializeField] private TextMeshProUGUI powerText;
        [SerializeField] private Button button;

        public int CardId { get; private set; }
        private bool isInPlayArea;

        public void Bind(CardData data)
        {
            // Validate input data using utility method
            if (!ValidationUtils.ValidateCardData(data, gameObject.name))
            {
                return;
            }

            // Validate UI elements using utility method
            Object[] uiElements = { nameText, costText, powerText, button };
            if (!ValidationUtils.ValidateUIElements(uiElements, gameObject.name))
            {
                return;
            }

            CardId = data.Id;
            
            // Update UI elements - cache values to avoid repeated property access
            nameText.text = data.Name ?? "N/A";
            costText.text = data.Cost.ToString();
            powerText.text = data.Power.ToString();

            // Efficiently handle button listeners
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClicked);
        }

        public void Bind(CardData data, bool inPlayArea)
        {
            // Validate input data using utility method
            if (!ValidationUtils.ValidateCardData(data, gameObject.name))
            {
                return;
            }

            // Validate UI elements using utility method
            Object[] uiElements = { nameText, costText, powerText, button };
            if (!ValidationUtils.ValidateUIElements(uiElements, gameObject.name))
            {
                return;
            }

            CardId = data.Id;
            isInPlayArea = inPlayArea;
            
            // Update UI elements - cache values to avoid repeated property access
            nameText.text = data.Name ?? "N/A";
            costText.text = data.Cost.ToString();
            powerText.text = data.Power.ToString();

            // Efficiently handle button listeners
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClicked);
        }

        private void OnClicked()
        {
            if (isInPlayArea)
            {
                UIEventBus.Publish(new CardDeselectedEvent
                {
                    CardId = CardId
                });
            }
            else
            {
                UIEventBus.Publish(new CardSelectedEvent
                {
                    CardId = CardId
                });
            }
        }

        public void SetInPlayArea(bool inPlayArea)
        {
            isInPlayArea = inPlayArea;
        }

        /// <summary>
        /// Sets the visual state of the card based on whether it's in play area
        /// </summary>
        /// <param name="inPlayArea">True if card is in play area, false otherwise</param>
        public void UpdateVisualState(bool inPlayArea)
        {
            isInPlayArea = inPlayArea;
            // Optionally add visual feedback here (outline, color change, etc.)
        }
    }
}
