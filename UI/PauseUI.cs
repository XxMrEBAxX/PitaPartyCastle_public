using System.Collections.Generic;
using MyBox;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace UB.UI
{
    public class PauseUI : UIView
    {
        [OverrideLabel("ESC 시 일시정지 창 활성화 여부")]
        [SerializeField] private bool _active = true;
        public GameObject Buttons;

        private Material _originFontMaterial;
        [SerializeField] private Material _selectFontMaterial;

        [SerializeField] private RectTransform _leftSelectImage;
        [SerializeField] private RectTransform _rightSelectImage;

        private List<Button> _buttons;
        private UINavigation _navigation;
        private MainMenuUI _mainMenuUI;

        private void Start()
        {
            _buttons = new List<Button>();
            _navigation = new UINavigation();
            _mainMenuUI = GameObject.Find("MainMenuButtons")?.GetComponent<MainMenuUI>();

            if (_panel.activeSelf)
                _navigation.PushView(this);

            foreach (var button in Buttons.GetComponentsInChildren<Button>())
            {
                _buttons.Add(button);
            }
            _originFontMaterial = _buttons[0].GetComponentInChildren<TMP_Text>().fontMaterial;
        }

        private void Update()
        {
            //if (Input.GetKeyDown(KeyCode.Escape))
            if (Keyboard.current[Key.Escape].wasPressedThisFrame)
            {
                if (_navigation.Count() > 0)
                {
                    if (_navigation.Count() == 1)
                    {
                        ClickContinue();
                        if (_mainMenuUI != null)
                            _mainMenuUI.SelectButton(_mainMenuUI.CurIndex);
                    }
                    else
                        _navigation.PopView();
                }
                else if (_active)
                {
                    ShowPanel(this);
                }
            }

            // 메인 화면 버튼이 DeSelect 되는 것을 방지
            if (_mainMenuUI != null)
            {
                if (_navigation.Count() == 0)
                {
                    _mainMenuUI.SelectButton(_mainMenuUI.CurIndex);
                }
            }
        }

        public void ClickContinue()
        {
            _navigation.PopView();
            TimeManager.Instance.AbleSetTimeScale = true;
            TimeManager.Instance.SetForceTimeScale(1);
        }

        public void GameExit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
            Application.Quit();
        }

        public void ShowPanel(UIView uiView)
        {
            _navigation.PushView(uiView);
            TimeManager.Instance.AbleSetTimeScale = false;
            TimeManager.Instance.SetForceTimeScale(0);
        }

        public void HidePanel(UIView uiView)
        {
            if (_navigation.Count() == 1)
            {
                ClickContinue();
                if (_mainMenuUI != null)
                    _mainMenuUI.SelectButton(_mainMenuUI.CurIndex);
            }
            else
                _navigation.PopView();
        }

        public void SelectButton(int index)
        {
            //EventSystem.current.SetSelectedGameObject(_buttons[index].gameObject);
            var mesh = _buttons[index].GetComponentInChildren<TMP_Text>();
            mesh.color = Color.white;
            mesh.fontMaterial = _selectFontMaterial;
            var box = _buttons[index].GetComponent<BoxCollider2D>();
            _leftSelectImage.position = new Vector3(box.bounds.min.x, box.bounds.center.y, 0);
            _rightSelectImage.position = new Vector3(box.bounds.max.x, box.bounds.center.y, 0);

            foreach (var button in _buttons)
            {
                if (_buttons[index] == button)
                    continue;

                var buttonMesh = button.GetComponentInChildren<TMP_Text>();
                buttonMesh.color = Color.gray;
                buttonMesh.fontMaterial = _originFontMaterial;
            }
            _buttons[index].Select();
        }

        public override void EnableAction()
        {
            SelectButton(0);
        }
    }
}