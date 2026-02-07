using System;
using CardDuel.Events;
using CardDuel.Utils;
using UnityEngine;

namespace CardDuel.Networking.Netcode
{
    public static class NetworkClient
    {
        private static NetworkPlayer _player;

        public static void Initialize(NetworkPlayer player)
        {
            _player = player;
            EventBus.Subscribe<EndTurnPressedEvent>(_ => SendEndTurn());
        }

        public static void SendEndTurn()
        {
            Send(new NetworkMessage
            {
                action = "endTurn",
                payload = "{}"
            });
        }

        public static void SendTest()
        {
            Send(new NetworkMessage
            {
                action = "test",
                payload = "{ \"msg\": \"hello\" }"
            });
        }
        public static void SendPlayerReady()
        {
            Send(new NetworkMessage
            {
                action = "playerReady",
                payload = "{}"
            });
        }

        public static void Send(NetworkMessage msg)
        {
            string json = JsonUtility.ToJson(msg);
            Utils.Logger.Log(LogCategory.Network, $"Client sending {msg.action}");
            _player.SendMessageServerRpc(json);
        }

    }
}
