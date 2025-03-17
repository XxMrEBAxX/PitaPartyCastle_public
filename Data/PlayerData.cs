using MyBox;
using UnityEngine;

namespace UB
{
	[CreateAssetMenu(menuName = "Data/Player Data")] //Create a new playerData object by right clicking in the Project Menu then Create/Player/Player Data and drag onto the player
	public class PlayerData : ScriptableObject
	{
		[Header("플레이어 스탯")]

		[OverrideLabel("최대 체력")]
		[SerializeField] private int _maxHP = 100;
		public int MaxHP => _maxHP;

		[OverrideLabel("데미지")]
		[SerializeField] private int _damage = 4;
		public int Damage => _damage;

		[OverrideLabel("박쥐돌진 데미지")]
		[SerializeField] private int _batRushDamage = 1;
		public int BatRushDamage => _batRushDamage;

		[OverrideLabel("피격 무적 시간")]
		[SerializeField] private float _damagedInvincible = 0.5f;
		public float DamagedInvincible => _damagedInvincible;

		[Tooltip("(x : 시간, y : 타임 스케일)")]
		[OverrideLabel("피격 시 느려질 시간 커브")]
		[SerializeField] private AnimationCurve _damagedTimeScale;
		public AnimationCurve DamagedTimeScale => _damagedTimeScale;

		[OverrideLabel("공격 시 공중 체공할 시간")]
		[SerializeField] private float _attackAirTime = 0.3f;
		public float AttackAirTime => _attackAirTime;

		[OverrideLabel("공격 시 공중 체공 가능 카운트")]
		[SerializeField] private int _attackAirCount = 1;
		public int AttackAirCount => _attackAirCount;

		[OverrideLabel("에임 보정 유지 시간")]
		[SerializeField] private float _targetAimTime = 1;
		public float TargetAimTime => _targetAimTime;

		[Space(20)]

		private float _gravityStrength;
		public float GravityStrength => _gravityStrength;

		private float _gravityScale; //Strength of the player's gravity as a multiplier of gravity (set in ProjectSettings/Physics2D).
		public float GravityScale => _gravityScale;

		[Header("Gravity")]

		[OverrideLabel("중력 가속도")]
		[SerializeField] private float _fallGravityMultiple = 1;
		public float FallGravityMultiple => _fallGravityMultiple;

		[OverrideLabel("최대 낙하 속도")]
		[SerializeField] private float _maxFallSpeed = 20;
		public float MaxFallSpeed => _maxFallSpeed;

		[Space(20)]

		[Header("Run")]

		[OverrideLabel("걷기 최대 속도")]
		[SerializeField] private float _runMaxSpeed = 15;
		public float RunMaxSpeed => _runMaxSpeed;

		[OverrideLabel("걷기 가속력")]
		[SerializeField] private float _runAcceleration = 5;
		public float RunAcceleration => _runAcceleration;

		private float _runAccelAmount;
		public float RunAccelAmount => _runAccelAmount;

		[OverrideLabel("걷기 제동력")]
		[SerializeField] private float _runDeceleration = 10;
		public float RunDeceleration => _runDeceleration;

		private float _runDecelAmount; //Actual force (multiplied with speedDiff) applied to the player .
		public float RunDecelAmount => _runDecelAmount;

		[Space(5)]

		[OverrideLabel("공중에 있을 때의 가속력 배율")]
		[Range(0f, 1)][SerializeField] private float _accelInAir = 0.8f;
		public float AccelInAir => _accelInAir;

		[OverrideLabel("공중에 있을 때의 제동력 배율")]
		[Range(0f, 1)][SerializeField] private float _decelInAir = 0.8f;
		public float DecelInAir => _decelInAir;

		[Space(5)]

		//We won't slow the player down if they are moving in their desired direction but at a greater speed than their maxSpeed
		[Tooltip("최고 속력을 넘어도 플레이어가 입력중인 방향의 움직임이면 제동을 걸지 않음")]
		[OverrideLabel("이동 시 최고 속력 제동 여부")]
		[SerializeField] private bool _doConserveMomentum = true;
		public bool DoConserveMomentum => _doConserveMomentum;

		[Space(20)]

		[Header("Jump")]

		[OverrideLabel("점프 높이")]
		[SerializeField] private float _jumpHeight = 7;
		public float JumpHeight => _jumpHeight;

		[Tooltip("점프를 완료하는데 걸리는 시간 & 중력 옵션에 관련 있으므로 주의해서 조정할 것")]
		[SerializeField] private float _jumpTimeToApex = 0.3f;
		public float JumpTimeToApex => _jumpTimeToApex;

		private float _jumpForce;
		public float JumpForce => _jumpForce;

		[OverrideLabel("추가 점프 힘")]
		[SerializeField] private float _doubleJumpForce = 15;
		public float DoubleJumpForce => _doubleJumpForce;

		[OverrideLabel("추가 점프 카운트")]
		[SerializeField] private int _doubleJumpCount = 1;
		public int DoubleJumpCount => _doubleJumpCount;

		[Space(20)]

		[Header("Wall Jump")]

		[OverrideLabel("벽 점프 강도")]
		[SerializeField] private Vector2 _wallJumpForce = new Vector2(13, 22);
		public Vector2 WallJumpForce => _wallJumpForce;

		[OverrideLabel("벽 점프 중 Run 배율")]
		[Range(0f, 1f), SerializeField] private float _wallJumpRunLerp = 0.1f;
		public float WallJumpRunLerp => _wallJumpRunLerp;

		[Space(20)]

		[Header("Both Jumps")]

		[OverrideLabel("점프컷 중력 가속도")]
		[SerializeField] private float _jumpCutGravityMultiple = 2;
		public float JumpCutGravityMultiple => _jumpCutGravityMultiple;

		[OverrideLabel("점프행 중력 가속도")]
		[Range(0f, 1)] public float _jumpHangGravityMultiple = 0.5f;
		public float JumpHangGravityMultiple => _jumpHangGravityMultiple;

		[Tooltip("점프행 시 점프행 중력 가속도를 허용하는 속력 (작으면 작을 수록 점프 가속도가 0에 가까워야 함)")]
		[SerializeField] private float _jumpHangTimeThreshold = 0.1f;
		public float JumpHangTimeThreshold => _jumpHangTimeThreshold;

		[OverrideLabel("점프행 유지 시간")]
		[SerializeField] private float _jumpHangTime = 0.1f;
		public float JumpHangTime => _jumpHangTime;

		[OverrideLabel("점프행 움직임 가속")]
		[SerializeField] private float _jumpHangAccelerationMultiple = 1;
		public float JumpHangAccelerationMultiple => _jumpHangAccelerationMultiple;

		[OverrideLabel("점프행 움직임 속력 가속")]
		[SerializeField] private float _jumpHangMaxSpeedMultiple = 1;
		public float JumpHangMaxSpeedMultiple => _jumpHangMaxSpeedMultiple;

		[Space(20)]

		[Header("벽타기 & 슬라이드")]

		[Tooltip("슬라이드의 배율입니다. 입력이 없을 경우 아래 방향으로 중력이 적용됩니다. (중력 * 슬라이드 배율)")]
		[OverrideLabel("슬라이드 낙하 속력 배율")]
		[Range(1f, 10f), SerializeField] private float _slideAccel = 2;
		public float SlideAccel => _slideAccel;

		[OverrideLabel("벽타기 최대 속력")]
		[SerializeField] private float _wallWalkMaxSpeed = 13;
		public float WallWalkMaxSpeed => _wallWalkMaxSpeed;

		[OverrideLabel("벽타기의 가속력")]
		[SerializeField] private float _wallWalkAcceleration = 13;
		public float WallWalkAcceleration => _wallWalkAcceleration;

		private float _wallWalkAccelAmount;
		public float WallWalkAccelAmount => _wallWalkAccelAmount;

		[OverrideLabel("벽타기의 제동력")]
		[SerializeField] private float _wallWalkDecceleration = 13;
		public float WallWalkDecceleration => _wallWalkDecceleration;

		private float _wallWalkDeccelAmount;
		public float WallWalkDeccelAmount => _wallWalkDeccelAmount;

		[Tooltip("벽타기 중 올라갈 시 추가 속력 배율입니다.")]
		[OverrideLabel("벽 올라가기 추가 속력 배율")]
		[Range(0f, 2f), SerializeField] private float _wallUpAccel = 1;
		public float WallUpAccel => _wallUpAccel;

		[Tooltip("벽타기 중 내려갈 시 추가 속력 배율입니다.")]
		[OverrideLabel("벽 내려가기 추가 속력 배율")]
		[Range(0f, 2f), SerializeField] private float _wallDownAccel = 1;
		public float WallDownAccel => _wallDownAccel;

		[Space(20)]

		[Header("Umbrella")]
		[OverrideLabel("우산의 활공 속력")]
		[SerializeField] private float _umbrellaFallVelocity = 5;
		public float UmbrellaFallVelocity => _umbrellaFallVelocity;

		[OverrideLabel("우산을 회전하는 속도.")]
		[SerializeField] private float _umbrellaRotationSpeed = 1000;
		public float UmbrellaRotationSpeed => _umbrellaRotationSpeed;

		[OverrideLabel("우산의 패링 인정 시간입니다.")]
		[SerializeField] private float _umbrellaParryingTime = 0.2f;
		public float UmbrellaParryingTime => _umbrellaParryingTime;

		[Space(10)]

		[OverrideLabel("우산의 공격 지속 시간")]
		[SerializeField] private float _umbrellaAttackTime = 0.2f;
		public float UmbrellaAttackTime => _umbrellaAttackTime;

		[OverrideLabel("우산의 공격 쿨타임")]
		[SerializeField] private float _umbrellaAttackCooltime = 0.1f;
		public float UmbrellaAttackCooltime => _umbrellaAttackCooltime;

		[Space(20)]

		[Header("우산 던지기")]

		[OverrideLabel("우산 던지기 속력")]
		[SerializeField] private float _umbrellaThrowSpeed = 70;
		public float UmbrellaThrowSpeed => _umbrellaThrowSpeed;

		[OverrideLabel("우산 회수 속력")]
		[SerializeField] private float _umbrellaReturnSpeed = 100;
		public float UmbrellaReturnSpeed => _umbrellaReturnSpeed;

		[OverrideLabel("우산 최대 사거리")]
		[SerializeField] private float _umbrellaThrowMaxDistance = 20;
		public float UmbrellaThrowMaxDistance => _umbrellaThrowMaxDistance;

		[OverrideLabel("우산 던지기 보정 각도")]
		[SerializeField] private float _umbrellaAngle = 40;
		public float UmbrellaAngle => _umbrellaAngle;

		[OverrideLabel("우산 던지기 시 공중 체공할 시간")]
		[SerializeField] private float _umbrellaThrowAirTime = 0.3f;
		public float UmbrellaThrowAirTime => _umbrellaThrowAirTime;

		[OverrideLabel("우산 던지기 시 공중 체공 가능 카운트")]
		[SerializeField] private int _umbrellaThrowAirCount = 1;
		public int UmbrellaThrowAirCount => _umbrellaThrowAirCount;

		[OverrideLabel("우산 던지기 체공 시 Run Lerp 값")]
		[Range(0f, 1)][SerializeField] private float _umbrellaThrowAirLerp = 0.2f;
		public float UmbrellaThrowAirLerp => _umbrellaThrowAirLerp;

		[OverrideLabel("우산 던지기 후 회수가 가능한 시간")]
		[SerializeField] private float _umbrellaReturnAbleTime = 0;
		public float UmbrellaReturnAbleTime => _umbrellaReturnAbleTime;

		[OverrideLabel("우산 튕기기 최대 횟수")]
		[SerializeField] private float _umbrellaMaxBounceCount = 3;
		public float UmbrellaMaxBounceCount => _umbrellaMaxBounceCount;

		[OverrideLabel("우산 던지기로 박쥐 돌진 시 던지기 쿨타임")]
		[SerializeField] private float _umbrellaThrowCoolTime = 0.2f;
		public float UmbrellaThrowCoolTime => _umbrellaThrowCoolTime;

		[Space(20)]

		// [Header("우산 던지기 돌진")]

		// [Tooltip("우산 던지기 돌진 진입 시 타임 스케일 조정 커브 (x : 시간, y : 타임 스케일)")]
		// [OverrideLabel("우산 던지기 돌진 진입 시간 및 타임")]
		// [SerializeField] private AnimationCurve _umbrellaRushInputTimeScale;
		// public AnimationCurve UmbrellaRushInputTimeScale => _umbrellaRushInputTimeScale;

		// [OverrideLabel("우산 던지기 돌진 속도")]
		// [SerializeField] private float _umbrellaRushSpeed = 30;
		// public float UmbrellaRushSpeed => _umbrellaRushSpeed;

		// [Tooltip("우산 던지기 돌진 진입 시 속도 커브 (x : 시간, y : 배율)")]
		// [SerializeField] private AnimationCurve _umbrellaRushSpeedTimeScale;
		// public AnimationCurve UmbrellaRushSpeedTimeScale => _umbrellaRushSpeedTimeScale;

		// [Tooltip("우산 던지기 돌진 활공으로 취소 시 밀려날 힘")]
		// [SerializeField] private float _umbrellaRushCancelForce = 50;
		// public float UmbrellaRushCancelForce => _umbrellaRushCancelForce;

		// [OverrideLabel("우산 던지기 돌진 후 체공 시간")]
		// [SerializeField] private float _umbrellaRushAirTime = 0.4f;
		// public float UmbrellaRushAirTime => _umbrellaRushAirTime;

		[Space(20)]

		[Header("Bat Rush")]
		[OverrideLabel("박쥐 돌진 점프 강도")]
		[SerializeField] private float _batRushJumpForce = 40;
		public float BatRushJumpForce => _batRushJumpForce;

		[Tooltip("박쥐 돌진 그랩으로 넘어가기 까지 눌러야 하는 시간")]
		[SerializeField] private float _batRushGrabHoldTime = 0.3f;
		public float BatRushGrabHoldTime => _batRushGrabHoldTime;

		[OverrideLabel("박쥐 돌진 그랩 타임")]
		[SerializeField] private float _batRushGrabTime = 1.0f;
		public float BatRushGrabTime => _batRushGrabTime;

		[OverrideLabel("박쥐 돌진 시 공중 체공할 시간")]
		[SerializeField] private float _batRushAirTime = 0.3f;
		public float BatRushAirTime => _batRushAirTime;

		[Tooltip("박쥐 돌진 시 좌우로 움직일 수 있는 정도")]
		[OverrideLabel("박쥐 돌진 Run Lerp 값")]
		[Range(0f, 1)][SerializeField] private float _umbrellaBatRushAirLerp = 0;
		public float UmbrellaBatRushAirLerp => _umbrellaBatRushAirLerp;

		[OverrideLabel("박쥐 돌진 후 무적 시간")]
		[SerializeField] private float _batRushInvincibleTime = 0.5f;
		public float BatRushInvincibleTime => _batRushInvincibleTime;

		[Tooltip("박쥐 돌진 시 잔상")]
		[SerializeField] private float[] _batRushTrails = { 1 };
		public float[] BatRushTrails => _batRushTrails;

		[Tooltip("(x : 시간, y : 타임 스케일)")]
		[OverrideLabel("박쥐 돌진 시 시간 커브")]
		[SerializeField] private AnimationCurve _batRushTimeScale;
		public AnimationCurve BatRushTimeScale => _batRushTimeScale;

		[Space(20)]

		[Header("Obstacle")]
		[Tooltip("바람의 영향을 받으면 Wind Gravity Multiple만큼 기본 중력이 적용됩니다.")]
		[Range(0f, 1)][SerializeField] private float _windMaxGravityMultiple = 0.7f;
		public float WindMaxGravityMultiple => _windMaxGravityMultiple;

		[Tooltip("바람의 영향을 받으면 Wind Fall Multiple만큼 최대 낙하 속력이 적용됩니다.")]
		[Range(0f, 1)][SerializeField] private float _windMaxFallMultiple = 0.7f;
		public float WindMaxFallMultiple => _windMaxFallMultiple;

		[OverrideLabel("바람 활공 속도")]
		[SerializeField] private float _windGlidingSpeed = 7;
		public float WindGlidingSpeed => _windGlidingSpeed;

		[Space(20)]
		[Header("Assists")]
		[Tooltip("땅이나 벽에서 떨어졌을 때 점프가 가능한 시간")]
		[OverrideLabel("코요테 타임")]
		[Range(0.01f, 0.5f)][SerializeField] private float _coyoteTime;
		public float CoyoteTime => _coyoteTime;

		[Tooltip("점프 선 입력시 이 시간안에 땅에 닿을 시 점프")]
		[OverrideLabel("점프 입력 버퍼 시간")]
		[Range(0.01f, 0.5f)][SerializeField] private float _jumpInputBufferTime;
		public float JumpInputBufferTime => _jumpInputBufferTime;

		[Space(20)]
		[Header("플레이어 옵션")]

		[Tooltip("좌우 방향키로 벽을 탈 수 있게 설정합니다.")]
		[OverrideLabel("좌우 슬라이드 활성화")]
		[SerializeField] private bool _isMoveXSliding = true;
		public bool IsMoveXSliding => _isMoveXSliding;

		[Tooltip("기존 설정된 우산 보정 각도 X 에임 보정 값")]
		[OverrideLabel("에임 보정")]
		[SerializeField] private float _aimCorrection = 1;
		public float AimCorrection => _aimCorrection;


		//Unity Callback, called when the inspector updates
		private void OnValidate()
		{
			//Calculate gravity strength using the formula (gravity = 2 * jumpHeight / timeToJumpApex^2) 
			_gravityStrength = -(2 * _jumpHeight) / (_jumpTimeToApex * _jumpTimeToApex);

			//Calculate the rigidbody's gravity scale (ie: gravity strength relative to unity's gravity value, see project settings/Physics2D)
			_gravityScale = _gravityStrength / Physics2D.gravity.y;

			//Calculate are run acceleration & deceleration forces using formula: amount = ((1 / Time.fixedDeltaTime) * acceleration) / runMaxSpeed
			_runAccelAmount = (50 * _runAcceleration) / _runMaxSpeed;
			_runDecelAmount = (50 * _runDeceleration) / _runMaxSpeed;

			_wallWalkAccelAmount = (50 * _wallWalkAcceleration) / _wallWalkMaxSpeed;
			_wallWalkDeccelAmount = (50 * _wallWalkDecceleration) / _wallWalkMaxSpeed;

			//Calculate jumpForce using the formula (initialJumpVelocity = gravity * timeToJumpApex)
			_jumpForce = Mathf.Abs(_gravityStrength) * _jumpTimeToApex;

			#region Variable Ranges
			_runAcceleration = Mathf.Clamp(_runAcceleration, 0.01f, _runMaxSpeed);
			_runDeceleration = Mathf.Clamp(_runDeceleration, 0.01f, _runMaxSpeed);
			#endregion
		}

		private void Awake()
		{
			OnValidate();
		}

		public void SetIsMoveXSliding(bool value)
		{
			_isMoveXSliding = value;
		}

		public void SetAimCorrection(float value)
		{
			_aimCorrection = value;
		}
	}
}