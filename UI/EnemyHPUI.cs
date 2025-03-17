using UnityEngine;
using UnityEngine.UI;

namespace UB.UI
{
    public class EnemyHPUI : MonoBehaviour
    {
        private RectTransform _rectTransform;
        private Image _hpImage;
        private float _fillAmount;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _hpImage = GetComponent<Image>();
        }

        /// <summary>
        /// UI를 회전시킵니다.
        /// </summary>
        /// <param name="yAngle: 회전할 각도"></param>
        public void RotateHPUI(float yAngle)
        {
            _rectTransform.localEulerAngles = new Vector3(0, yAngle, 0);
        }

        /// <summary>
        /// 체력 UI를 설정합니다.
        /// </summary>
        /// <param name="enemyMaxHP: 현재 체력"></param>
        /// <param name="enemyCurrentHP: 최대 체력"></param>
        public void SetHPFillAmount(float enemyMaxHP, float enemyCurrentHP)
        {
            _fillAmount = enemyCurrentHP / enemyMaxHP;
            _hpImage.fillAmount = _fillAmount;
        }
    }
}