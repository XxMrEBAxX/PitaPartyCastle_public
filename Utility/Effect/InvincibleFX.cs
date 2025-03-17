using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using MyBox;
using UnityEngine;

namespace UB.EFFECT
{
    public class InvincibleFX : MonoBehaviour
    {
        public MeshRenderer TargetRenderer;

        public float Duration = 0.5f;

        [SearchableEnum]
        public Ease EaseStyle = Ease.OutSine;

        private Color _color;
        private CancellationTokenSource _source = new();

        private void OnValidate()
        {
            TargetRenderer = GetComponent<MeshRenderer>();
        }

        private void Start()
        {
            _color = new Color();
        }

        private Color SetAndGetColor(float r, float g, float b, float a = 1)
        {
            _color.r = r;
            _color.g = g;
            _color.b = b;
            _color.a = a;

            return _color;
        }

        /// <summary>
        /// Invincible 표현을 실행합니다.
        /// </summary>
        public void PlayFX(float duration = 0)
        {
            if (duration == 0)
                duration = Duration;

            OnDestroy();
            RepeatHit(duration).Forget();
        }

        public void StopFX()
        {
            OnDestroy();
        }

        private async UniTaskVoid RepeatHit(float duration)
        {
            _source = new();
            float curTime = TimeManager.Instance.GetTime();
            while (true)
            {
                if (curTime + duration < TimeManager.Instance.GetTime() || _source.Token.IsCancellationRequested)
                    break;

                DOTween.To(x =>
                {
                    TargetRenderer.material.SetColor("_Color", SetAndGetColor(x, x, x));
                }, 1, 0, Duration * 0.5f)
                .SetEase(EaseStyle).onComplete = () =>
                {
                    DOTween.To(x => { TargetRenderer.material.SetColor("_Color", SetAndGetColor(x, x, x)); }, 0, 1, Duration * 0.5f)
                .SetEase(EaseStyle);
                };
                await UniTask.Delay((int)(Duration * 1000), cancellationToken: _source.Token);
            }
            TargetRenderer.material.SetColor("_Color", SetAndGetColor(1, 1, 1));
            DOTween.Kill(this);
        }

        private void OnDestroy()
        {
            if (_source != null)
            {
                _source.Cancel();
                _source.Dispose();
                _source = null;
            }
            TargetRenderer.material.SetColor("_Color", SetAndGetColor(1, 1, 1));
            DOTween.Kill(this);
        }

        private void OnDisable()
        {
            OnDestroy();
        }
    }
}
