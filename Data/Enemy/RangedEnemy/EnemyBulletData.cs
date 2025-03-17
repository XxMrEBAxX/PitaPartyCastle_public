using UnityEngine;
using MyBox;
using UnityEngine.Serialization;

namespace UB
{
    [CreateAssetMenu(menuName = "Data/Enemy/Ranged Enemy/EnemyBulletData")]
    public class EnemyBulletData : ScriptableObject
    {
        [Space(5), Header("탄알 관련")] 
        
        [SerializeField, OverrideLabel("탄알 속도")]
        private float _bulletVelocity = 30f;
        public float BulletVelocity => _bulletVelocity;

        [SerializeField, OverrideLabel("탄알 최대 거리")]
        private float _bulletRange = 15f;
        public float BulletRange => _bulletRange;

        [Space(5), Header("반동 관련")] 
        
        [SerializeField, OverrideLabel("반동 여부")]
        private bool _isRecoil = true;
        public bool IsRecoil => _isRecoil;

        [SerializeField, OverrideLabel("반동력")] 
        private float _recoil;
        public float Recoil => _recoil;

        [Space(5), Header("패링")] 
        
        [SerializeField, OverrideLabel("패링 이동 보너스 배율")]
        private float _parryingBonusVelocity = 2f;
        public float ParryingBonusVelocity => _parryingBonusVelocity;
        
        [SerializeField, OverrideLabel("패링 데미지 보너스 배율")]
        private int _parryingBonusDamage = 20;
        public int ParryingBonusDamage => _parryingBonusDamage;

        [SerializeField, OverrideLabel("패링 추적 여부")]
        private bool _parryingTracking = false;
        public bool ParryingTracking => _parryingTracking;

        [SerializeField, OverrideLabel("패링 가능 여부")]
        private bool _enableParrying;
        public bool EnableParrying => _enableParrying;
    }
}