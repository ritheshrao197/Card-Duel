using UnityEngine;
using CardDuel.Utils;
using Logger = CardDuel.Utils.Logger;
using Unity.VisualScripting;

namespace CardDuel.UI.Core
{
    public abstract class UIPanel : MonoBehaviour
    {
        [SerializeField] private UIState state;
        public UIState State => state;

        // ❌ REMOVE subscription from lifecycle
        // protected virtual void OnEnable()
        // {
        //     Logger.Log(LogCategory.UI, $"{name} | OnEnable");
        // }

        // protected virtual void OnDisable()
        // {
        //     Logger.Log(LogCategory.UI, $"{name} | OnDisable");
        // }

        public virtual void SubscribeToEvents()
        {
            Logger.Log(LogCategory.UI, $"{name} | SubscribeToEvents");
        }

        public virtual void UnsubscribeFromEvents()
        {
            Logger.Log(LogCategory.UI, $"{name} | UnsubscribeFromEvents");
        }

        public virtual void Show()
        {
            if (gameObject.activeSelf)
                return;

            Logger.Log(LogCategory.UI, $"{name} | Show");
            gameObject.SetActive(true);
        }

        public virtual void Hide()
        {
            if (!gameObject.activeSelf)
                return;

            Logger.Log(LogCategory.UI, $"{name} | Hide");
            gameObject.SetActive(false);
        }
    }
}
