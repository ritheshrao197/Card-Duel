using System;
using System.Collections.Generic;
using UnityEngine;
using CardDuel.UI.Events;
using CardDuel.Gameplay;

namespace CardDuel.Gameplay
{
    /// <summary>
    /// Manages the staging area where players can move cards before finalizing their turn
    /// </summary>
    public class TurnStagingSystem
    {
        private readonly PlayerHandState _playerHand;
        private readonly List<CardData> _stagingArea;
        private int _availableEnergy;
        private int _usedEnergy;
        
        public List<CardData> StagingArea => _stagingArea;
        public int AvailableEnergy => _availableEnergy;
        public int UsedEnergy => _usedEnergy;
        public int RemainingEnergy => _availableEnergy - _usedEnergy;
        
        public event Action<CardData> OnCardMovedToStaging;
        public event Action<CardData> OnCardMovedBackToHand;
        public event Action OnEnergyChanged;
        
        public TurnStagingSystem(PlayerHandState playerHand)
        {
            _playerHand = playerHand;
            _stagingArea = new List<CardData>();
            _availableEnergy = 0;
            _usedEnergy = 0;
        }
        
        /// <summary>
        /// Initialize staging system for new turn
        /// </summary>
        public void InitializeTurn(int energy)
        {
            _availableEnergy = energy;
            _usedEnergy = 0;
            ClearStagingArea();
            
            OnEnergyChanged?.Invoke();
        }
        
        /// <summary>
        /// Move card from hand to staging area
        /// </summary>
        public bool MoveCardToStaging(int cardId)
        {
            var card = GetCardDataById(cardId);
            if (card == null) return false;
            
            // Check if player has enough energy
            if (_usedEnergy + card.Cost > _availableEnergy)
            {
                // TODO: Publish energy insufficient event
                return false;
            }
            
            // Move card to staging
            // TODO: Implement proper card removal from hand
            _stagingArea.Add(card);
            _usedEnergy += card.Cost;
                        
            OnCardMovedToStaging?.Invoke(card);
            OnEnergyChanged?.Invoke();
                        
            // TODO: Publish card staged event
            return true;
        }
        
        /// <summary>
        /// Move card from staging area back to hand
        /// </summary>
        public bool MoveCardBackToHand(int cardId)
        {
            var card = _stagingArea.Find(c => c.Id == cardId);
            if (card == null) return false;
            
            // Move card back to hand
            _stagingArea.Remove(card);
            // _playerHand.AddCard(card); // TODO: Implement proper card return
            _usedEnergy -= card.Cost;
            
            OnCardMovedBackToHand?.Invoke(card as CardData);
            OnEnergyChanged?.Invoke();
            
            // TODO: Publish card unstaged event
            return true;
        }
        
        /// <summary>
        /// Clear all cards from staging area back to hand
        /// </summary>
        public void ClearStagingArea()
        {
            while (_stagingArea.Count > 0)
            {
                var card = _stagingArea[0];
                MoveCardBackToHand(card.Id);
            }
        }
        
        /// <summary>
        /// Finalize the staging area - lock cards for reveal
        /// </summary>
        public List<CardData> FinalizeStaging()
        {
            var stagedCards = new List<CardData>(_stagingArea);
            _stagingArea.Clear();
            _usedEnergy = 0;
            
            // TODO: Publish staging finalized event
            return stagedCards;
        }
        
        /// <summary>
        /// Check if card can be moved to staging (energy check)
        /// </summary>
        public bool CanMoveToStaging(int cardId)
        {
            var card = GetCardDataById(cardId);
            return card != null && (_usedEnergy + card.Cost) <= _availableEnergy;
        }
        
        /// <summary>
        /// Get total power of staged cards
        /// </summary>
        public int GetStagedPower()
        {
            int totalPower = 0;
            foreach (var card in _stagingArea)
            {
                totalPower += card.Power;
            }
            return totalPower;
        }
        
        /// <summary>
        /// Helper method to get CardData by ID from player hand
        /// </summary>
        private CardData GetCardDataById(int cardId)
        {
            // This would need to be implemented based on how cards are stored
            // For now, returning null - this needs to be connected to the actual card system
            return null;
        }
    }
    
    // Events for staging system
    public class CardStagedEvent
    {
        public int CardId;
        public int RemainingEnergy;
    }
    
    public class CardUnstagedEvent
    {
        public int CardId;
        public int RemainingEnergy;
    }
    
    public class StagingFinalizedEvent
    {
        public int CardCount;
    }
    
    public class InsufficientEnergyEvent
    {
        public int RequiredEnergy;
    }
}