using UnityEngine;
using UnityEngine.UI;

namespace UB
{
    public class BackGroundUI : MonoBehaviour
    {
        [SerializeField] private GameObject[] _gameObjects;
        private Image _image;

        private void Start()
        {
            _image = GetComponent<Image>();

        }

        private void Update()
        {
            for (int i = 0; i < _gameObjects.Length; i++)
            {
                if (_gameObjects[i].activeSelf)
                {
                    _image.enabled = true;
                    return;
                }
            }
            _image.enabled = false;
        }
    }
}
