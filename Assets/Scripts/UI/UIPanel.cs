
using UnityEngine;

namespace CardDuel.UI.Core
{
    public abstract class UIPanel : MonoBehaviour
    {
        public virtual UIState State { get; }

        public void Show()
        {
            gameObject.SetActive(true);
            OnShow();
        }

        public void Hide()
        {
            OnHide();
            gameObject.SetActive(false);
        }

        protected virtual void OnShow() { }
        protected virtual void OnHide() { }
    }
}
