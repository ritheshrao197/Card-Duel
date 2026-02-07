using CardDuel.Events;
using CardDuel.UI.Core;
using CardDuel.Utils;
using UnityEngine;
using Logger = CardDuel.Utils.Logger;

namespace CardDuel.UI
{
    public class UIFlowController : MonoBehaviour
    {
        private void OnEnable()
        {
            Logger.Log(LogCategory.UI, "UIFlowController | OnEnable | Subscribing to events");

            EventBus.Subscribe<GameStartEvent>(OnGameStart);
            EventBus.Subscribe<GameEndEvent>(OnGameEnd);
            EventBus.Subscribe<SpectatorModeEvent>(OnSpectator);
        }

        private void OnDisable()
        {
            Logger.Log(LogCategory.UI, "UIFlowController | OnDisable | Unsubscribing from events");

            EventBus.Unsubscribe<GameStartEvent>(OnGameStart);
            EventBus.Unsubscribe<GameEndEvent>(OnGameEnd);
            EventBus.Unsubscribe<SpectatorModeEvent>(OnSpectator);
        }

        private void Start()
        {
            Logger.Log(LogCategory.UI, "UIFlowController | Start | Switching to Matchmaking");

            if (UIManager.Instance == null)
            {
                Logger.Log(LogCategory.UI, "UIFlowController | UIManager missing");
                return;
            }

            // UIManager.Instance.SwitchTo(UIState.Matchmaking);
        }

        private void OnGameStart(GameStartEvent e)
        {
            Logger.Log(LogCategory.UI, "UIFlowController | GameStartEvent received");

            SwitchUI(UIState.Gameplay);
        }

        private void OnGameEnd(GameEndEvent e)
        {
            Logger.Log(LogCategory.UI, "UIFlowController | GameEndEvent received");

            SwitchUI(UIState.GameOver);
        }

        private void OnSpectator(SpectatorModeEvent e)
        {
            Logger.Log(LogCategory.UI, "UIFlowController | SpectatorModeEvent received");

            SwitchUI(UIState.Spectator);
        }

        private static void SwitchUI(UIState state)
        {
            if (UIManager.Instance == null)
            {
                Logger.Log(LogCategory.UI, $"UIFlowController | SwitchUI failed | UIManager null | Target: {state}");
                return;
            }

            Logger.Log(LogCategory.UI, $"UIFlowController | SwitchUI -> {state}");

            UIManager.Instance.SwitchTo(state);
        }
    }
}
