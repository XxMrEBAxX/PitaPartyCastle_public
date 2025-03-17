using System.Collections.Generic;
using UB.UI;
using UnityEngine;

namespace UB
{
    public class KeyMappingUI : MonoBehaviour
    {
        [SerializeField] private GameObject _keyBindingButtonParent;
        List<KeyBindingButton> _keyBindingButtons;

        private void Start()
        {
            _keyBindingButtons = new List<KeyBindingButton>();
            foreach (var keyBindingButton in _keyBindingButtonParent.GetComponentsInChildren<KeyBindingButton>())
            {
                _keyBindingButtons.Add(keyBindingButton);
            }
        }

        public void ResetKeyBindings()
        {
            foreach (var keyBindingButton in _keyBindingButtons)
            {
                keyBindingButton.ResetBinding();
            }
        }
    }
}
