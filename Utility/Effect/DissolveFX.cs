using DG.Tweening;
using MyBox;
using UnityEngine;

namespace UB.EFFECT
{
    public class DissolveFX : MonoBehaviour
    {
        public MeshRenderer TargetRenderer;

        public float Duration = 0.5f;

        [SearchableEnum] public Ease EaseStyle = Ease.OutSine;

        private MaterialPropertyBlock _materialPropertyBlock;

        // Shader Property ID
        private static readonly int Level = Shader.PropertyToID("_Level");
        private static readonly int Edges = Shader.PropertyToID("_Edges");

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
        public void PlayHit(bool isRollback = true)
        {
            DOTween.To(x =>
                {
                    TargetRenderer.GetPropertyBlock(_materialPropertyBlock);
                    _materialPropertyBlock.SetFloat(Level, x);
                    _materialPropertyBlock.SetFloat(Edges, x * 0.1f);
                }, 0, 1, Duration)
                .SetEase(EaseStyle)
                .OnUpdate(UpdateDissolveIntensity)
                .OnComplete(() =>
                {
                    if(!isRollback)
                        return;
                    SetDissolve(0);
                    UpdateDissolveIntensity();
                });
        }

        public void PlayHitReverse(bool isRollback = true)
        {
            DOTween.To(x =>
                {
                    TargetRenderer.GetPropertyBlock(_materialPropertyBlock);
                    _materialPropertyBlock.SetFloat(Level, x);
                    _materialPropertyBlock.SetFloat(Edges, x * 0.1f);
                }, 1, 0, Duration)
                .SetEase(EaseStyle)
                .OnUpdate(UpdateDissolveIntensity)
                .OnComplete(() =>
                {
                    if(!isRollback)
                        return;
                    SetDissolve(1);
                    UpdateDissolveIntensity();
                });
        }

        public void SetDissolve(float value)
        {
            TargetRenderer.GetPropertyBlock(_materialPropertyBlock);
            _materialPropertyBlock.SetFloat(Level, value);
            _materialPropertyBlock.SetFloat(Edges, value);
            UpdateDissolveIntensity();
        }

        private void OnDestroy()
        {
            DOTween.Kill(this);
        }

        private void UpdateDissolveIntensity()
        {
            TargetRenderer.SetPropertyBlock(_materialPropertyBlock);
        }
    }
}