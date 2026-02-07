using TMPro;
using CardDuel.Events;
using CardDuel.Core;
using CardDuel.UI.Core;
using UnityEngine;

namespace CardDuel.UI.Game
{
    public class GameEndPanel : UIPanel
    {
        [SerializeField] private TextMeshProUGUI resultText;

        public override void SubscribeToEvents()
        {
            EventBus.Subscribe<GameEndEvent>(OnGameEnd);
        }

        public override void UnsubscribeFromEvents()
        {
            EventBus.Unsubscribe<GameEndEvent>(OnGameEnd);
        }

        private void OnGameEnd(GameEndEvent e)
        {
            UIManager.Instance.SwitchTo(UIState.GameOver);

            bool isWinner = LocalPlayerContext.PlayerId == e.winnerId;
            resultText.text = isWinner
                ? "You Win 🎉"
                : "You Lose ❌";
        }
    }
}
