using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UB.Animation;
using UnityEngine;
using UnityEngine.InputSystem;
using UB.EVENT;

namespace UB
{
	public partial class PlayerMovement : Singleton<PlayerMovement>
	{
		public PlayerData PlayerDataInstance;

		#region COMPONENTS
		private PlayerAnimation _playerAnimation;
		private Player _player;
		private BoxCollider2D _boxCollider2D;
		/// <summary>
		/// 절대로 캐싱을 위한 목적으로 가져오지 마십쇼.<code>Instead of. Player.Instance.GetComponent&lt; Rigidbody2D&gt;()</code>
		/// </summary>
		public Rigidbody2D PlayerRigidBody { get; private set; }
		private ParticleSystem _runFX;
		#endregion

		#region STATE PARAMETERS
		public bool IsFacingRight { get; private set; } = true;

		public bool IsLedgeDetected { get; set; }
		/// <summary>
		/// 장애물로 인한 중력 0인 상태 (예: 바람 활공)
		/// </summary>
		public bool IsObstacle { get; set; }
		public bool IsWind { get; set; }

		public float LastOnGroundTime { get; private set; }
		public float LastOnWallTime { get; private set; }
		public float LastOnWallRightTime { get; private set; }
		public float LastOnWallLeftTime { get; private set; }
		public float LastOnSlidingRightTime { get; private set; }
		public float LastOnSlidingLeftTime { get; private set; }
		public float LastOnJumpHangTime { get; private set; }
		public float LastOnFacingTime { get; private set; }
		public float LastOnJumpTime { get; private set; }
		public float DenyJumpTime { get; set; }
		public float LastOnMoveTime { get; private set; }

		//Jump
		private bool _isJumpCut;
		private int _doubleJumpCount;

		//Ledge
		public bool CanGrab { get; private set; } = true;
		private const float GRAB_TERM = 0.2f;
		private float _lastLedgeTime;
		private const float SLIDE_JUMP_TERM = 0.2f;
		public Vector2 LedgeRBPos { get; set; }
		public string LedgeTag { get; set; }
		public GameObject LedgeObject { get; set; }

		//BatRush
		private Vector2 _batRushPos;
		private float _grabTime;

		// Air Gravity : 수치가 겹치면 안됨!!!
		public const float THROW_HOLDING_AIR_GRAVITY = 0.01f;
		public const float BATRUSH_HOLDING_AIR_GRAVITY = 0.009f;
		private const float GLIDING_GRAVITY = 0.1f;
		private Coroutine _holdingRoutine;
		private UniTask _runEffectTask;

		#endregion

		#region INPUT PARAMETERS
		private Vector2 _moveInput;
		public bool IsHoldMoveButton { get; private set; }
		public bool IsHoldJumpButton { get; private set; }
		public float LastPressedJumpTime { get; private set; }
		public bool DenyRun { get; set; }
		#endregion

		#region CHECK PARAMETERS
		[Header("Checks")]
		[SerializeField] private BoxCollider2D _groundCheckPoint;
		[Space(5)]
		[SerializeField] private BoxCollider2D _frontWallCheckPoint;
		[SerializeField] private BoxCollider2D _backWallCheckPoint;
		#endregion

		#region LAYERS & TAGS
		[Header("Layers & Tags")]
		[Tooltip("착지할 수 있는 레이어")]
		[SerializeField] private LayerMask _groundLayer;
		[Tooltip("벽타기 할 수 있는 레이어")]
		[SerializeField] private LayerMask _wallWalkLayer;
		private GameObject _rightWallGameObject;
		private GameObject _leftWallGameObject;
		private MovementInteractive _lastAddedWallMovementInteractive;
		#endregion

		#region Umbrella
		private Umbrella _umbrella;
		private StickUmbrella _stickUmbrella;
		private RushGuide _rushGuide;
		public bool AbleGliding { get; private set; }
		#endregion

		private const int IDLE_CHANGE_TIME = 11;

		protected override void Awake()
		{
			base.Awake();
			_runFX = transform.Find("RunFX")?.GetComponent<ParticleSystem>();
		}

		private void Start()
		{
			PlayerRigidBody = GetComponent<Rigidbody2D>();
			_boxCollider2D = GetComponent<BoxCollider2D>();
			_playerAnimation = PlayerAnimation.Instance;
			_player = Player.Instance;
			_rushGuide = RushGuide.Instance;
			_umbrella = Umbrella.Instance;
			_stickUmbrella = StickUmbrella.Instance;

			SetGravityScale(PlayerDataInstance.GravityScale);
			CurrentMovementState = StatePlayerMovementIdle.Instance;
			_rushGuide.Active = false;
		}

		private void Update()
		{
			if (_player.CurPlayerState == Player.PlayerState.DEAD)
				return;

			CheckCollisionChecks();

			SubtractTimeFromTimers();

			int flagIsAbleGliding = (int)PlayerMovementState.GLIDING | (int)PlayerMovementState.FALL;
			AbleGliding = GetVelocity().y < 1 && _umbrella.LastOnUmbrellaSleepTime <= 0 && ((int)CurrentMovementStateEnum & flagIsAbleGliding) > 0;

			// 낙하 속력 조절
			if (CurrentMovementStateEnum == PlayerMovementState.FALL)
			{
				float _fallSpeed = Mathf.Max(PlayerRigidBody.velocity.y, -PlayerDataInstance.MaxFallSpeed);
				if (IsWind)
					_fallSpeed = Mathf.Max(PlayerRigidBody.velocity.y, -PlayerDataInstance.MaxFallSpeed * PlayerDataInstance.WindMaxFallMultiple);
				SetVelocity(PlayerRigidBody.velocity.x, _fallSpeed);
			}

			CurrentMovementState.action(this);

			#region GRAVITY
			// 외부의 요인 때문에 중력이 수정되어야 하는 경우
			SetExternalFactorGravity();
			#endregion
		}

		private void SubtractTimeFromTimers()
		{
			LastOnGroundTime = Mathf.Clamp(LastOnGroundTime - TimeManager.Instance.GetDeltaTime(), 0, float.MaxValue);
			LastOnWallTime = Mathf.Clamp(LastOnWallTime - TimeManager.Instance.GetDeltaTime(), 0, float.MaxValue);
			LastOnWallRightTime = Mathf.Clamp(LastOnWallRightTime - TimeManager.Instance.GetDeltaTime(), 0, float.MaxValue);
			LastOnWallLeftTime = Mathf.Clamp(LastOnWallLeftTime - TimeManager.Instance.GetDeltaTime(), 0, float.MaxValue);
			LastOnSlidingLeftTime = Mathf.Clamp(LastOnSlidingLeftTime - TimeManager.Instance.GetDeltaTime(), 0, float.MaxValue);
			LastOnSlidingRightTime = Mathf.Clamp(LastOnSlidingRightTime - TimeManager.Instance.GetDeltaTime(), 0, float.MaxValue);
			LastOnJumpHangTime = Mathf.Clamp(LastOnJumpHangTime - TimeManager.Instance.GetDeltaTime(), 0, float.MaxValue);
			LastOnJumpTime = Mathf.Clamp(LastOnJumpTime - TimeManager.Instance.GetDeltaTime(), 0, float.MaxValue);
			LastOnFacingTime = Mathf.Clamp(LastOnFacingTime - TimeManager.Instance.GetDeltaTime(), 0, float.MaxValue);
			_lastLedgeTime = Mathf.Clamp(_lastLedgeTime - TimeManager.Instance.GetDeltaTime(), 0, float.MaxValue);
			DenyJumpTime = Mathf.Clamp(DenyJumpTime - TimeManager.Instance.GetDeltaTime(), 0, float.MaxValue);
			LastPressedJumpTime = Mathf.Clamp(LastPressedJumpTime - TimeManager.Instance.GetDeltaTime(), 0, float.MaxValue);
			LastOnMoveTime = Mathf.Clamp(LastOnMoveTime - TimeManager.Instance.GetDeltaTime(), 0, float.MaxValue);
		}

		private void SetExternalFactorGravity()
		{
			if (!CanMoveState() || IsObstacle)
			{
				if (PlayerRigidBody.gravityScale != 0)
					SetGravityScale(0);
			}
			else if (PlayerRigidBody.gravityScale == 0)
			{
				SetGravityScale(PlayerDataInstance.GravityScale);
			}
		}

		private void CheckCollisionChecks()
		{
			// JUMP 상태에서도 Ground Check를 하면 JUMP인데 땅에 닿은 판정이 되어서 빠르게 FALL -> IDLE 로 변해서 JUMPCUT을 무시하는 케이스가 있음 (빠르게 점프키를 했을 때)
			if (CurrentMovementStateEnum != PlayerMovementState.JUMP && MathF.Abs(GetVelocity().y) <= 0.001f)
			{
				//Ground Check
				if (Physics2D.OverlapBox(_groundCheckPoint.bounds.center, _groundCheckPoint.size, 0, _groundLayer))
				{
					if (LastOnGroundTime == 0)
					{
						var obj = EffectManager.Instance.PlayAndGetEffect(EffectEnum.LandFX);
						obj.transform.SetPositionAndRotation(_groundCheckPoint.bounds.center, Quaternion.Euler(-90, 0, 0));
					}
					// coyote time
					LastOnGroundTime = PlayerDataInstance.CoyoteTime;
				}
			}

			//Right Wall Check
			Collider2D collider = null;
			if (IsFacingRight)
				collider = Physics2D.OverlapBox(_frontWallCheckPoint.bounds.center, _frontWallCheckPoint.size, 0, _groundLayer);
			else
				collider = Physics2D.OverlapBox(_backWallCheckPoint.bounds.center, _backWallCheckPoint.size, 0, _groundLayer);

			if (collider != null)
			{
				LastOnWallRightTime = PlayerDataInstance.CoyoteTime;
				if (collider.TryGetComponent<GroundOptions>(out var groundOptions))
				{
					if (groundOptions.AbleSliding)
						LastOnSlidingRightTime = PlayerDataInstance.CoyoteTime;
				}
				_rightWallGameObject = collider.gameObject;
			}
			else
			{
				_rightWallGameObject = null;
			}

			//Left Wall Check
			collider = null;
			if (IsFacingRight)
				collider = Physics2D.OverlapBox(_backWallCheckPoint.bounds.center, _backWallCheckPoint.size, 0, _groundLayer);
			else
				collider = Physics2D.OverlapBox(_frontWallCheckPoint.bounds.center, _frontWallCheckPoint.size, 0, _groundLayer);

			if (collider != null)
			{
				LastOnWallLeftTime = PlayerDataInstance.CoyoteTime;
				if (collider.TryGetComponent<GroundOptions>(out var groundOptions))
				{
					if (groundOptions.AbleSliding)
						LastOnSlidingLeftTime = PlayerDataInstance.CoyoteTime;
				}
				_leftWallGameObject = collider.gameObject;
			}
			else
			{
				_leftWallGameObject = null;
			}

			if (LastOnWallLeftTime > 0 || LastOnWallRightTime > 0)
				LastOnWallTime = Mathf.Max(LastOnWallLeftTime, LastOnWallRightTime);
		}

		private void FixedUpdate()
		{
			if (_player.CurPlayerState == Player.PlayerState.DEAD)
				return;

			if (!DenyRun)
			{
				// 시선 돌리기
				if (LastOnFacingTime <= 0)
				{
					if (_moveInput.x != 0)
					{
						CheckDirectionToFace(_moveInput.x > 0);
					}
				}
				//Handle Run
				if (PlayerRigidBody.gravityScale == THROW_HOLDING_AIR_GRAVITY)
				{
					Run(PlayerDataInstance.UmbrellaThrowAirLerp);
				}
				else if (PlayerRigidBody.gravityScale == BATRUSH_HOLDING_AIR_GRAVITY)
				{
					Run(PlayerDataInstance.UmbrellaBatRushAirLerp);
				}
				else if (CurrentMovementStateEnum == PlayerMovementState.WALLJUMP)
				{
					Run(PlayerDataInstance.WallJumpRunLerp);
				}
				else if (CanMoveState())
				{
					Run(1);
				}
			}

			if (CurrentMovementStateEnum == PlayerMovementState.SLIDING)
			{
				WallWalkAndSlide();
			}
		}

		#region INPUT CALLBACKS
		public void OnPressMoveButton(InputAction.CallbackContext context)
		{
			Vector2 input = context.ReadValue<Vector2>();
			if (input != null)
			{
				_moveInput = input;
			}
		}

		public void OnHoldMoveXButton(InputAction.CallbackContext context)
		{
			if (context.performed)
			{
				IsHoldMoveButton = true;
			}

			if (context.canceled)
			{
				IsHoldMoveButton = false;
			}
		}

		public void OnPressJumpButton(InputAction.CallbackContext context)
		{
			if (context.started)
			{
				if (TimeManager.Instance.TimeScale == 0)
					return;

				LastPressedJumpTime = PlayerDataInstance.JumpInputBufferTime;
				IsHoldJumpButton = true;

				if (CanDoubleJump() && _player.UnlockDoubleJump)
				{
					ChangeState(StatePlayerMovementDoubleJump.Instance);
					_doubleJumpCount++;
				}
			}

			if (context.canceled)
			{
				IsHoldJumpButton = false;
			}
		}
		#endregion

		private void SetGravityScale(float scale)
		{
			PlayerRigidBody.gravityScale = scale;
		}

		private void Run(float lerpAmount)
		{
			float targetSpeed = _moveInput.x * PlayerDataInstance.RunMaxSpeed;
			targetSpeed = Mathf.Lerp(PlayerRigidBody.velocity.x, targetSpeed, lerpAmount);

			float accelRate;
			if (LastOnGroundTime > 0)
				accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? PlayerDataInstance.RunAccelAmount : PlayerDataInstance.RunDecelAmount;
			else
				accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? PlayerDataInstance.RunAccelAmount * PlayerDataInstance.AccelInAir : PlayerDataInstance.RunDecelAmount * PlayerDataInstance.DecelInAir;

			if (IsAir() && Mathf.Abs(PlayerRigidBody.velocity.y) <= PlayerDataInstance.JumpHangTimeThreshold)
			{
				accelRate *= PlayerDataInstance.JumpHangAccelerationMultiple;
				targetSpeed *= PlayerDataInstance.JumpHangMaxSpeedMultiple;
			}

			if (PlayerDataInstance.DoConserveMomentum && Mathf.Abs(PlayerRigidBody.velocity.x) > Mathf.Abs(targetSpeed) && Mathf.Sign(PlayerRigidBody.velocity.x).Equals(Mathf.Sign(targetSpeed)) && Mathf.Abs(targetSpeed) > 0.01f && LastOnGroundTime <= 0)
			{
				accelRate = 0;
			}

			float speedDif = targetSpeed - PlayerRigidBody.velocity.x;
			if (PlayerDataInstance.RunMaxSpeed < Mathf.Abs(PlayerRigidBody.velocity.x))
			{
				accelRate *= 0.1f;
			}

			float movement = speedDif * accelRate;
			if (MathF.Abs(movement) < 0.01f)
				movement = 0;

			PlayerRigidBody.AddForce(movement * Vector2.right, ForceMode2D.Force);
		}

		private void Turn(bool direction)
		{
			Vector3 scale = transform.localScale;
			scale.x = direction ? 1 : -1;
			transform.localScale = scale;

			IsFacingRight = direction;
		}

		/// <summary>
		/// 원하는 방향으로 특정한 시간동안 시선을 고정시킵니다.
		/// </summary>
		/// <param name="isRight"> true == 오른쪽 </param>
		/// <param name="time"> float 지속시간 </param>
		public void TurnFixed(bool isRight, float time)
		{
			CheckDirectionToFace(isRight);
			LastOnFacingTime = time;
		}

		private void Jump()
		{
			LastPressedJumpTime = 0;
			LastOnGroundTime = 0;
			LastOnJumpTime = PlayerDataInstance.CoyoteTime;

			float force = PlayerDataInstance.JumpForce;
			if (PlayerRigidBody.velocity.y < 0)
				force -= PlayerRigidBody.velocity.y;

			PlayerRigidBody.AddForce(Vector2.up * force, ForceMode2D.Impulse);

			_playerAnimation.PlayForceAnimation(PlayerAnimationEnum.JUMP);
			_playerAnimation.OnJumpTrigger();
		}

		private void WallJump(int dir)
		{
			LastPressedJumpTime = 0;
			LastOnGroundTime = 0;
			LastOnWallRightTime = 0;
			LastOnWallLeftTime = 0;
			LastOnWallTime = 0;

			Vector2 force = new Vector2(PlayerDataInstance.WallJumpForce.x, PlayerDataInstance.WallJumpForce.y);
			force.x *= dir;

			// if (Mathf.Sign(PlayerRigidBody.velocity.x) != Mathf.Sign(force.x))
			// 	force.x -= PlayerRigidBody.velocity.x > 0 ? Mathf.Min(PlayerRigidBody.velocity.x, PlayerDataInstance.WallJumpForce.x * 0.1f)
			// 	 : Mathf.Max(PlayerRigidBody.velocity.x, -PlayerDataInstance.WallJumpForce.x * 0.1f);
			// if (PlayerRigidBody.velocity.y < 0) //checks whether player is falling, if so we subtract the velocity.y (counteracting force of gravity). This ensures the player always reaches our desired jump force or greater
			// 	force.y -= MathF.Max(PlayerRigidBody.velocity.y, -PlayerDataInstance.WallJumpForce.y * 0.1f);

			if (PlayerRigidBody.velocity.y < 0)
				PlayerRigidBody.AddForce(-PlayerRigidBody.velocity.y * Vector2.up, ForceMode2D.Force);

			PlayerRigidBody.AddForce(force, ForceMode2D.Impulse);

			_playerAnimation.OnJumpTrigger();
		}

		private void DoubleJump()
		{
			LastOnJumpHangTime = PlayerDataInstance.JumpHangTime;

			LastPressedJumpTime = 0;
			LastOnGroundTime = 0;

			float force = PlayerDataInstance.DoubleJumpForce;
			if (PlayerRigidBody.velocity.y < 0)
				force -= PlayerRigidBody.velocity.y;

			PlayerRigidBody.AddForce(Vector2.up * force, ForceMode2D.Impulse);

			_playerAnimation.OnJumpTrigger();
		}

		private async UniTaskVoid SetVelocityInFixedUpdate(Vector2 velocity)
		{
			await UniTask.WaitForFixedUpdate(cancellationToken: this.GetCancellationTokenOnDestroy());
			PlayerRigidBody.velocity = velocity;
		}

		private async UniTaskVoid RunActionInFixedUpdate(Action func = null)
		{
			await UniTask.WaitForFixedUpdate(cancellationToken: this.GetCancellationTokenOnDestroy());
			func?.Invoke();
		}

		public void StartLedge()
		{
			if (CurrentMovementStateEnum == PlayerMovementState.FALL)
			{
				if (CanLedge())
				{
					ChangeState(StatePlayerMovementLedge.Instance, true);
					return;
				}
			}
			else
			{
				if (CanLedge() && _moveInput.y == 0)
				{
					ChangeState(StatePlayerMovementLedge.Instance, true);
					return;
				}
			}
		}

		#region CHECK METHODS
		public void CheckDirectionToFace(bool isMovingRight, bool force = false)
		{
			if (!CanMoveState() && !force)
				return;
			if (IsFacingRight != isMovingRight)
				Turn(isMovingRight);
		}

		private bool CanJump()
		{
			return LastOnGroundTime > 0 && DenyJumpTime == 0;
		}

		private bool CanDoubleJump()
		{
			return IsAir() && DenyJumpTime == 0 && _doubleJumpCount < PlayerDataInstance.DoubleJumpCount;
		}

		public bool CanLedge()
		{
			return CanGrab && PlayerRigidBody.velocity.y < 0 && IsLedgeDetected;
		}

		public bool CanLedgeState()
		{
			return CurrentMovementStateEnum == PlayerMovementState.FALL || CurrentMovementStateEnum == PlayerMovementState.SLIDING || CurrentMovementStateEnum == PlayerMovementState.GLIDING; 
		}

		public bool CanSliding()
		{
			// 방향 고정이 되어 있으면 false
			if (LastOnFacingTime != 0)
				return false;

			// 만약 왼쪽 벽이나 오른쪽 벽에 비비고 있으면
			bool isLeftWallInput = LastOnSlidingLeftTime > 0 && _moveInput.x < 0;
			bool isRightWallInput = LastOnSlidingRightTime > 0 && _moveInput.x > 0;
			if (isLeftWallInput)
			{
				if (ReferenceEquals(_leftWallGameObject, null))
					return false;
				// GroundOptions가 있으면 그 설정을 따름
				if (_leftWallGameObject.TryGetComponent<GroundOptions>(out var groundOptions))
					return groundOptions.AbleSliding;
				else
				{
					// 없으면 슬라이딩이 가능한 레이어인지 판단
					return (1 << _leftWallGameObject.layer & _wallWalkLayer) > 0;
				}
			}
			else if (isRightWallInput)
			{
				if (ReferenceEquals(_rightWallGameObject, null))
					return false;
				// GroundOptions가 있으면 그 설정을 따름
				if (_rightWallGameObject.TryGetComponent<GroundOptions>(out var groundOptions))
					return groundOptions.AbleSliding;
				else
				{
					// 없으면 슬라이딩이 가능한 레이어인지 판단
					return (1 << _rightWallGameObject.layer & _wallWalkLayer) > 0;
				}
			}
			// 입력이 없으면 false
			else
				return false;
		}
		#endregion

		/// <summary>
		/// 입력 값 벡터를 가져옵니다.
		/// </summary>
		public Vector2 GetMoveInput()
		{
			return _moveInput;
		}

		/// <summary>
		/// 플레이어블 캐릭터에게 힘을 가합니다. (매달리기가 풀립니다.)
		/// </summary>
		/// <param name="isAffectiveAir">공중에서의 가속력과 제동력 적용 여부입니다.</param>
		/// <returns></returns>
		public void PlayerAddForce(Vector2 dir, bool isAffectiveAir = true, ForceMode2D forceMode = ForceMode2D.Impulse)
		{
			if (CurrentMovementStateEnum == PlayerMovementState.LEDGE)
			{
				ChangeState(StatePlayerMovementFall.Instance, true);
			}
			if (!isAffectiveAir)
			{
				PlayerRigidBody.AddForce(dir, forceMode);
				return;
			}
			// 충격 완화
			float accelRate = (Mathf.Abs(_moveInput.x) > 0.01f) ? PlayerDataInstance.AccelInAir : PlayerDataInstance.DecelInAir;
			if (IsAir())
			{
				dir *= accelRate;
				PlayerRigidBody.AddForce(dir, forceMode);
			}
			else
			{
				PlayerRigidBody.AddForce(dir, forceMode);
			}
		}

		/// <summary>
		/// 공중 공격(패링) 시 플레이어 무브먼트 관련 값 상태 초기화 함수 (중력 초기화 x)
		/// </summary>
		public void SettingAirAttackMovementStateInit()
		{
			LastOnGroundTime = 0;
			LastOnJumpHangTime = 0;
			_isJumpCut = false;
		}

		/// <summary>
		/// 장애물 상호 작용 시 플레이어 무브먼트 관련 값 상태 초기화 함수 (중력 초기화 O)
		/// </summary>
		public void SettingObstacleMovementStateInit()
		{
			LastOnGroundTime = 0;
			LastOnJumpHangTime = 0;
			InitializeCounts();
			SetGravityScale(PlayerDataInstance.GravityScale);
			_isJumpCut = false;
		}

		public Vector2 GetVelocity()
		{
			return PlayerRigidBody.velocity;
		}

		public void SetVelocity(float x, float y)
		{
			PlayerRigidBody.velocity = Vector2.right * x + Vector2.up * y;
		}

		public void SetVelocity(Vector2 v)
		{
			PlayerRigidBody.velocity = v;
		}

		public void SetPlayerPosition(Vector3 position)
		{
			transform.position = position;
			PlayerRigidBody.position = position;
			LedgeDetection.Instance.SetForceLedgePos(position);
		}

		/// <summary>
		/// 캐릭터가 공중 상태이면 true를 반환합니다.
		/// </summary>
		/// <returns></returns>
		public bool IsAir()
		{
			int flagIsAir = (int)PlayerMovementState.JUMP | (int)PlayerMovementState.GLIDING | (int)PlayerMovementState.FALL;
			return ((int)CurrentMovementStateEnum & flagIsAir) > 0;
		}

		/// <summary>
		/// 캐릭터가 움직일 수 있는 상태이면 true를 반환합니다. (Run() 메소드를 호출할 수 있는가)
		/// </summary>
		/// <returns></returns>
		public bool CanMoveState()
		{
			int flagIsNotMove = (int)PlayerMovementState.LEDGE | (int)PlayerMovementState.BATRUSH |
								(int)PlayerMovementState.RUSH | (int)PlayerMovementState.UMBRELLARUSH |
								(int)PlayerMovementState.SLIDING;
			return !(((int)CurrentMovementStateEnum & flagIsNotMove) > 0);
		}

		public bool IsClosedGround(float distance)
		{
			return Physics2D.Raycast(_groundCheckPoint.bounds.center, Vector2.down, distance, _groundLayer);
		}

		public void StartBatRushOnWall(Vector2 pos)
		{
			_runFX.Stop();
			_batRushPos = pos;

			Collider2D ledgeCollider = Physics2D.OverlapBox(_batRushPos, _boxCollider2D.size, 0f, _groundLayer);
			if (!ReferenceEquals(ledgeCollider, null))
			{
				Vector2 checkPos = _stickUmbrella.BatRushTransform.position;
				// 우산이 위에 박힘
				if (_umbrella.HitWallVector.y < -0.01f)
				{
					_batRushPos = AbleBatRushOnNewPosition(checkPos, Vector2.up);
				}
				// 우산이 아래에 박힘
				else if (_umbrella.HitWallVector.y > 0.01f)
				{
					_batRushPos = AbleBatRushOnNewPosition(checkPos, Vector2.down);
				}
				// 우산이 오른쪽에 박힘
				else if (_umbrella.HitWallVector.x < -0.01f)
				{
					_batRushPos = AbleBatRushOnNewPosition(checkPos, Vector2.right);
				}
				// 우산이 왼쪽에 박힘
				else if (_umbrella.HitWallVector.x > 0.01f)
				{
					_batRushPos = AbleBatRushOnNewPosition(checkPos, Vector2.left);
				}
				_umbrella.ReturnStick = true;
			}

			ChangeState(StatePlayerMovementBatRush.Instance, true);
		}

		public void StartBatRushOnObject(Vector2 pos)
		{
			_batRushPos = pos;

			Collider2D ledgeCollider = Physics2D.OverlapBox(_batRushPos, _boxCollider2D.size, 0f, _groundLayer);
			if (ledgeCollider != null)
			{
				Vector2 inverseDir = -_umbrella.Direction;
				int i;
				int testSize = 4;
				for (i = 1; i < testSize; i++)
				{
					pos += inverseDir * i * 0.5f;
					ledgeCollider = Physics2D.OverlapBox(pos, _boxCollider2D.size, 0f, _groundLayer);
					if (ledgeCollider == null)
					{
						_batRushPos = pos;
						break;
					}
				}
				if (i == testSize)
					_batRushPos = _umbrella.TargetAimPosition;
			}

			ChangeState(StatePlayerMovementBatRush.Instance, true);
		}

		/// <summary>
		/// 4방향 중 direction 방향이 아닌 3방향에 대해 플레이어가 충돌하지 않는 위치가 있는지 찾음
		/// </summary>
		/// <param name="direction"></param>
		/// <returns></returns>
		private Vector2 AbleBatRushOnNewPosition(Vector2 checkPos, Vector2 direction)
		{
			if (direction == Vector2.zero)
				return Vector2.zero;

			float offset = 1f;
			if (direction == Vector2.up)
			{
				Vector2 newPos = checkPos + (Vector2.down * offset);
				var down = Physics2D.OverlapBox(newPos, _boxCollider2D.size, 0, _groundLayer);
				if (down == null)
					return newPos;

				newPos = checkPos + (Vector2.left * offset);
				var left = Physics2D.OverlapBox(newPos, _boxCollider2D.size, 0, _groundLayer);
				if (left == null)
					return newPos;

				newPos = checkPos + (Vector2.right * offset);
				var right = Physics2D.OverlapBox(newPos, _boxCollider2D.size, 0, _groundLayer);
				if (right == null)
					return newPos;

				return Vector2.zero;
			}
			else if (direction == Vector2.down)
			{
				Vector2 newPos = checkPos + (Vector2.up * offset);
				var up = Physics2D.OverlapBox(newPos, _boxCollider2D.size, 0, _groundLayer);
				if (up == null)
					return newPos;

				newPos = checkPos + (Vector2.left * offset);
				var left = Physics2D.OverlapBox(newPos, _boxCollider2D.size, 0, _groundLayer);
				if (left == null)
					return newPos;

				newPos = checkPos + (Vector2.right * offset);
				var right = Physics2D.OverlapBox(newPos, _boxCollider2D.size, 0, _groundLayer);
				if (right == null)
					return newPos;

				return Vector2.zero;
			}
			else if (direction == Vector2.left)
			{
				Vector2 newPos = checkPos + (Vector2.right * offset);
				var right = Physics2D.OverlapBox(newPos, _boxCollider2D.size, 0, _groundLayer);
				if (right == null)
					return newPos;

				newPos = checkPos + (Vector2.up * offset);
				var up = Physics2D.OverlapBox(newPos, _boxCollider2D.size, 0, _groundLayer);
				if (up == null)
					return newPos;

				newPos = checkPos + (Vector2.down * offset);
				var down = Physics2D.OverlapBox(newPos, _boxCollider2D.size, 0, _groundLayer);
				if (down == null)
					return newPos;

				return Vector2.zero;
			}
			else if (direction == Vector2.right)
			{
				Vector2 newPos = checkPos + (Vector2.left * offset);
				var left = Physics2D.OverlapBox(newPos, _boxCollider2D.size, 0, _groundLayer);
				if (left == null)
					return newPos;

				newPos = checkPos + (Vector2.up * offset);
				var up = Physics2D.OverlapBox(newPos, _boxCollider2D.size, 0, _groundLayer);
				if (up == null)
					return newPos;

				newPos = checkPos + (Vector2.down * offset);
				var down = Physics2D.OverlapBox(newPos, _boxCollider2D.size, 0, _groundLayer);
				if (down == null)
					return newPos;
			}

			return Vector2.zero;
		}

		public bool CheckBatRushSliding(Vector2 pos)
		{
			var result = Physics2D.OverlapBox(pos, _boxCollider2D.size, 0, _groundLayer);
			return !result;
		}

		private IEnumerator<WaitForSeconds> WaitAndRunFunctionCoroutine(Action func, float duration)
		{
			yield return new WaitForSeconds(duration);
			func();
		}

		/// <summary>
		/// duration 시간 동안 공중에 떠있는 상태를 유지합니다.
		/// </summary>
		/// <param name="duration"></param>
		/// <param name="initVelocity">속도를 초기화 합니다.</param>
		public void AirHolding(float duration, bool isThrow, bool initVelocity = true)
		{
			StopAirHolding();

			if (isThrow)
				SetGravityScale(THROW_HOLDING_AIR_GRAVITY);
			else
				SetGravityScale(BATRUSH_HOLDING_AIR_GRAVITY);

			SettingAirAttackMovementStateInit(); // 감속 만들어야됨
			if (initVelocity)
				SetVelocity(Vector2.zero);

			// 일정 시간 이후 중력 복구
			_holdingRoutine = StartCoroutine(WaitAndRunFunctionCoroutine(() => { StopAirHolding(); }, duration));
		}

		private void StopAirHolding()
		{
			if (_holdingRoutine != null)
			{
				StopCoroutine(_holdingRoutine);
				_holdingRoutine = null;
				SetGravityScale(PlayerDataInstance.GravityScale);
			}
		}

		public bool IsAirHolding()
		{
			return _holdingRoutine != null;
		}

		private void WallWalkAndSlide(float lerpAmount = 1)
		{
			SetVelocity(0, PlayerRigidBody.velocity.y);

			if (((_moveInput.x < 0 && LastOnWallLeftTime > 0) || (_moveInput.x > 0 && LastOnWallRightTime > 0)) && PlayerDataInstance.IsMoveXSliding)
				_moveInput.y = 1;

			float targetSpeed = _moveInput.y * PlayerDataInstance.WallWalkMaxSpeed;
			if (_moveInput.y != 0)
				targetSpeed *= _moveInput.y > 0 ? PlayerDataInstance.WallUpAccel : PlayerDataInstance.WallDownAccel;

			targetSpeed = Mathf.Lerp(PlayerRigidBody.velocity.y, targetSpeed, lerpAmount);

			float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ?
			PlayerDataInstance.WallWalkAccelAmount : PlayerDataInstance.WallWalkDeccelAmount;

			float gravity = _moveInput.y != 0 ? 0 : PlayerDataInstance.GravityScale * PlayerDataInstance.SlideAccel;

			float speedDif = targetSpeed - PlayerRigidBody.velocity.y;
			if (_moveInput.y == 0)
				speedDif -= gravity;

			float movement = speedDif * accelRate;

			PlayerRigidBody.AddForce(movement * Vector2.up);
		}

		/// <summary>
		/// pos 위치에 잡기를 실행합니다.
		/// </summary>
		/// <param name="pos"></param>
		public void LedgePosition(Vector2 pos, string tag)
		{
			LedgeTag = tag;
			LedgeRBPos = pos;
			ChangeState(StatePlayerMovementLedge.Instance);
		}

		private void JumpEffect()
		{
			var obj = EffectManager.Instance.PlayAndGetEffect(EffectEnum.JumpFX);
			obj.transform.SetPositionAndRotation(_groundCheckPoint.bounds.center, Quaternion.Euler(-90, 0, 0));
		}

		private void PlayRunFX(Vector2 pos)
		{
			_runFX.transform.position = pos;
			var emission = _runFX.emission;
			emission.rateOverDistance = 5;
		}

		private void StopRunFX()
		{
			var emission = _runFX.emission;
			emission.rateOverDistance = 0;
		}

		private void InitializeCounts()
		{
			_umbrella.CountThrowAirHolding = 0;
			_umbrella.CountAttackHolding = 0;
			_doubleJumpCount = 0;
		}

		private void HangAnimation()
        {
            if (LedgeTag == "Obstacle" || (_umbrella.HitWallVector.y < -0.01f && LedgeTag == "Umbrella"))
            {
                _playerAnimation.PlayForceAnimation(PlayerAnimationEnum.HANGCEILING);
            }
            else if (LastOnWallTime > 0)
            {
                _playerAnimation.PlayForceAnimation(PlayerAnimationEnum.HANGWALL);
            }
            else
            {
                _playerAnimation.PlayForceAnimation(PlayerAnimationEnum.HANG);
            }
        }

		private void AddMovementInteractive()
		{
			if (IsFacingRight)
			{
				if (ReferenceEquals(_rightWallGameObject, null))
					return;

				if (_rightWallGameObject.TryGetComponent<MovementInteractive>(out var movementInteractive))
				{
					if(_lastAddedWallMovementInteractive == movementInteractive)
						return;
					movementInteractive.Add(gameObject);
					movementInteractive.Execute();
					_lastAddedWallMovementInteractive = movementInteractive;
				}
			}
			else
			{
				if (ReferenceEquals(_leftWallGameObject, null))
					return;

				if (_leftWallGameObject.TryGetComponent<MovementInteractive>(out var movementInteractive))
				{
					movementInteractive.Add(gameObject);
					movementInteractive.Execute();
					_lastAddedWallMovementInteractive = movementInteractive;
				}
			}
		}

		private void RemoveMovementInteractive()
		{
			if (_lastAddedWallMovementInteractive != null)
			{
				_lastAddedWallMovementInteractive.Remove(gameObject);
				_lastAddedWallMovementInteractive = null;
			}
		}

		#region EDITOR METHODS
#if UNITY_EDITOR
		private void OnDrawGizmos()
		{
			Gizmos.color = Color.green;
			Gizmos.DrawWireCube(_groundCheckPoint.bounds.center, _groundCheckPoint.size);
			Gizmos.color = Color.blue;
			Gizmos.DrawWireCube(_frontWallCheckPoint.bounds.center, _frontWallCheckPoint.size);
			Gizmos.DrawWireCube(_backWallCheckPoint.bounds.center, _backWallCheckPoint.size);
		}
#endif
		#endregion
	}
}