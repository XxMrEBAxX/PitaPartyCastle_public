using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UB
{
    public class MainMenuUI : MonoBehaviour
    {
        private Material _originFontMaterial;
        [SerializeField] private Material _selectFontMaterial;

        [SerializeField] private RectTransform _selectImage;

        private List<Button> _buttons;
        public int CurIndex {get; private set;}
        
        private void Start()
        {
            _buttons = new List<Button>();

            foreach (var button in GetComponentsInChildren<Button>())
            {
                _buttons.Add(button);
            }

            _originFontMaterial = _buttons[0].GetComponentInChildren<TMP_Text>().fontMaterial;

            _buttons[0].onClick.AddListener(() => SceneManager.Instance.OnButtonNextScene());

            SelectButton(0);
        }

        public void SelectButton(int index)
        {
            CurIndex = index;
            //EventSystem.current.SetSelectedGameObject(_buttons[index].gameObject);
            _buttons[index].Select();
            var mesh = _buttons[index].GetComponentInChildren<TMP_Text>();
            mesh.fontMaterial = _selectFontMaterial;
            var box = _buttons[index].GetComponent<RectTransform>();
            _selectImage.position = new Vector3(_selectImage.position.x, box.position.y, 0);

            foreach (var button in _buttons)
            {
                if (_buttons[index] == button)
                    continue;

                var buttonMesh = button.GetComponentInChildren<TMP_Text>();
                buttonMesh.fontMaterial = _originFontMaterial;
            }
        }

        public void GameExit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
            Application.Quit();
        }

        public void Credit()
        {
            SceneManager.Instance.LoadSceneWithFade("sn-Credit").Forget();
        }
    }
}
