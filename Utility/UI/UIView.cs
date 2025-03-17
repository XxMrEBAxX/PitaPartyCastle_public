using UnityEngine;

namespace UB.UI
{
    public abstract class UIView : MonoBehaviour
    {
        [SerializeField] protected GameObject _panel;
        
        public void Show()
        {
            _panel.SetActive(true);
            EnableAction();
        }

        public void Hide()
        {
            DisableAction();
            _panel.SetActive(false);
        }

        public virtual void EnableAction() {}
        public virtual void DisableAction() {}
    }
}
