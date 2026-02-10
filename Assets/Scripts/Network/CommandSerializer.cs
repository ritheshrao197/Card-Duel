using CardDuel.Domain.Commands;
using CardDuel.Gameplay.Commands;
using CardDuel.Gameplay.Events;
using UnityEngine;

namespace CardDuel.Networking.Serialization
{
    public static class CommandSerializer
    {
        public static string Serialize(ICommand command)
        {
            return JsonUtility.ToJson(command);
        }

        public static ICommand Deserialize(string json, string type)
        {
            return type switch
            {
                nameof(PlayCardCommand) =>
                    JsonUtility.FromJson<PlayCardCommand>(json),

                nameof(EndTurnCommand) =>
                    JsonUtility.FromJson<EndTurnCommand>(json),

                _ => null
            };
        }
    }
}