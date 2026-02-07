using System.Collections.Generic;
using UnityEngine;
using CardDuel.Utils;
using Logger = CardDuel.Utils.Logger;
using Unity.VisualScripting;

namespace CardDuel.UI.Core
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance;

        [SerializeField]
        private List<UIPanel> panels;

        private readonly Dictionary<UIState, UIPanel> _panelMap = new();
        private UIState _currentState = UIState.None;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

           
            
        }
        private void OnEnable()
        {
            Logger.Log(LogCategory.UI, "UIManager | OnEnable");

            foreach (var panel in panels)
            {
                if (panel == null)
                    continue;

                _panelMap[panel.State] = panel;

                // ✅ subscribe ONCE and keep it
                panel.SubscribeToEvents();

                // hide after subscription
                panel.Hide();
            }
            SwitchTo(UIState.Matchmaking);

        }

        private void OnDestroy()
        {
            Logger.Log(LogCategory.UI, "UIManager | OnDestroy");

            foreach (var panel in panels)
            {
                if (panel == null)
                    continue;

                panel.UnsubscribeFromEvents();
            }
        }

        public void SwitchTo(UIState newState)
        {
            if (_currentState == newState)
                return;

            Logger.Log(
                LogCategory.UI,
                $"UIManager | Switch {_currentState} -> {newState}"
            );

            if (_panelMap.TryGetValue(_currentState, out var current))
                current.Hide();

            _currentState = newState;

            if (_panelMap.TryGetValue(newState, out var next))
                next.Show();
        }

        public UIState CurrentState => _currentState;
    }
}
