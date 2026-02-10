using TMPro;
using CardDuel.UI.Core;
using CardDuel.UI.Events;
using UnityEngine;

namespace CardDuel.UI.Panels
{
    public class RevealPanel : UIPanel
    {
        [SerializeField] private UnityEngine.GameObject initiativeIndicator;
        [SerializeField] private TextMeshProUGUI logText;

        protected override void RegisterEvents()
        {
            UIEventBus.Subscribe<RevealPhaseStartedEvent>(OnRevealStarted);
            UIEventBus.Subscribe<CardResolvedEvent>(OnCardResolved);
            UIEventBus.Subscribe<OpponentTimedOutEvent>(OnOpponentTimeout);

        }

        protected override void UnregisterEvents()
        {
            UIEventBus.Unsubscribe<RevealPhaseStartedEvent>(OnRevealStarted);
            UIEventBus.Unsubscribe<CardResolvedEvent>(OnCardResolved);
            UIEventBus.Unsubscribe<OpponentTimedOutEvent>(OnOpponentTimeout);

        }

        private void OnRevealStarted(RevealPhaseStartedEvent evt)
        {
            initiativeIndicator.SetActive(evt.PlayerHasInitiative);
            logText.text = string.Empty;
        }

        private void OnCardResolved(CardResolvedEvent evt)
        {
            logText.text = $"{evt.CardName} ({evt.ScoreDelta:+#;-#})";
        }

        private void OnOpponentTimeout(OpponentTimedOutEvent evt)
        {
            UIEventBus.Publish(new UIStateChangedEvent
            {
                State = UIState.Results
            });

            UIEventBus.Publish(new GameEndedEvent
            {
                IsVictory = true,
                PlayerScore = 0,
                OpponentScore = 0
            });
        }
    }
}
