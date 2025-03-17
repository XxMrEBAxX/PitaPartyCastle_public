using DG.Tweening;
using MyBox;
using UnityEngine;

namespace UB
{
    public class HitFX : MonoBehaviour
    {
        public MeshRenderer TargetRenderer;

        public float Duration = 0.5f;

        [SearchableEnum] public Ease EaseStyle = Ease.OutSine;

        private MaterialPropertyBlock _materialPropertyBlock;

        // Shader Property ID
        private static readonly int HitIntensity = Shader.PropertyToID("_HitIntensity");

        private void OnValidate()
        {
            TargetRenderer = GetComponent<MeshRenderer>();
        }

        private void Start()
        {
            _materialPropertyBlock = new MaterialPropertyBlock();
        }

        /// <summary>
        /// Hit 표현을 실행합니다.
        /// </summary>
        public void PlayHit()
        {
            DOTween.To(x =>
            {
                TargetRenderer.GetPropertyBlock(_materialPropertyBlock);
                _materialPropertyBlock.SetFloat(HitIntensity, x);
            }
            , 1, 0, Duration)
                .SetEase(EaseStyle)
                .OnUpdate(UpdateHitIntensity);
        }

        private void OnDestroy()
        {
            DOTween.Kill(this);
        }

        private void UpdateHitIntensity()
        {
            TargetRenderer.SetPropertyBlock(_materialPropertyBlock);
        }
    }
}