using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UB
{
    public class GraphicUI : MonoBehaviour
    {
        [SerializeField] private Sprite _selectedSprite;
        [SerializeField] private Sprite _unSelectedSprite;

        [Space(10)]

        [SerializeField] private TMP_Dropdown _resolutionDropdown;

        [Space(10)]

        [SerializeField] private Button[] _fullScreenButtons;

        [Space(10)]

        [SerializeField] private Button[] _vsyncButtons;

        [Space(10)]

        [SerializeField] private Slider _frameRateSlider;
        [SerializeField] private TMP_InputField _frameRateInputField;
        [SerializeField] private TMP_Text _minFrameRateText;
        [SerializeField] private TMP_Text _maxFrameRateText;

        private int _maxFrameRate;
        private int _minFrameRate = 30;
        private int _curFrameRate;
        private bool _curFullScreen;
        private int _curVsync;
        private static Resolution _curResolution;
        private static bool _isCurResolutionSet = false;
        private List<Resolution> fixResolutions;

        private void Awake()
        {
            fixResolutions = new List<Resolution>();
            Resolution[] resolutions = Screen.resolutions;

            _resolutionDropdown.ClearOptions();
            int tmpWidth = 0;
            int tmpHeight = 0;
            foreach (Resolution resolution in resolutions)
            {
                if (tmpWidth == resolution.width && tmpHeight == resolution.height)
                    continue;
                _resolutionDropdown.options.Add(new TMP_Dropdown.OptionData(resolution.width + " x " + resolution.height));
                tmpWidth = resolution.width;
                tmpHeight = resolution.height;
                fixResolutions.Add(resolution);
            }
            _resolutionDropdown.RefreshShownValue();

            if (!_isCurResolutionSet)
            {
                _curResolution.width = 1280;
                _curResolution.height = 720;
                _isCurResolutionSet = true;
            }
            _maxFrameRate = (int)resolutions[resolutions.Length - 1].refreshRateRatio.value;
            _minFrameRateText.text = _minFrameRate.ToString();
            _maxFrameRateText.text = _maxFrameRate.ToString();

            Application.targetFrameRate = _maxFrameRate;
        }

        public void Enable()
        {
            _frameRateSlider.value = (float)(Application.targetFrameRate - _minFrameRate) / (_maxFrameRate - _minFrameRate);;
            OnValueChangedSliderToFrameRate();

            //FIXME - Vsync 옵션 갑 받아와서 Button에 적용
            OnSelectVSync(QualitySettings.vSyncCount == 0 ? 1 : 0);

            _resolutionDropdown.value = GetCurrentResolutionIndex();
            //FIXME - FullScreen 옵션 갑 받아와서 Button에 적용
            OnSelectFullScreen(Screen.fullScreen ? 0 : 1);

            //FIXME - FrameRate 옵션 갑 받아와서 InputField에 적용
            _frameRateInputField.text = Application.targetFrameRate.ToString();
            OnValueChangedInputField();
        }

        public void OnValueChangedSliderToFrameRate()
        {
            int intValue = Mathf.FloorToInt(Mathf.Lerp(_minFrameRate, _maxFrameRate, _frameRateSlider.value));
            _frameRateInputField.text = intValue.ToString();
            _curFrameRate = intValue;
        }

        public void OnValueChangedInputField()
        {
            int value = int.Parse(_frameRateInputField.text);
            if (value > _maxFrameRate || value < _minFrameRate)
            {
                value = _curFrameRate;
            }

            _curFrameRate = value;
            _frameRateInputField.text = value.ToString($"{0}");
            _frameRateSlider.value = (float)(value - _minFrameRate) / (_maxFrameRate - _minFrameRate);
        }

        public void OnSelectVSync(int index)
        {
            foreach (var button in _vsyncButtons)
            {
                button.image.sprite = _unSelectedSprite;
            }

            _vsyncButtons[index].image.sprite = _selectedSprite;

            _curVsync = index == 0 ? 1 : 0;
        }

        public void SetResolution()
        {
            if (!_isCurResolutionSet)
            {
                return;
            }
            int index = _resolutionDropdown.value;
            if (index >= 0 && index < fixResolutions.Count)
            {
                Resolution resolution = fixResolutions[index];
                _curResolution = resolution;
            }
        }

        public void OnSelectFullScreen(int index)
        {
            foreach (var button in _fullScreenButtons)
            {
                button.image.sprite = _unSelectedSprite;
            }

            _fullScreenButtons[index].image.sprite = _selectedSprite;

            _curFullScreen = index == 0 ? true : false;
        }

        private int GetCurrentResolutionIndex()
        {
            for (int i = 0; i < fixResolutions.Count; i++)
            {
                if (fixResolutions[i].width == _curResolution.width && fixResolutions[i].height == _curResolution.height)
                {
                    return i;
                }
            }
            return -1;
        }

        public void OnSave()
        {
            Screen.SetResolution(_curResolution.width, _curResolution.height, Screen.fullScreen);
            Application.targetFrameRate = _curFrameRate;
            Screen.fullScreen = _curFullScreen;
            QualitySettings.vSyncCount = _curVsync;
        }
    }
}
