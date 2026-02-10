// using CardDuel.UI.Events;
// using CardDuel.Gameplay;

// namespace CardDuel.Application
// {
//     public class CardSelectionUseCase
//     {
//         private readonly PlayerHandState _hand;
//         private readonly GameState _game;

//         public CardSelectionUseCase(PlayerHandState hand, GameState game)
//         {
//             _hand = hand;
//             _game = game;

//             UIEventBus.Subscribe<CardSelectedEvent>(OnSelect);
//             UIEventBus.Subscribe<CardDeselectedEvent>(OnDeselect);
//         }

//         private void OnSelect(CardSelectedEvent evt)
//         {
//             var card = _hand.Hand.Find(c => c.Id == evt.CardId);
//             if (card == null)
//                 return;

//             if (_hand.TotalDrawnCost() + card.Cost > _game.EnergyCap)
//                 return;

//             _hand.Hand.Remove(card);
//             _hand.Drawn.Add(card);

//             UIEventBus.Publish(new CardMovedEvent(evt.CardId, true));
//         }

//         private void OnDeselect(CardDeselectedEvent evt)
//         {
//             var card = _hand.Drawn.Find(c => c.Id == evt.CardId);
//             if (card == null)
//                 return;

//             _hand.Drawn.Remove(card);
//             _hand.Hand.Add(card);

//             UIEventBus.Publish(new CardMovedEvent(evt.CardId, false));
//         }
//     }
// }
