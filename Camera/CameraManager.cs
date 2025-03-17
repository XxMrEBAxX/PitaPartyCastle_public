using System.Collections;
using UnityEngine;
using Cinemachine;
using DG.Tweening;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace UB
{
    public enum CameraShakeType
    {
        Attack,
        Damaged,
        Teleprot,
        TeleprotWithAttack,
        Parry,
    }

    public class CameraManager : Singleton<CameraManager>
    {
        #region Inspector_Definition
        [SerializeField] private Volume _globalVolume;

        [SerializeField] private CinemachineImpulseSource _attackImpulseSource;
        [SerializeField] private float _attackImpulseForce;
        [SerializeField] private float _attackTimeStopDuration = 0.05f;

        [SerializeField] private CinemachineImpulseSource _damagedImpulseSource;
        [SerializeField] private float _damagedImpulseForce;
        [SerializeField] private Color _damageColor = Color.red;
        [SerializeField] private float _damageVignetteIntensity = 0.4f;
        [SerializeField] private float _damageChromaticIntensity = 0.2f;
        [SerializeField] private float _damagePlayTime = 0.2f;
        public float DamagePlayTime => _damagePlayTime;

        [SerializeField] private CinemachineImpulseSource _teleprotImpulseSource;
        [SerializeField] private float _teleprotImpulseForce;
        [SerializeField] private float _teleprotChromaticIntensity = 0.2f;
        [SerializeField] private float _teleprotPlayTime = 0.2f;
        #endregion

        private Vignette _vignette;
        private ChromaticAberration _chromatic;

        private Color _vignetteColor;
        private float _vignetteIntensity;


        private Camera _mainCam;
        private CinemachineVirtualCamera _cinemachineVirtualCamera;
        private CinemachineFramingTransposer _cinemachineFramingTransposer;
        private CinemachineConfiner2D _cinemachineConfiner;
        public float DefaultCameraDistanceValue { get; private set; }
        public float DefaultDampingValue { get; private set; }

        protected override void Awake()
        {
            CinemachineImpulseManager.Instance.IgnoreTimeScale = true;
            
            base.Awake();
            _mainCam = GetComponent<Camera>();
            _cinemachineVirtualCamera = GetComponentInChildren<CinemachineVirtualCamera>();
            _cinemachineConfiner = GetComponentInChildren<CinemachineConfiner2D>();
            CinemachineComponentBase componentBase = _cinemachineVirtualCamera.GetCinemachineComponent(CinemachineCore.Stage.Body);
            if (componentBase is CinemachineFramingTransposer)
            {
                _cinemachineFramingTransposer = componentBase as CinemachineFramingTransposer;
                DefaultCameraDistanceValue = _cinemachineFramingTransposer.m_CameraDistance;
            }
            else
            {
                Debug.Assert(false, "CinemachineFramingTransposer is not found in the camera component");
            }

            DefaultDampingValue = _cinemachineConfiner.m_Damping;
            
            SetVignette();
            SetChromatic();
        }

        private void SetVignette()
        {
            _globalVolume.profile.TryGet(out _vignette);

            _vignetteColor = _vignette.color.value;
            _vignetteIntensity = _vignette.intensity.value;
        }
        private void SetChromatic()
        {
            _globalVolume.profile.TryGet(out _chromatic);
        }

        public void SetCameraDistance(float distance)
        {
            _cinemachineFramingTransposer.m_CameraDistance = distance;
        }

        public void DefaultCameraDistance()
        {
            _cinemachineFramingTransposer.m_CameraDistance = DefaultCameraDistanceValue;
        }

        public void SetFollowCamera(bool follow)
        {
            _cinemachineVirtualCamera.Follow = follow ? Player.Instance.transform : null;
        }

        public void SetDamping(float damping)
        {
            _cinemachineConfiner.m_Damping = damping;
        }

        public void DefaultDamping()
        {
            _cinemachineConfiner.m_Damping = DefaultDampingValue;
        }

        public void ShakeCam(CameraShakeType type, Vector2 dir)
        {
            if(type == CameraShakeType.Attack)
            {
                _attackImpulseSource.GenerateImpulseWithVelocity(dir * _attackImpulseForce);
                StartCoroutine(shakeTimeStop(_attackTimeStopDuration));
            }
            else if(type == CameraShakeType.Damaged)
            {
                _damagedImpulseSource.GenerateImpulseWithVelocity(dir * _damagedImpulseForce);
                PlayDamageEffect();
            }
            else if (type == CameraShakeType.Teleprot)
            {
                _teleprotImpulseSource.GenerateImpulseWithVelocity(dir * _teleprotImpulseForce);
                StartCoroutine(shakeChromatic(_teleprotPlayTime));
            }
            else if (type == CameraShakeType.TeleprotWithAttack)
            {
                _attackImpulseSource.GenerateImpulseWithVelocity(dir * _attackImpulseForce);
                StartCoroutine(shakeChromatic(_teleprotPlayTime));
            }
            else if( type == CameraShakeType.Parry)
            {
                _attackImpulseSource.GenerateImpulseWithVelocity(dir * _attackImpulseForce);
                StartCoroutine(shakeTimeStop(_attackTimeStopDuration));
            }
        }

        public void ShakeCam(float a, float b)
        { //레거시 코드 전부 교체 후 삭제 요망
            _attackImpulseSource.GenerateImpulseWithVelocity(new Vector2(1, 0) * _attackImpulseForce);
            StartCoroutine(shakeTimeStop(_attackTimeStopDuration));
        }

        private IEnumerator shakeTimeStop(float duration)
        {
            TimeManager.Instance.SetTimeScale(0);

            yield return new WaitForSecondsRealtime(duration);

            TimeManager.Instance.SetTimeScale(1);
        }

        private IEnumerator shakeChromatic(float duration)
        {
            _chromatic.intensity.value = _teleprotChromaticIntensity;
            yield return new WaitForSecondsRealtime(duration);
            _chromatic.intensity.value = 0;
        }

        private void PlayDamageEffect()
        {
            var duration = _damagePlayTime * 0.5f;
            var sequence = DOTween.Sequence();


            //var seq = DOTween.Sequence();
            //seq.Join(DOTween.To(() => _vignette.intensity.value, x => _vignette.intensity.value = x,
            //    _damageVignetteIntensity, duration).SetEase(Ease.OutBounce));

            //seq.Join(DOTween.To(() => _vignette.color.value, x => _vignette.color.value = x,
            //    _damageColor, duration).SetEase(Ease.OutBounce));
            //seq.SetUpdate(true);
            //sequence.Append(seq);


            //seq = DOTween.Sequence();
            //seq.Join(DOTween.To(() => _vignette.intensity.value, x => _vignette.intensity.value = x,
            //    _vignetteIntensity, duration).SetEase(Ease.OutQuad));

            //seq.Join(DOTween.To(() => _vignette.color.value, x => _vignette.color.value = x,
            //    _vignetteColor, duration).SetEase(Ease.OutQuad));
            ////seq.SetUpdate(true);
            //sequence.Append(seq);


            sequence.Insert(0, DOTween.To(() => _vignette.intensity.value, x => _vignette.intensity.value = x,
                _damageVignetteIntensity, 0));
            sequence.Insert(0, DOTween.To(() => _vignette.color.value, x => _vignette.color.value = x,
                _damageColor, 0));

            sequence.Insert(_damagePlayTime, DOTween.To(() => _vignette.intensity.value, x => _vignette.intensity.value = x,
                _vignetteIntensity, 0));
            sequence.Insert(_damagePlayTime, DOTween.To(() => _vignette.color.value, x => _vignette.color.value = x,
                _vignetteColor, 0));

            sequence.Insert(0, DOTween.To(() => _chromatic.intensity.value, x => _chromatic.intensity.value = x,
                _damageChromaticIntensity, 0));
            sequence.Insert(_damagePlayTime, DOTween.To(() => _chromatic.intensity.value, x => _chromatic.intensity.value = x,
                0, 0));


            //Ÿ�� ������ ����
            sequence.OnStart(() => TimeManager.Instance.SetTimeScale(0));
            sequence.OnComplete(() => TimeManager.Instance.SetTimeScale(1));


            sequence.SetUpdate(true);//Ÿ�ӽ����� 0������ �۵��ǰ� update ������� ����
            sequence.Play();
        }

    }

}