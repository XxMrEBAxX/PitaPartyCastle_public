using UnityEngine;
using MyBox;

namespace UB
{
    [CreateAssetMenu(menuName = "Data/Enemy/MeleeEnemyData")]
    public class MeleeEnemyData : EnemyData
    {
        public enum MeleeEnemyLookAtDirection
        {
            RIGHT = 0,
            LEFT = 1
        };

        [Header("근접 적 관련")] 
        
        [Space(5), Header("정찰")]
        
        [OverrideLabel("정찰 시작 방향"), SerializeField]
        private MeleeEnemyLookAtDirection _patrolInitDirection;
        public MeleeEnemyLookAtDirection PatrolInitDirection => _patrolInitDirection;

        [OverrideLabel("정찰 속도")] 
        [SerializeField] private float _patrolSpeed = 1f;
        public float PatrolSpeed => _patrolSpeed;

        [OverrideLabel("정찰 거리")] 
        [SerializeField] private float _patrolDistance = 5f;
        public float PatrolDistance => _patrolDistance;

        [Space(5)] [Header("추적")] 
        
        [OverrideLabel("X축 공격 시전 범위")] 
        [SerializeField] private float _attackCastingRangeX = 1f;
        public float AttackCastingRangeX => _attackCastingRangeX;
        
        [OverrideLabel("Y축 공격 시전 범위")] 
        [SerializeField] private float _attackCastingRangeY = 1f;
        public float AttackCastingRangeY => _attackCastingRangeY;
        
        [OverrideLabel("추적 해제 거리")] 
        [SerializeField] private float _chaseCancelDistance = 10f;
        public float ChaseCancelDistance => _chaseCancelDistance;

        [Space(5)] [Header("공격 범위")] 
        
        [OverrideLabel("X축 공격 범위")] 
        [SerializeField] private float _attackRangeX = 1f;
        public float AttackRangeX => _attackRangeX;
        
        [OverrideLabel("Y축 공격 범위")] 
        [SerializeField] private float _attackRangeY = 1f;
        public float AttackRangeY => _attackRangeY;
        
        [Space(5), Header("시야")] 
        
        [OverrideLabel("시야 거리"), SerializeField]
        private float _sightDistance = 5f;
        public float SightDistance => _sightDistance;

        [OverrideLabel("시야 높이"), SerializeField]
        private float _sightHeight = 4f;
        public float SightHeight => _sightHeight;

        // [OverrideLabel("시야 각도"), SerializeField]
        // private float _sightAngle = 30f;
        // public float SightAngle => _sightAngle;
    }
}