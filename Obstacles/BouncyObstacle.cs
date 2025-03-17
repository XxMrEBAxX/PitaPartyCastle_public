using System;
using UB.EVENT;
using UnityEngine;

namespace UB
{
    [RequireComponent(typeof(UmbrellaInteractive))]
    [RequireComponent(typeof(PlayerAttackInteractive))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class BouncyObstacle : MonoBehaviour
    {
        [Tooltip("벽차기 당할 시 힘 강도")]
        [SerializeField] private float force = 1000;
        [Tooltip("제동력")]
        [SerializeField] private float deccel = 50;
        [Tooltip("돌아갈 속력")]
        [SerializeField] private float speed = 5;
        [Tooltip("공격 대기시간")]
        [SerializeField] private float attackCoolTime = 1;
        [Tooltip("데미지")]
        [SerializeField] private int damage = 10;

        private Rigidbody2D _rigidBody;
        private float _deccelAmount;
        private Vector2 _dir = Vector2.zero;
        private Vector3 _originPos;
        private UmbrellaInteractive _umbrellaInteractive;
        private PlayerAttackInteractive _playerAttackInteractive;
        private CameraManager _camera;
        private float _lastAttackTime = 0;
        private float _lastForceTime = 0;

        private void Start()
        {
            _rigidBody = GetComponent<Rigidbody2D>();
            _umbrellaInteractive = GetComponent<UmbrellaInteractive>();
            _playerAttackInteractive = GetComponent<PlayerAttackInteractive>();
            _camera = Camera.main.transform.GetComponent<CameraManager>();

            _umbrellaInteractive.UmbrellaEventHandler += SetBouncy;
            _playerAttackInteractive.AttackEventHandler += SetBouncy;

            _deccelAmount = 50 * deccel / force;
            _originPos = transform.position;
        }

        public void SetBouncy()
        {
            if (_lastForceTime > 0)
                return;

            _dir = Umbrella.Instance.Direction;

            _rigidBody.isKinematic = false;
            _rigidBody.AddForce(_dir * force, ForceMode2D.Force);
            _lastForceTime = 0.2f;
        }

        private void FixedUpdate()
        {
            _lastAttackTime -= TimeManager.Instance.GetDeltaTime();
            _lastForceTime -= TimeManager.Instance.GetDeltaTime();

            if (_rigidBody.velocity != Vector2.zero)
            {
                _rigidBody.isKinematic = false;
                _rigidBody.AddForce(-_rigidBody.velocity * _deccelAmount, ForceMode2D.Force);
                if (MathF.Abs(_rigidBody.velocity.magnitude) < 1)
                {
                    _rigidBody.velocity = Vector2.zero;
                    _rigidBody.isKinematic = true;
                }
            }
            else if (_originPos != transform.position && _rigidBody.velocity == Vector2.zero)
            {
                Vector2 dir = (_originPos - transform.position).normalized;
                _rigidBody.position = (Vector2)transform.position + dir * speed * TimeManager.Instance.GetDeltaTime();
                if ((_originPos - transform.position).magnitude < 0.1f)
                {
                    _rigidBody.position = _originPos;
                    _rigidBody.isKinematic = true;
                }
            }
        }
        
        // 바디 어택
        private void OnCollisionStay2D(Collision2D other)
        {
            if (other.collider.tag == "Player")
            {
                if (attackCoolTime - _lastAttackTime > attackCoolTime)
                {
                    Player.Instance.SubtractHP(damage);
                    _lastAttackTime = attackCoolTime;
                }
            }
        }
    }
}
