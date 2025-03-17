using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UB.UI
{
    public class KeyBindingButton : MonoBehaviour
    {
        [SerializeField] private InputActionReference _actionKey = null;
        [SerializeField] private InputActionReference _actionKeyMoveHoldX = null;
        [SerializeField] private TMP_Text _bindingDisplayNameText = null;
        [SerializeField] private GameObject _startRebindObject = null;
        [SerializeField] private GameObject _waitingForInputObject = null;

        private InputActionRebindingExtensions.RebindingOperation _rebindingOperation;
        [SerializeField] private int _index = 0;

        private void Start()
        {
            int bindingIndex = _actionKey.action.GetBindingIndexForControl(_actionKey.action.controls[_index]);

            _bindingDisplayNameText.text = InputControlPath.ToHumanReadableString(
                _actionKey.action.bindings[bindingIndex].effectivePath,
                InputControlPath.HumanReadableStringOptions.OmitDevice);
        }

        public void StartRebinding()
        {
            _startRebindObject.SetActive(false);
            _waitingForInputObject.SetActive(true);

            _actionKey.action.Disable();

            // Vector로 되어있는 PlayerActionsReference인 경우 리바인딩 시 index가 1부터 시작함
            int vectorInx = _index;
            if(_actionKey.action.name == "Move")
                vectorInx++;

            //_playerInput.SwitchCurrentActionMap("Menu");

            _rebindingOperation = _actionKey.action.PerformInteractiveRebinding(vectorInx)
                //.WithControlsExcluding("Mouse")
                .OnMatchWaitForAnother(0.1f)
                .OnComplete(operation => RebindComplete())
                .Start();
        }

        private void RebindComplete()
        {
            _actionKey.action.Enable();
            int bindingIndex = _actionKey.action.GetBindingIndexForControl(_actionKey.action.controls[_index]);

            _bindingDisplayNameText.text = InputControlPath.ToHumanReadableString(
                _actionKey.action.bindings[bindingIndex].effectivePath,
                InputControlPath.HumanReadableStringOptions.OmitDevice);

            _rebindingOperation.Dispose();

            _startRebindObject.SetActive(true);
            _waitingForInputObject.SetActive(false);

            if(_actionKey.action.name == "Move" && (_index > 1))
            {
                _actionKeyMoveHoldX.action.Disable();
                _actionKeyMoveHoldX.action.ApplyBindingOverride(_index - 1, _actionKey.action.bindings[bindingIndex]);
                _actionKeyMoveHoldX.action.Enable();
            }
        }

        public void ResetBinding()
        {
            _actionKey.action.Disable();

            int vectorInx = _index;
            if(_actionKey.action.name == "Move")
                vectorInx++;

            _actionKey.action.RemoveBindingOverride(vectorInx);
            _actionKey.action.Enable();

            int bindingIndex = _actionKey.action.GetBindingIndexForControl(_actionKey.action.controls[_index]);

            _bindingDisplayNameText.text = InputControlPath.ToHumanReadableString(
                _actionKey.action.bindings[bindingIndex].effectivePath,
                InputControlPath.HumanReadableStringOptions.OmitDevice);
        }
    }
}
