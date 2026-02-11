using System;
using System.Collections.Generic;
using UnityEngine;
using CardDuel.UI.Events;
using CardDuel.Gameplay;

namespace CardDuel.Gameplay
{
    /// <summary>
    /// Manages the staging area where players can move cards before finalizing their turn
    /// Cards in staging are not yet committed - players can move them back to hand
    /// </summary>
    public class TurnStagingSystem
    {
        private readonly PlayerHandState _playerHand;
        private readonly List<CardData> _stagingArea;
        private int _availableEnergy;
        private int _usedEnergy;
        private int _turnNumber;
        
        public List<CardData> StagingArea => _stagingArea;
        public int AvailableEnergy => _availableEnergy;
        public int UsedEnergy => _usedEnergy;
        public int RemainingEnergy => _availableEnergy - _usedEnergy;
        public int TurnNumber => _turnNumber;
        
        public event Action<CardData> OnCardMovedToStaging;
        public event Action<CardData> OnCardMovedBackToHand;
        public event Action OnEnergyChanged;
        public event Action OnStagingAreaCleared;
        
        public TurnStagingSystem(PlayerHandState playerHand)
        {
            _playerHand = playerHand;
            _stagingArea = new List<CardData>();
            _availableEnergy = 0;
            _usedEnergy = 0;
            _turnNumber = 0;
        }
        
        /// <summary>
        /// Initialize staging system for new turn
        /// </summary>
        public void InitializeTurn(int turnNumber, int energy)
        {
            _turnNumber = turnNumber;
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
            // Find card in player's hand
            var card = _playerHand.GetCardFromHand(cardId);
            if (card == null)
            {
                Debug.LogWarning($"Card {cardId} not found in hand");
                return false;
            }
            
            // Check if player has enough energy
            if (_usedEnergy + card.Cost > _availableEnergy)
            {
                UIEventBus.Publish(new InsufficientEnergyEvent { RequiredEnergy = card.Cost });
                return false;
            }
            
            // Move card to staging area
            if (_playerHand.RemoveFromHand(card))
            {
                _stagingArea.Add(card);
                _usedEnergy += card.Cost;
                
                // Assign order index for reveal sequence
                card.OrderIndex = _stagingArea.Count - 1;
                card.IsRevealed = false;
                
                OnCardMovedToStaging?.Invoke(card);
                OnEnergyChanged?.Invoke();
                
                UIEventBus.Publish(new CardStagedEvent 
                { 
                    CardId = card.Id, 
                    RemainingEnergy = RemainingEnergy 
                });
                
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// Move card from staging area back to hand
        /// </summary>
        public bool MoveCardBackToHand(int cardId)
        {
            var card = _stagingArea.Find(c => c.Id == cardId);
            if (card == null)
            {
                Debug.LogWarning($"Card {cardId} not found in staging area");
                return false;
            }
            
            // Move card back to hand
            _stagingArea.Remove(card);
            _playerHand.AddToHand(card);
            _usedEnergy -= card.Cost;
            
            // Reset card properties
            card.OrderIndex = -1;
            card.IsRevealed = false;
            
            // Re-index remaining cards
            for (int i = 0; i < _stagingArea.Count; i++)
            {
                _stagingArea[i].OrderIndex = i;
            }
            
            OnCardMovedBackToHand?.Invoke(card);
            OnEnergyChanged?.Invoke();
            
            UIEventBus.Publish(new CardUnstagedEvent 
            { 
                CardId = card.Id, 
                RemainingEnergy = RemainingEnergy 
            });
            
            return true;
        }
        
        /// <summary>
        /// Clear all cards from staging area back to hand
        /// </summary>
        public void ClearStagingArea()
        {
            while (_stagingArea.Count > 0)
            {
                var card = _stagingArea[_stagingArea.Count - 1];
                MoveCardBackToHand(card.Id);
            }
            
            OnStagingAreaCleared?.Invoke();
        }
        
        /// <summary>
        /// Finalize the staging area - lock cards for reveal
        /// </summary>
        public List<CardData> FinalizeStaging()
        {
            var stagedCards = new List<CardData>(_stagingArea);
            _stagingArea.Clear();
            _usedEnergy = 0;
            
            UIEventBus.Publish(new StagingFinalizedEvent { CardCount = stagedCards.Count });
            
            return stagedCards;
        }
        
        /// <summary>
        /// Check if card can be moved to staging (energy check)
        /// </summary>
        public bool CanMoveToStaging(int cardId)
        {
            var card = _playerHand.GetCardFromHand(cardId);
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
        /// Get total cost of staged cards
        /// </summary>
        public int GetStagedCost()
        {
            int totalCost = 0;
            foreach (var card in _stagingArea)
            {
                totalCost += card.Cost;
            }
            return totalCost;
        }
        
        /// <summary>
        /// Check if staging area is empty
        /// </summary>
        public bool IsEmpty => _stagingArea.Count == 0;
        
        /// <summary>
        /// Get staging area as read-only collection
        /// </summary>
        public IReadOnlyList<CardData> GetStagedCardsReadOnly()
        {
            return _stagingArea.AsReadOnly();
        }
    }
    
    // Events for staging system
    public class CardStagedEvent : IUIEvent
    {
        public int CardId;
        public int RemainingEnergy;
    }
    
    public class CardUnstagedEvent : IUIEvent
    {
        public int CardId;
        public int RemainingEnergy;
    }
    
    public class StagingFinalizedEvent : IUIEvent
    {
        public int CardCount;
    }
    
    public class InsufficientEnergyEvent : IUIEvent
    {
        public int RequiredEnergy;
    }
}