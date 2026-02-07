using UnityEngine;
using CardDuel.Events;

namespace CardDuel.UI.Score
{
    public class ScoreUI : MonoBehaviour
    {
        private void OnEnable()
        {
            EventBus.Subscribe<ScoreUpdatedEvent>(OnScoreUpdated);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<ScoreUpdatedEvent>(OnScoreUpdated);
        }

        private void OnScoreUpdated(ScoreUpdatedEvent e)
        {
            // Update score text
        }
    }
}


namespace CardDuel.UI.Game
{
}
