using UnityEngine;
using UnityEngine.UI;

namespace UB.UI
{
    public class PlayerHPUI : MonoBehaviour
    {
        [SerializeField] private Sprite _emptyHeart;
        [SerializeField] private Sprite _fullHeart;
        [SerializeField] private GameObject _heartGameObject;
        
        private GameObject[] _heart;
        private int _heartCount;
        
        private void Start() {
            _heartCount = PlayerMovement.Instance.PlayerDataInstance.MaxHP;

            _heart = new GameObject[_heartCount];
            _heart[0] = transform.GetChild(0).gameObject;

            for (int i = 1; i < _heartCount; i++)
            {
                _heart[i] = Instantiate(_heartGameObject, transform);
            }
        }

        /// <summary>
        /// 현재 체력을 기준으로 스프라이트를 변경합니다.
        /// </summary>
        /// <param name="currentHp: 현재 플레이어의 체력 -> UI 자식 개수보다 큰 경우 자식 개수로 변경됩니다."></param>
        public void SetHPUI(int currentHp)
        {   
            for (int i = 0; i < _heartCount; i++)
            {
                if (i < currentHp)
                {
                    _heart[i].GetComponent<Image>().sprite = _fullHeart;
                }
                else
                {
                    _heart[i].GetComponent<Image>().sprite = _emptyHeart;
                }
            }
        }
    }
}