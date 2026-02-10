using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CardDuel.Gameplay;
using CardDuel.UI.Events;

namespace CardDuel.UI.Gameplay
{
    public class CardUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI costText;
        [SerializeField] private TextMeshProUGUI powerText;
        [SerializeField] private Button button;

        public int CardId { get; private set; }
        private bool _isInPlayArea;

        // public void Bind(CardData data, bool inPlayArea)
        // {
        //     CardId = data.Id;
        //     _isInPlayArea = inPlayArea;

        //     nameText.text = data.Name;
        //     costText.text = data.Cost.ToString();
        //     powerText.text = data.Power.ToString();

        //     button.onClick.RemoveAllListeners();
        //     button.onClick.AddListener(OnClicked);
        // }
        public void Bind(CardData data)
        {
            CardId = data.Id;
            nameText.text = data.Name;
            costText.text = data.Cost.ToString();
            powerText.text = data.Power.ToString();
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClicked);
        }
        private void OnClicked()
        {
            Debug.Log("CardUI");
            if (_isInPlayArea)
            {
                Debug.Log("CardUI+_isInPlayArea");

                UIEventBus.Publish(new CardDeselectedEvent
                {
                    CardId = CardId
                });
            }
            else
            {
                Debug.Log("CardUI+nOT InPlayArea");
                UIEventBus.Publish(new CardSelectedEvent
                {
                    CardId = CardId
                });
            }
        }

        public void SetInPlayArea(bool inPlayArea)
        {
            _isInPlayArea = inPlayArea;
        }
    }
}
