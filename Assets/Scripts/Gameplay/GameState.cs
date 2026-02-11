using System;
using System.Collections.Generic;
using Unity.Netcode;

namespace CardDuel.Gameplay
{
    public class GameState
    {
        public static GameState Instance { get; private set; }

        public int CurrentRound { get; private set; } = 1;
        public int EnergyCap => CurrentRound;

        public PlayerHandState LocalHand { get; } = new();

        // 🔥 Multiplayer round tracking
        public HashSet<ulong> TurnEndedPlayers = new();

        public Dictionary<ulong, List<CardData>> PlayedCards
            = new();

        public Dictionary<ulong, int> Scores
            = new();

        public GameState()
        {
            Instance = this;
        }

        public void ResetMatch()
        {
            CurrentRound = 1;
            TurnEndedPlayers.Clear();
            PlayedCards.Clear();
            Scores.Clear();
            LocalHand.Hand.Clear();
            LocalHand.Drawn.Clear();
            // Clear the lookup dictionaries as well
            // Since the lists are cleared, we don't need to manually clear the dictionaries
            // as they'll be empty by association
        }

        public void NextRound()
        {
            CurrentRound++;
            TurnEndedPlayers.Clear();
            PlayedCards.Clear();
            LocalHand.Drawn.Clear();
        }

        public void RegisterPlayer(ulong clientId)
        {
            // Check if key exists and add if not present
            if (!Scores.ContainsKey(clientId))
                Scores[clientId] = 0;

            if (!PlayedCards.ContainsKey(clientId))
                PlayedCards[clientId] = new List<CardData>();
        }

       
    }
}
