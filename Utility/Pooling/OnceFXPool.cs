using UnityEngine;

namespace UB
{
    [RequireComponent(typeof(ParticleSystem))]
    // 한번만 실행되는 파티클 풀링 오브젝트 스크립트 입니다. Not Loop
    public class OnceFXPool : PoolAble
    {
        private ParticleSystem _particleSystem;

        void Awake()
        {
            _particleSystem = GetComponent<ParticleSystem>();
        }

        void OnEnable()
        {
            _particleSystem.Play();
        }

        void Update()
        {
            if (!_particleSystem.isPlaying)
            {
                // 오브젝트 풀에 반환
                ReleaseObject();
            }
        }
    }
}