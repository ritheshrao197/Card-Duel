using UnityEngine;
using TMPro;

namespace CardDuel.UI.Gameplay.Cards
{
    public class CardView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI cardText;

        private int cardId;
        private GameplayInputHandler inputHandler;

        public void Init(int id, GameplayInputHandler handler)
        {
            cardId = id;
            inputHandler = handler;
            cardText.text = id.ToString();
        }

        public void OnClick()
        {
            inputHandler.OnCardClicked(cardId);
        }
    }
}
