using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CardDuel.Domain.Commands;
using CardDuel.Infrastructure.Networking;

namespace CardDuel.Presentation.UI.Components
{
    /// <summary>
    /// Reactive card display component
    /// Listens to events, emits intents, never validates
    /// </summary>
    public class CardDisplay : MonoBehaviour
    {
        [Header("UI References")]
        public Image CardArt;
        public TextMeshProUGUI CardNameText;
        public TextMeshProUGUI CostText;
        public TextMeshProUGUI PowerText;
        public TextMeshProUGUI AbilityText;
        public Button CardButton;
        
        [Header("Card Data")]
        public int CardId;
        public string CardName;
        public int Cost;
        public int Power;
        public string AbilityType;
        public int AbilityValue;

        private bool isSelected = false;
        private ulong localPlayerId = 1; // Placeholder

        void Start()
        {
            InitializeCardDisplay();
        }

        private void InitializeCardDisplay()
        {
            // Set up UI elements
            if (CardNameText != null)
                CardNameText.text = CardName;
                
            if (CostText != null)
                CostText.text = Cost.ToString();
                
            if (PowerText != null)
                PowerText.text = Power.ToString();
                
            if (AbilityText != null)
            {
                if (!string.IsNullOrEmpty(AbilityType))
                    AbilityText.text = $"{AbilityType}: {AbilityValue}";
                else
                    AbilityText.text = "No Ability";
            }

            if (CardButton != null)
            {
                CardButton.onClick.AddListener(OnCardClicked);
            }
        }

        private void OnCardClicked()
        {
            // Toggle selection visually
            isSelected = !isSelected;
            UpdateVisualSelection();

            // Emit intent - never change game state directly
            if (isSelected)
            {
                var playCardCommand = new PlayCardCommand(localPlayerId, CardId);
                if (NetworkService.Instance != null)
                {
                    NetworkService.Instance.SendCommandToServer(playCardCommand);
                }
            }
        }

        private void UpdateVisualSelection()
        {
            // Visual feedback for selection
            var image = GetComponent<Image>();
            if (image != null)
            {
                image.color = isSelected ? Color.yellow : Color.white;
            }
        }

        // Public methods for external control
        public void SetSelected(bool selected)
        {
            isSelected = selected;
            UpdateVisualSelection();
        }

        public bool IsSelected()
        {
            return isSelected;
        }

        public void UpdateCardData(int cardId, string name, int cost, int power, string abilityType = null, int abilityValue = 0)
        {
            CardId = cardId;
            CardName = name;
            Cost = cost;
            Power = power;
            AbilityType = abilityType;
            AbilityValue = abilityValue;
            
            InitializeCardDisplay();
        }
    }
}