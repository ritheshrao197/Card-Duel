using UnityEngine;
using CardDuel.Domain.Events;
using CardDuel.Gameplay.Events;

namespace CardDuel.Networking.Serialization
{
    public static class EventSerializer
    {
        public static string Serialize(IGameEvent evt)
        {
            return JsonUtility.ToJson(evt);
        }

        public static IGameEvent Deserialize(string json, string type)
        {
            return type switch
            {
                nameof(CardPlayedEvent) =>
                    JsonUtility.FromJson<CardPlayedEvent>(json),

                nameof(TurnStartedEvent) =>
                    JsonUtility.FromJson<TurnStartedEvent>(json),

                nameof(TurnEndedEvent) =>
                    JsonUtility.FromJson<TurnEndedEvent>(json),

                nameof(ScoreUpdatedEvent) =>
                    JsonUtility.FromJson<ScoreUpdatedEvent>(json),

                nameof(MatchEndedEvent) =>
                    JsonUtility.FromJson<MatchEndedEvent>(json),

                _ => null
            };
        }
    }
}
