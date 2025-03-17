using MyBox;
using UnityEngine;

namespace UB
{
    [CreateAssetMenu(menuName = "Data/Enemy/EnemyData")]
    public class EnemyData : ScriptableObject
    {
        [Header("스테이터스")] [OverrideLabel("최대 체력"), SerializeField]
        private int _maxHealth = 10;

        public int MaxHealth => _maxHealth;

        [OverrideLabel("이동 속도"), SerializeField]
        private float _moveSpeed = 5f;

        public float MoveSpeed => _moveSpeed;

        [Space(5), Header("공격관련 변수")] [OverrideLabel("공격력"), SerializeField]
        private int _damage = 4;

        public int Damage => _damage;

        [OverrideLabel("공격 선딜레이(준비 시간)"), SerializeField]
        private float _prepareDelayAttack = 0.5f;

        public float PrepareDelayAttack => _prepareDelayAttack;

        [OverrideLabel("공격 딜레이"), SerializeField]
        private float _dealayAttack = 0.5f;

        public float DelayAttack => _dealayAttack;

        [OverrideLabel("공격 쿨타임(후딜레이)"), SerializeField]
        private float _attackCooldown = 2f;

        public float AttackCooldown => _attackCooldown;

        [OverrideLabel("백어택 허용 여부"), SerializeField]
        private bool _enableBackAttack = true;

        public bool EnableBackAttack => _enableBackAttack;

        [Space(5), Header("Stretch(늘어남) 관련")]
        [OverrideLabel("처음 늘어남 스케일"), SerializeField]
        private float _startStretchScale = 0.65f;

        public float StartStretchScale => _startStretchScale;

        [OverrideLabel("늘어남 지속시간"), SerializeField]
        private float _stretchDuration = 0.2f;

        public float StretchDuration => _stretchDuration;

        [Space(5), Header("바디 어택")] // PC와 충돌처리 시 데미지 여부
        [OverrideLabel("바디 어택 여부"), SerializeField]
        private bool _canBodyAttack = true;

        public bool CanBodyAttack => _canBodyAttack;

        [OverrideLabel("바디 어택 데미지"), SerializeField]
        private int _bodyAttackDamage = 5;

        public int BodyAttackDamage => _bodyAttackDamage;

        [Space(5), Header("죽음 관련")] [OverrideLabel("Die 애니메이션이 끝난 후 시체 사라지는 시간"), SerializeField]
        private float _disappearDeadBodyDelayTime = 0.3f;

        public float DisappearDeadBodyDelayTime => _disappearDeadBodyDelayTime;

        [OverrideLabel(("시체 사라지는 여부")), SerializeField]
        private bool _isDisappearDeadBody = true;

        public bool IsDisappearDeadBody => _isDisappearDeadBody;
    }
}