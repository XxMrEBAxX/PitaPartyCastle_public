using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UB.EVENT;
using System;
using UB.Animation;

namespace UB
{
    public partial class Umbrella : Singleton<Umbrella>
    {
        #region Inspector

        [Tooltip("0번째 스프라이트는 접은 우산, 1번째 스프라이트는 펼친 우산")]
        public Sprite[] umbrellaSprite = new Sprite[2];

        [Tooltip("공격 가능한 레이어")]
        [SerializeField]
        private LayerMask attackLayer;

        [Tooltip("우산 던지기에 꽃힐 수 있는 레이어")] public LayerMask AbleStickLayer;

        [Tooltip("우산 던지기 돌진 시 무시할 레이어")]
        [SerializeField]
        private LayerMask ignoreRushLayer;

        #endregion

        #region Objects & instance

        private GameObject _checks;
        private SpriteRenderer _spriteRenderer;
        private PlayerMovement _playerMovement;
        private PlayerData _playerData;
        private Player _player;
        private BoxCollider2D _attackRange;
        private CameraManager _cameraManager;
        private Camera _camera;
        private StickUmbrella _stickUmbrella;
        private PlayerAnimation _playerAnimation;
        private MouseDotted _mouseDotted;
        private TrailRenderer _trailRenderer;

        #endregion

        #region Timers

        public float LastOnUmbrellaSleepTime { get; private set; }
        public float LastOnBatRushButtonTime { get; private set; }

        /// <summary>
        /// 0 보다 크면 패링 인정 시간입니다.
        /// </summary>
        public float LastParryButtonTime { get; private set; }

        public float LastOnAttackTime { get; private set; }
        public float LastOnAttackCoolTime { get; private set; }
        public float LastOnTargetingTime { get; private set; }
        public float LastOnUmbrellaThrowCoolTime { get; private set; }

        #endregion

        #region Input Parameter

        public bool IsHoldAttackButton { get; private set; }
        public bool IsHoldBatRushButton { get; private set; }
        public bool IsHoldGlidingButton { get; private set; }
        public bool IsHoldThrowButton { get; private set; }

        #endregion

        private float _rotZ = 0;
        private List<int> _hitAttackList = new List<int>();
        private float _throwSpeed;
        private float _returnSpeed;
        private float _maxThrowDistance;
        private Vector2 _originLocalPos;
        private Vector2 _originThrewWorldPos;
        private bool _returnStick;
        private Vector2 _previousThrowPosition;
        private int _bouncesCount = 0;
        private Coroutine _ThrowSlowTimeRoutine;
        public Vector2 TargetAimPosition { get; private set; }

        public Vector2 Direction { get; private set; }
        public bool IsUmbrellaFixed { get; private set; }
        public bool IsThrewUmbrella { get; private set; }
        public bool IsAbleReturnUmbrella { get; private set; }
        public Vector2 StickHitPoint { get; set; } = Vector2.zero;
        public Vector2 HitWallVector { get; private set; } = Vector2.zero;
        public int HitThrowLayer { get; private set; }
        public GameObject HitObject { get; private set; }
        public UmbrellaInteractive curAbleStickObstacle { get; set; }

        public bool ReturnStick
        {
            get { return _returnStick; }
            set
            {
                _returnStick = value;
                if (_returnStick) SoundManager.Instance.PlaySFX(SoundManager.SFXNames.Return);
            }
        }

        public int CountThrowAirHolding { get; set; }
        public int CountAttackHolding { get; set; }

        protected override void Awake()
        {
            base.Awake();
            StartCashing();
        }

        private void Start()
        {
            StartInitializing();
        }

        private void StartInitializing()
        {
            _throwSpeed = _playerData.UmbrellaThrowSpeed;
            _returnSpeed = _playerData.UmbrellaReturnSpeed;
            _maxThrowDistance = _playerData.UmbrellaThrowMaxDistance;
            //_mouseDotted.Active = true;
            _originLocalPos = transform.localPosition;
            _spriteRenderer.gameObject.SetActive(false);
            _trailRenderer.emitting = false;
            _playerAnimation.PlayForceAnimation(PlayerAnimationEnum.NONE, PlayerAnimationLayer.AIM);
            HitObject = null;

            _stickUmbrella.transform.SetParent(null);
            _stickUmbrella.Disable();
        }

        private void StartCashing()
        {
            _player = Player.Instance;
            _playerMovement = PlayerMovement.Instance;
            _playerAnimation = PlayerAnimation.Instance;
            _mouseDotted = MouseDotted.Instance;
            _playerData = _playerMovement.PlayerDataInstance;
            _trailRenderer = GetComponent<TrailRenderer>();
            _cameraManager = CameraManager.Instance;
            _camera = _cameraManager.GetComponent<Camera>();
            //REVIEW -  의존성이 강한 코드다.
            _attackRange = transform.Find("Checks").transform.Find("AttackRange").GetComponent<BoxCollider2D>();
            _spriteRenderer = transform.Find("Sprite").GetComponent<SpriteRenderer>();
            _checks = transform.Find("Checks").gameObject;
            _stickUmbrella = transform.Find("StickUmbrella").GetComponent<StickUmbrella>();
            //
        }

        private void Update()
        {
            if (!_player.UnlockUmbrella)
                return;

            SubtractTimeFromTimers();

            UpdateState();

            //FIXME - 우산 시스템 변경 시 수정될 코드
            if (!IsThrewUmbrella)
            {
                bool isPressGlidingButtonWhileAbleGliding = IsHoldGlidingButton && !IsUmbrellaFixed;
                if (isPressGlidingButtonWhileAbleGliding || CurrentUmbrellaStateEnum == UmbrellaState.FOLD)
                    Direction = Vector2.up;

                SetRotationUmbrella(Direction);
            }
        }

        private void SubtractTimeFromTimers()
        {
            LastOnUmbrellaSleepTime = Mathf.Clamp(LastOnUmbrellaSleepTime - TimeManager.Instance.GetDeltaTime(), 0, float.MaxValue);
            LastOnBatRushButtonTime = Mathf.Clamp(LastOnBatRushButtonTime - TimeManager.Instance.GetDeltaTime(), 0, float.MaxValue);
            LastParryButtonTime = Mathf.Clamp(LastParryButtonTime - TimeManager.Instance.GetDeltaTime(), 0, float.MaxValue);
            LastOnAttackTime = Mathf.Clamp(LastOnAttackTime - TimeManager.Instance.GetDeltaTime(), 0, float.MaxValue);
            LastOnAttackCoolTime = Mathf.Clamp(LastOnAttackCoolTime - TimeManager.Instance.GetDeltaTime(), 0, float.MaxValue);
            LastOnTargetingTime = Mathf.Clamp(LastOnTargetingTime - TimeManager.Instance.GetDeltaTime(), 0, float.MaxValue);
            LastOnUmbrellaThrowCoolTime = Mathf.Clamp(LastOnUmbrellaThrowCoolTime - TimeManager.Instance.GetDeltaTime(), 0, float.MaxValue);
        }

        #region InputEvent

        public void OnPressAttackButton(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                if (!_player.UnlockUmbrella || _player.CurPlayerState == Player.PlayerState.DEAD)
                    return;

                if (TimeManager.Instance.AbleSetTimeScale is false)
                    return;

                IsHoldAttackButton = true;
            }

            if (context.canceled)
            {
                IsHoldAttackButton = false;
            }
        }

        public void OnPressReturnButton(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                if (!_player.UnlockUmbrella || _player.CurPlayerState == Player.PlayerState.DEAD)
                    return;

                if (TimeManager.Instance.AbleSetTimeScale is false)
                    return;

                bool isUmbrellaLedge = (_playerMovement.LedgeTag == "Umbrella" &&
                                        _playerMovement.CurrentMovementStateEnum == PlayerMovementState.LEDGE)
                                       || _playerMovement.CurrentMovementStateEnum == PlayerMovementState.BATRUSH ||
                                       _playerMovement.CurrentMovementStateEnum == PlayerMovementState.RUSH;
                //if (CurrentUmbrellaStateEnum == UmbrellaState.STICK && !_returnStick && !isUmbrellaLedge)
                if (IsAbleReturnUmbrella && !_returnStick)
                {
                    ReturnStick = true;
                }
            }

            if (context.canceled)
            {
                // 나중에 쓸듯
            }
        }

        public void OnPressThrowButton(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                if (!_player.UnlockUmbrella || _player.CurPlayerState == Player.PlayerState.DEAD)
                    return;

                if (TimeManager.Instance.AbleSetTimeScale is false)
                    return;

                IsHoldThrowButton = true;

                if (!IsThrewUmbrella && LastOnUmbrellaThrowCoolTime == 0)
                {
                    ChangeState(StateUmbrellaThrow.Instance);
                }
            }

            if (context.canceled)
            {
                IsHoldThrowButton = false;
            }
        }

        public void OnPressBatRushButton(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                if (!_player.UnlockTeleport || _player.CurPlayerState == Player.PlayerState.DEAD)
                    return;

                if (TimeManager.Instance.AbleSetTimeScale is false)
                    return;

                IsHoldBatRushButton = true;
                LastOnBatRushButtonTime = _playerData.CoyoteTime;
            }

            if (context.canceled)
            {
                IsHoldBatRushButton = false;
            }
        }

        public void OnPressGlidingButton(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                if (!_player.UnlockGliding || _player.CurPlayerState == Player.PlayerState.DEAD)
                    return;

                if (TimeManager.Instance.AbleSetTimeScale is false)
                    return;

                IsHoldGlidingButton = true;
            }

            if (context.canceled)
            {
                IsHoldGlidingButton = false;
            }
        }

        #endregion

        private void SetAndGetMouseDir()
        {
            if (_mouseDotted.Active)
            {
                Direction = _mouseDotted.Direction;
            }
            else
            {
                Vector3 mousePos = Mouse.current.position.ReadValue();
                mousePos.z = -_camera.gameObject.transform.position.z;
                Vector2 mouseWorldPos = _camera.ScreenToWorldPoint(mousePos);
                Vector2 direction = mouseWorldPos - (Vector2)_player.transform.position;
                Direction = direction.normalized;
            }
        }

        private void SetRotationUmbrella(Vector2 direction)
        {
            if (!IsUmbrellaFixed)
                _rotZ = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90;

            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, 0, _rotZ),
                _playerData.UmbrellaRotationSpeed * TimeManager.Instance.GetDeltaTime());
        }

        public void SetUmbrellaFixed(bool value)
        {
            IsUmbrellaFixed = value;
        }

        private void StartBatRush()
        {
            _player.SetInvincible(_playerData.BatRushInvincibleTime);
            Vector2 batRushPosition;
            if (HitThrowLayer == LayerMask.GetMask(LayerEnum.AbleStick.ToString()))
            {
                if (curAbleStickObstacle.gameObject.TryGetComponent<Switch>(out var switchComponent))
                    return;

                batRushPosition = StickHitPoint;
            }
            else
            {
                if (Mathf.Abs(HitWallVector.y) < 0.01f)
                {
                    if (HitObject.TryGetComponent<GroundOptions>(out var groundOptions))
                    {
                        if (groundOptions.AbleSliding)
                        {
                            Vector2 slidingPos = _stickUmbrella.SlidingTransform.position;
                            if (_playerMovement.CheckBatRushSliding(slidingPos))
                            {
                                _playerMovement.StartBatRushOnWall(slidingPos);
                                return;
                            }
                        }
                    }
                }

                // 우산이 위에 박힘
                if (HitWallVector.y < -0.01f)
                {
                    batRushPosition = _stickUmbrella.CeilingStickPosition;
                }
                // 우산이 아래에 박힘
                else if (HitWallVector.y > 0.01f)
                {
                    batRushPosition = (Vector2)_stickUmbrella.BatRushTransform.position + Vector2.down * 0.1f;
                }
                // 우산이 왼쪽에 박힘
                else if (HitWallVector.x > 0.01f)
                {
                    batRushPosition = _stickUmbrella.LeftLedgeTransform.position;
                }
                // 우산이 오른쪽에 박힘
                else
                {
                    batRushPosition = _stickUmbrella.RightLedgeTransform.position;
                }
            }

            _playerMovement.StartBatRushOnWall(batRushPosition);

            //ANCHOR - Legacy BatRush Grab
            //StartCoroutine(CheckHoldBatRushRoutine(_playerData.BatRushGrabHoldTime));
        }

        private void StartThrowAttackBatRush(Vector2 pos)
        {
            _player.SetInvincible(_playerData.BatRushInvincibleTime);
            _playerMovement.StartBatRushOnObject(pos);
            LastOnUmbrellaThrowCoolTime = _playerData.UmbrellaThrowCoolTime;
            //StartSlowTime();
        }

        private void StartSlowTime()
        {
            _ThrowSlowTimeRoutine = StartCoroutine(ThrewSlowTime());
        }

        private IEnumerator<WaitUntil> ThrewSlowTime(bool denyMoveInput = false)
        {
            if (_ThrowSlowTimeRoutine != null)
                StopCoroutine(_ThrowSlowTimeRoutine);

            if (denyMoveInput)
            {
                _playerMovement.DenyRun = true;
                _playerMovement.SetVelocity(Vector2.zero);
            }

            float sumDeltaTime = 0;
            var lastFrame = _playerData.BatRushTimeScale[_playerData.BatRushTimeScale.length - 1];
            float lastKeyTime = lastFrame.time;
            while (sumDeltaTime < lastKeyTime)
            {
                yield return new WaitUntil(() => { return TimeManager.Instance.AbleSetTimeScale; });
                TimeManager.Instance.SetTimeScale(_playerData.BatRushTimeScale.Evaluate(sumDeltaTime));
                sumDeltaTime += TimeManager.Instance.GetUnscaledDeltaTime();
            }

            _playerMovement.DenyRun = false;
            TimeManager.Instance.SetOriginTimeScale();
        }

        public void EndBatRush()
        {
            _stickUmbrella.DisableCollider();
            if (HitWallVector.y > 0 && Mathf.Abs(HitWallVector.x) < 0.01f)
                ReturnStick = true;
        }

        private void CheckThrowHitLayer(Vector2 SizeY, RaycastHit2D hit)
        {
            HitWallVector = hit.normal;
            HitThrowLayer = 1 << hit.collider.gameObject.layer;
            HitObject = hit.collider.gameObject;
            StickHitPoint = hit.point;

            if (HitThrowLayer == LayerMask.GetMask(LayerEnum.Obstacle.ToString()))
            {
                ReturnStick = true;
                ChangeState(StateUmbrellaStick.Instance, true);
            }
            else if (HitThrowLayer == LayerMask.GetMask(LayerEnum.AbleStick.ToString()))
            {
                _stickUmbrella.Enable();
                _stickUmbrella.SetSpriteActive(false);
                StickHitPoint = (Vector2)hit.collider.bounds.center + SizeY * 0.5f;
                _stickUmbrella.transform.rotation = Quaternion.Euler(0, 0, _rotZ);

                curAbleStickObstacle = hit.collider.GetComponent<UmbrellaInteractive>() ?? null;
                if (curAbleStickObstacle != null)
                    curAbleStickObstacle.Call();

                SoundManager.Instance.PlaySFX(SoundManager.SFXNames.Hit);
                ChangeState(StateUmbrellaStick.Instance, true);
            }
            else if (HitThrowLayer == LayerMask.GetMask(LayerEnum.Enemy.ToString()))
            {
                TargetingEnemy(hit.collider.gameObject);
                //FIXME - 매직넘버 수정 요망 2.5f
                StartThrowAttackBatRush((Vector2)hit.collider.transform.position + Direction * 2);
                ForceReturnUmbrella();
            }
            else if (HitThrowLayer == LayerMask.GetMask(LayerEnum.Ground.ToString()))
            {
                if (hit.collider.TryGetComponent<GroundOptions>(out var groundOptions))
                {
                    if (groundOptions.AbleBounce)
                    {
                        if (_bouncesCount > _playerData.UmbrellaMaxBounceCount)
                        {
                            ReturnStick = true;
                            ChangeState(StateUmbrellaStick.Instance, true);
                        }
                        else
                        {
                            Direction = Direction + 2 * HitWallVector * Vector2.Dot(-Direction, HitWallVector);
                            SetRotationUmbrella(Direction);
                            _bouncesCount++;
                        }
                    }
                    else if (groundOptions.AbleStick is false)
                    {
                        ReturnStick = true;
                        ChangeState(StateUmbrellaStick.Instance, true);
                    }
                    // Stick 처리
                    // x, y값 중 하나가 0이라는 뜻은 법선 벡터가 수직을 보장한다는 것이므로
                    else if (Mathf.Abs(HitWallVector.x) <= 0.01f || Mathf.Abs(HitWallVector.y) <= 0.01f)
                    {
                        _stickUmbrella.Enable();

                        if (HitWallVector.x > 0.01f)
                            _stickUmbrella.transform.rotation = Quaternion.Euler(0, 0, 90);
                        else if (HitWallVector.x < -0.01f)
                            _stickUmbrella.transform.rotation = Quaternion.Euler(0, 0, 270);
                        else if (HitWallVector.y > 0.01f) // 바닥
                            _stickUmbrella.transform.rotation = Quaternion.Euler(0, 0, 180);
                        else // 천장
                            _stickUmbrella.transform.rotation = Quaternion.Euler(0, 0, 0);

                        if (hit.collider.TryGetComponent<MovementInteractive>(out var movementInteractive))
                        {
                            movementInteractive.Add(_stickUmbrella.gameObject);
                            movementInteractive.Execute();
                        }

                        ChangeState(StateUmbrellaStick.Instance, true);
                        SoundManager.Instance.PlaySFX(SoundManager.SFXNames.Hit);
                    }
                    else
                    {
                        ReturnStick = true;
                        ChangeState(StateUmbrellaStick.Instance);
                    }
                }
                else
                {
                    ReturnStick = true;
                    ChangeState(StateUmbrellaStick.Instance);
                    Debug.LogError("GroundOptions 컴포넌트가 없습니다.");
                }
            }
        }

        private void ReturnUmbrella()
        {
            Vector2 dir = (_playerMovement.transform.position - transform.position).normalized;
            Direction = dir;
            Vector2 destination = Direction * (_returnSpeed * TimeManager.Instance.GetDeltaTime());
            SetRotationUmbrella(Direction);

            //ANCHOR - 2024.05.22 - 회수 공격 삭제 요청
            // Vector2 SizeY = _spriteRenderer.sprite.bounds.size.y * Direction;
            // Vector2 curPosMagnitude = (Vector2)transform.position - _previousThrowPosition;

            // RaycastHit2D hit = Physics2D.Raycast(_previousThrowPosition, Direction,
            //     curPosMagnitude.magnitude + SizeY.magnitude, AbleStickLayer);
            transform.Translate(destination, Space.World);
            // _previousThrowPosition = transform.position;
            // if (!ReferenceEquals(hit.collider, null))
            // {
            //     HitWallVector = hit.normal;
            //     HitThrowLayer = 1 << hit.collider.gameObject.layer;
            //     if (HitThrowLayer == LayerMask.GetMask(LayerEnum.Enemy.ToString()))
            //     {
            //         TargetingEnemy(hit.collider.gameObject);
            //         //FIXME - 매직넘버 수정 요망 2
            //         StartThrowAttackBatRush(hit.point - Direction * 2);
            //         ForceReturnUmbrella();
            //         //StartSlowTime();
            //     }
            // }

            float LastReturnSqrMagnitude = (_playerMovement.transform.position - transform.position).sqrMagnitude;
            if (LastReturnSqrMagnitude <= 2)
            {
                transform.SetParent(_player.transform);
                transform.localPosition = _originLocalPos;
                ChangeState(StateUmbrellaFold.Instance, true);
                return;
            }

            // 공격
            Vector2 centerPoint = Direction * _throwSpeed * TimeManager.Instance.GetDeltaTime() +
                                  (Vector2)transform.position;
            Vector2 SizeAttackRange = umbrellaSprite[0].bounds.size;

            int layer = attackLayer & ~LayerMask.GetMask(LayerEnum.Enemy.ToString());
            Collider2D[] returnAttackList = Physics2D.OverlapBoxAll(centerPoint, SizeAttackRange, _rotZ, layer);
            if (returnAttackList.Length > 0)
            {
                foreach (var e in returnAttackList)
                {
                    ProcessAttack(e.gameObject, _player.AttackDamage);
                }
            }
#if UNITY_EDITOR
            returnAttackCenterPosition = centerPoint;
            returnAttackSize = SizeAttackRange;
            testQuaternion = Quaternion.Euler(0, 0, _rotZ);
#endif
        }

        /// <summary>
        /// 강제로 우산을 회수합니다.
        /// </summary>
        public void ForceReturnUmbrella()
        {
            if (!_player.UnlockUmbrella)
                return;

            EnterReturnState();
            ReturnStick = true;

            ChangeState(StateUmbrellaFold.Instance, true);
            var obj = EffectManager.Instance.PlayAndGetEffect(EffectEnum.ReturnFX);
            obj.transform.SetPositionAndRotation(_player.transform.position, Quaternion.Euler(0, 0, 0));

            transform.SetParent(_player.transform);
            transform.localPosition = _originLocalPos;

            ExitReturnState();
        }

        private void ProcessAttack(GameObject e, int damage, bool notCheck = false)
        {
            if (e.tag != "Trigger" && !_hitAttackList.Contains(e.GetInstanceID()))
            {
                ;
                if (e.TryGetComponent<PlayerAttackInteractive>(out var _hitInteractive))
                {
                    _hitInteractive.Call();
                    // notCheck == true 이면 우산이 공격상태인지 체크하지 않고 데미지를 가합니다.
                    if (notCheck)
                        _hitInteractive.Damage(damage, Direction);
                    else if (CurrentUmbrellaStateEnum == UmbrellaState.ATTACK)
                        _hitInteractive.Damage(damage, Direction);

                    if (CurrentUmbrellaStateEnum == UmbrellaState.THROW)
                        _hitInteractive.ThrowCall();
                    if (CurrentUmbrellaStateEnum == UmbrellaState.STICK)
                        _hitInteractive.ReturnCall();
                }

                _hitAttackList.Add(e.GetInstanceID());

                if (e.layer == LayerMask.NameToLayer(LayerEnum.Enemy.ToString()))
                {
                    SoundManager.Instance.PlaySFX(SoundManager.SFXNames.Hit);
                }
            }
        }

        private void InitializeUmbrellaThrowOrRush()
        {
            SetAndGetMouseDir();
            _rotZ = Mathf.Atan2(Direction.y, Direction.x) * Mathf.Rad2Deg - 90;
            _previousThrowPosition = transform.position;
            transform.rotation = Quaternion.Euler(0, 0, _rotZ);
            transform.SetParent(null);
            IsThrewUmbrella = true;
            _bouncesCount = 0;
            _hitAttackList.Clear();
            _spriteRenderer.sprite = umbrellaSprite[0];
            _spriteRenderer.gameObject.SetActive(true);
        }

        private void UmbrellaMove(Vector2 destination, RaycastHit2D hit, Vector2 sizeYMulDir)
        {
            if (hit)
            {
                transform.position = hit.point - sizeYMulDir;
                return;
            }
            else
            {
                transform.Translate(destination, Space.World);
            }

            _previousThrowPosition = transform.position;
        }

        public void ChangeUmbrellaActive(bool value)
        {
            _playerAnimation.SetUmbrellaAlpha(value);
            _mouseDotted.Active = value;
            if (value)
                StartState();
        }

        public void CancelAimCoroutine()
        {
            if (!ReferenceEquals(null, _cancelAimCoroutine))
                StopCoroutine(_cancelAimCoroutine);
        }

        private void TargetingEnemy(GameObject gameObject)
        {
            TargetAimPosition = gameObject.transform.position;
            LastOnTargetingTime = _playerData.TargetAimTime;
            ProcessAttack(gameObject, _player.BatRushDamage, true);
        }

        private IEnumerator<WaitForSeconds> CheckHoldBatRushRoutine(float duration)
        {
            yield return new WaitForSeconds(duration);
            if (_playerMovement.CurrentMovementStateEnum != PlayerMovementState.BATRUSH)
                yield break;
            if (IsHoldBatRushButton) //&& IsLedgeUmbrella)
                _playerMovement.ChangeState(StatePlayerMovementRush.Instance);
        }

        private IEnumerator<WaitForSeconds> WaitAndRunFunctionRoutine(Action func, float duration)
        {
            yield return new WaitForSeconds(duration);
            func();
        }
    }
}