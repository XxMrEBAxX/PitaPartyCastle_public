using UnityEngine;
using UnityEngine.UI;

namespace UB.UI
{
    public class EnemyShieldUI : MonoBehaviour
    {
        private RectTransform _rectTransform;
        private Image _shieldImage;
        private float _fillAmount;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _shieldImage = GetComponent<Image>();
        }

        /// <summary>
        /// UI를 회전시킵니다.
        /// </summary>
        /// <param name="yAngle: 회전할 각도"></param>
        public void RotateShieldUI(float yAngle)
        {
            Vector3 rotation = new Vector3(0, yAngle, 0);
            _rectTransform.localEulerAngles = rotation;
        }

        public void SetShieldUIFillAmount(float enemyMaxShield, float enemyCurrentShield)
        {
            _fillAmount = enemyCurrentShield / enemyMaxShield;
            _shieldImage.fillAmount = _fillAmount;
        }
    }
}