using UnityEngine;
using UnityEngine.UIElements.Experimental;

namespace UB
{
    public class StickUmbrella : Singleton<StickUmbrella>
    {
        public Transform LeftLedgeTransform;
        public Transform RightLedgeTransform;
        public Transform SlidingTransform;
        public Transform BatRushTransform;

        public Vector2 CeilingStickPosition
        {
            get
            {
                return (Vector2)_stickUmbrellaCeilingCollider.bounds.center + (Vector2.down * 1.1f);
            }
            private set
            {
                CeilingStickPosition = value;
            }
        }

        private Umbrella _umbrella;
        private PlayerMovement _playerMovement;
        private GameObject _stickUmbrellaPillar;
        private GameObject _stickUmbrellaCeiling;
        private BoxCollider2D _stickUmbrellaCeilingCollider;
        private LineRenderer _lineRenderer;
        private GameObject _spriteRendererObject;
        private SpriteRenderer _spriteRenderer;

        private bool _active;
        public bool Active
        {
            get { return _active; }
            set
            {
                _lineRenderer.enabled = value;
                _active = value;
                Update();
            }
        }

        protected override void Awake()
        {
            base.Awake();
            _stickUmbrellaPillar = transform.Find("Pillar").gameObject;
            _stickUmbrellaCeiling = transform.Find("Ceiling").gameObject;
            _spriteRendererObject = transform.Find("Sprite").gameObject;

            _stickUmbrellaCeilingCollider = _stickUmbrellaCeiling.GetComponent<BoxCollider2D>();
            _spriteRenderer = _spriteRendererObject.GetComponent<SpriteRenderer>();
            _lineRenderer = GetComponent<LineRenderer>();
        }

        private void Start()
        {
            _playerMovement = PlayerMovement.Instance;
            _umbrella = Umbrella.Instance;
        }

        private void Update()
        {
            if (_active)
            {
                _lineRenderer.SetPosition(0, _playerMovement.transform.position);
                _lineRenderer.SetPosition(1, GetBoundsCenter());
                _umbrella.transform.position = GetBoundsCenter() + (_umbrella.HitWallVector * (_spriteRenderer.size.y * 0.5f));
            }
        }

        public void SetCollider(Vector2 hitWallVector)
        {
            // 땅에 박힌게 아니면 콜라이더 해제
            if (hitWallVector == Vector2.zero || Umbrella.Instance.HitThrowLayer == LayerMask.GetMask(LayerEnum.AbleStick.ToString()))
            {
                _stickUmbrellaPillar.SetActive(false);
                _stickUmbrellaCeiling.SetActive(false);
                return;
            }

            _stickUmbrellaPillar.SetActive(true);
            _stickUmbrellaCeiling.SetActive(false);
            if (hitWallVector.y > 0.01f) // 바닥
            {
                _stickUmbrellaPillar.SetActive(false);
            }
            else if (hitWallVector.y < -0.01f) // 천장
            {
                _stickUmbrellaPillar.SetActive(false);
                _stickUmbrellaCeiling.SetActive(true);
            }
        }
        /// <summary>
        /// 콜라이더를 해제합니다.
        /// </summary>
        public void DisableCollider()
        {
            _stickUmbrellaPillar.SetActive(false);
            _stickUmbrellaCeiling.SetActive(false);
        }

        public void Disable()
        {
            _spriteRendererObject.SetActive(false);
            Active = false;
            DisableCollider();
        }

        public void Enable()
        {
            transform.position = _umbrella.StickHitPoint;
            _spriteRendererObject.SetActive(true);
            Active = true;
            SetSpriteActive(true);
        }

        public bool GetActive()
        {
            return _spriteRendererObject.activeSelf;
        }

        public void SetSpriteActive(bool value)
        {
            _spriteRenderer.enabled = value;
        }

        public Vector2 GetBoundsCenter()
        {
            return _spriteRenderer.bounds.center;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            bool isCeilingStick = _umbrella.CurrentUmbrellaStateEnum == UmbrellaState.STICK && _stickUmbrellaCeiling.activeSelf;
            if (other.CompareTag("Player") && isCeilingStick && _playerMovement.CanMoveState())
            {
                _playerMovement.LedgePosition(CeilingStickPosition, gameObject.tag);
                _stickUmbrellaCeiling.SetActive(false);
            }
        }
    }

}