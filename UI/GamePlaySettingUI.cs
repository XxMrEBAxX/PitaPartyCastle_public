using System.Collections.Generic;
using TMPro;
using UB.UI;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

namespace UB
{
    public class GamePlaySettingUI : MonoBehaviour
    {
        [SerializeField] private TMP_InputField _inputField;
        [SerializeField] private Slider _slider;

        [SerializeField] private Button[] _buttons;
        [SerializeField] private Button[] _buttonsLanguage;
        [SerializeField] private Sprite _selectedSprite;
        [SerializeField] private Sprite _unSelectedSprite;

        private const float AIM_VALUE_MAX = 2.0f;
        private const float AIM_VALUE_MIN = 0;
        private float _aimValue;
        private bool _isMoveXSlidingValue;
        private string _currentLanguage;

        private PlayerData _playerData;


        private void Awake()
        {
            _playerData = GetComponentInParent<SettingUI>().PlayerDataInstance;
            if (_playerData == null)
                Debug.LogError("PlayerData is null");

            _currentLanguage = LocalizationSettings.SelectedLocale.Identifier.Code;
        }

        public void Enable()
        {
            float saveValue = _playerData.AimCorrection;
            _inputField.text = saveValue.ToString($"{0:F1}");
            _slider.value = saveValue / AIM_VALUE_MAX;
            _aimValue = saveValue;

            OnSelectMoveXButton(_playerData.IsMoveXSliding ? 0 : 1);
            OnSelectLanguageButton(_currentLanguage == "ko-KR" ? 0 : 1);
        }

        public void Disable()
        {
            LoadLocale(_currentLanguage);
        }

        public void OnValueChangedInputField()
        {
            float value = float.Parse(_inputField.text);
            if (value > AIM_VALUE_MAX || value < AIM_VALUE_MIN)
            {
                value = _aimValue;
            }

            _inputField.text = value.ToString($"{0:F1}");
            _slider.value = value / AIM_VALUE_MAX;
            _aimValue = value;
        }

        public void OnValueChangedSlider()
        {
            float value = _slider.value * AIM_VALUE_MAX;
            _inputField.text = value.ToString($"{0:F1}");
            _aimValue = value;
        }

        public void OnSelectMoveXButton(int index)
        {
            foreach (var button in _buttons)
            {
                button.image.sprite = _unSelectedSprite;
            }

            _buttons[index].image.sprite = _selectedSprite;

            _isMoveXSlidingValue = index == 0 ? true : false;
        }

        public void OnSelectLanguageButton(int index)
        {
            foreach (var button in _buttonsLanguage)
            {
                button.image.sprite = _unSelectedSprite;
            }

            _buttonsLanguage[index].image.sprite = _selectedSprite;

            switch (index)
            {
                case 0:
                    //_currentLanguage = "ko-KR";
                    LoadLocale("ko-KR");
                    break;
                case 1:
                    //_currentLanguage = "en";
                    LoadLocale("en");
                    break;
            }
        }

        private void LoadLocale(string languageIdentifier)
        {
            LocaleIdentifier localeCode = new LocaleIdentifier(languageIdentifier);
            for (int i = 0; i < LocalizationSettings.AvailableLocales.Locales.Count; i++)
            {
                Locale aLocale = LocalizationSettings.AvailableLocales.Locales[i];
                LocaleIdentifier anIdentifier = aLocale.Identifier;
                if (anIdentifier == localeCode)
                {
                    LocalizationSettings.SelectedLocale = aLocale;
                }
            }
        }

        public void OnSave()
        {
            _playerData.SetIsMoveXSliding(_isMoveXSlidingValue);
            _playerData.SetAimCorrection(_aimValue);
            if (_currentLanguage != LocalizationSettings.SelectedLocale.Identifier.Code)
                _currentLanguage = LocalizationSettings.SelectedLocale.Identifier.Code;
        }
    }
}
