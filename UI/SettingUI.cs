using System.Collections.Generic;
using AssetKits.ParticleImage;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace UB.UI
{
    public class SettingUI : UIView
    {
        public PlayerData PlayerDataInstance;

        [SerializeField] private GameObject _buttons;
        [SerializeField] private Image _menuBarSelectImage;
        [SerializeField] private Image _OptionSelectImage;

        [SerializeField] private GameObject _GamePlayUIPanel;
        [SerializeField] private GameObject _graphicUIPanel;
        [SerializeField] private GameObject _soundUIPanel;
        [SerializeField] private GameObject _keyMappingUIPanel;

        [SerializeField] private ParticleImage _particleImage;

        private List<Button> _buttonList;
        private List<GameObject> _panelList;
        private Vector2 _originMenuBarAnchoredPosition;
        private Button _selectedButton;

        void Awake()
        {
            _buttonList = new List<Button>();

            foreach (var button in _buttons.GetComponentsInChildren<Button>())
            {
                _buttonList.Add(button);
            }

            _panelList = new List<GameObject>
            {
                _GamePlayUIPanel,
                _graphicUIPanel,
                _soundUIPanel,
                _keyMappingUIPanel
            };

            _originMenuBarAnchoredPosition = _buttonList[0].GetComponent<RectTransform>().anchoredPosition;
        }



        public void SelectButton(int index)
        {
            //EventSystem.current.SetSelectedGameObject(_buttons[index].gameObject);
            var sel = _buttonList[index];
            sel.Select();
            if (sel == _selectedButton)
                return;
            var rect = sel.GetComponent<RectTransform>();

            _selectedButton = sel;
            
            rect.anchoredPosition = new Vector2(_originMenuBarAnchoredPosition.x, rect.anchoredPosition.y);
            _menuBarSelectImage.transform.position = new Vector2(sel.transform.position.x, sel.transform.position.y);
            _menuBarSelectImage.rectTransform.anchoredPosition += new Vector2(80, -4);
            float originMenuBarSelectImageX = _menuBarSelectImage.rectTransform.anchoredPosition.x;
            DOTween.To(x =>
            {
                rect.anchoredPosition = new Vector2(x, rect.anchoredPosition.y);
                _menuBarSelectImage.rectTransform.anchoredPosition  = new Vector2(originMenuBarSelectImageX
                    + (x - _originMenuBarAnchoredPosition.x), _menuBarSelectImage.rectTransform.anchoredPosition.y);
            }, _originMenuBarAnchoredPosition.x, _originMenuBarAnchoredPosition.x - 50, 0.2f).SetUpdate(true);
            
            _menuBarSelectImage.color = new Color(1, 1, 1, 0);
            _menuBarSelectImage.DOColor(new Color(1, 1, 1, 0.4f), 0.3f).SetUpdate(true);

            foreach (var button in _buttonList)
            {
                if (sel == button)
                    continue;
                var rectTransform = button.GetComponent<RectTransform>();
                if (rectTransform.anchoredPosition.x != _originMenuBarAnchoredPosition.x)
                {
                    DOTween.To(x =>
                    {
                        rectTransform.anchoredPosition = new Vector2(x, rectTransform.anchoredPosition.y);
                    }, rectTransform.anchoredPosition.x, _originMenuBarAnchoredPosition.x, 0.2f).SetUpdate(true);
                }
            }

            foreach (var panel in _panelList)
            {
                panel.SetActive(false);
            }

            switch (index)
            {
                case 0:
                    _GamePlayUIPanel.SetActive(true);
                    break;
                case 1:
                    _graphicUIPanel.SetActive(true);
                    break;
                case 2:
                    _soundUIPanel.SetActive(true);
                    break;
                case 3:
                    _keyMappingUIPanel.SetActive(true);
                    break;
            }
        }

        public void SelectOptionButton(Button button)
        {
            button.Select();
            _OptionSelectImage.transform.position = button.transform.position;
            _OptionSelectImage.color = new Color(1, 1, 1, 0);
            _OptionSelectImage.DOKill();
            _OptionSelectImage.DOColor(new Color(1, 1, 1, 0.4f), 0.3f).SetUpdate(true);
        }

        public void UnSelectOptionButton()
        {
            _OptionSelectImage.DOKill();
            _OptionSelectImage.DOColor(new Color(1, 1, 1, 0), 0.3f).SetUpdate(true);
        }

        public override void EnableAction()
        {
            _GamePlayUIPanel.GetComponentInParent<GamePlaySettingUI>().Enable();
            _graphicUIPanel.GetComponentInParent<GraphicUI>().Enable();
            _OptionSelectImage.color = new Color(1, 1, 1, 0);
            SelectButton(0);
        }

        public override void DisableAction()
        {
            _GamePlayUIPanel.GetComponentInParent<GamePlaySettingUI>().Disable();
        }

        public void SaveButton(GameObject gameObject)
        {
            _GamePlayUIPanel.GetComponentInParent<GamePlaySettingUI>().OnSave();
            _graphicUIPanel.GetComponentInParent<GraphicUI>().OnSave();
            _particleImage.Play();
        }
    }
}