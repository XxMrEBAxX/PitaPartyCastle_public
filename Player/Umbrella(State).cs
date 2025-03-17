using System;
using UnityEngine;
using UB.EVENT;
using UB.Animation;
using Unity.VisualScripting;

namespace UB
{
	public enum UmbrellaState
	{
		FOLD = 0,
		ATTACK,
		GLIDING,
		BATRUSH,
		THROW,
		STICK,
		RETURN
	}

	public partial class Umbrella : Singleton<Umbrella>
	{
		public StateMachine<Umbrella> CurrentUmbrellaState { get; private set; }
		public UmbrellaState CurrentUmbrellaStateEnum { get; private set; }
		public UmbrellaState PreviousUmbrellaStateEnum { get; private set; }

		private void StartState()
		{
			CurrentUmbrellaState = StateUmbrellaFold.Instance;
			CurrentUmbrellaState.Init();
		}

		private void UpdateState()
		{
			CurrentUmbrellaState.action(this);
		}

		public void SetCurUmbrellaState(UmbrellaState umbrellaState)
		{
			CurrentUmbrellaStateEnum = umbrellaState;
		}

		public void SetPreviousUmbrellaState(UmbrellaState umbrellaState)
		{
			PreviousUmbrellaStateEnum = umbrellaState;
		}

		/// <summary>
		/// 현재 우산 상태에서 다른 상태로 전환합니다. <para> </para>
		/// 이 함수가 호출되고 또 다른 ChangeState 함수가 호출되지 않도록 return 하길 권장합니다.
		/// </summary>
		/// <param name="state"></param><param name="immediately">즉시 Enter함수를 호출합니다.</param>
		public void ChangeState(StateMachine<Umbrella> state, bool immediately = false)
		{
			CurrentUmbrellaState.Exit(this);
			CurrentUmbrellaState = state;
			if (immediately)
				CurrentUmbrellaState.Enter(this);
			else
				CurrentUmbrellaState.Init();
		}

		private Coroutine _cancelAimCoroutine;
		#region FOLD
		public void EnterFoldState()
		{
			_spriteRenderer.sprite = umbrellaSprite[0];
			_spriteRenderer.gameObject.SetActive(false);
			if (_checks.activeInHierarchy)
				_checks.SetActive(false);

			IsThrewUmbrella = false;
			_mouseDotted.Active = true;

			_cancelAimCoroutine = StartCoroutine(WaitAndRunFunctionRoutine(() => { _playerAnimation.PlayForceAnimation(PlayerAnimationEnum.NONE, PlayerAnimationLayer.AIM); }, 0.23f));
		}

		public void UpdateFoldState()
		{
			if (IsHoldAttackButton && LastOnAttackCoolTime == 0)
			{
				ChangeState(StateUmbrellaAttack.Instance);
				return;
			}

			if (IsHoldGlidingButton && (_playerMovement.AbleGliding || _playerMovement.IsWind) && Direction.y > 0)
			{
				ChangeState(StateUmbrellaGliding.Instance);
				return;
			}
		}

		public void ExitFoldState()
		{
			if (!_checks.activeInHierarchy)
				_checks.SetActive(true);

			//_playerAnimation.PlayForceAnimation(PlayerAnimationEnum.AIM, PlayerAnimationLayer.AIM);
		}
		#endregion
		#region ATTACK
		public void EnterAttackState()
		{
			IsHoldAttackButton = false;
			_mouseDotted.Active = false;
			LastParryButtonTime = _playerData.UmbrellaParryingTime;
			LastOnAttackCoolTime = _playerData.UmbrellaAttackCooltime;

			_attackRange.gameObject.SetActive(true);

			LastOnAttackTime = _playerData.UmbrellaAttackTime;

			if (LastOnTargetingTime > 0)
			{
				Direction = (TargetAimPosition - (Vector2)transform.position).normalized;
				LastOnTargetingTime = 0;
			}
			else
				SetAndGetMouseDir();

			SetRotationUmbrella(Direction);

			IsUmbrellaFixed = true;
			_hitAttackList.Clear();

			SoundManager.Instance.PlaySFX(SoundManager.SFXNames.Attack);
			_playerAnimation.PlayForceAnimationOnce(PlayerAnimationEnum.ATTACK, PlayerAnimationLayer.UMBRELLA);
			_playerMovement.TurnFixed(Direction.x > 0, LastOnAttackTime);

			var attackObj = EffectManager.Instance.PlayAndGetEffect(EffectEnum.AttackFX);
			attackObj.transform.SetParent(_player.transform);
			attackObj.transform.localScale = Vector3.one;
			attackObj.transform.position = transform.position + (Vector3)Direction * 2;
			attackObj.transform.rotation = Quaternion.Euler(0, 0, _rotZ + 90);

		}

		public void UpdateAttackState()
		{
			if (LastOnAttackTime <= 0)
			{
				// if (IsHoldAttackButton)
				// 	ChangeState(StateUmbrellaUnfold.Instance);
				// else
				ChangeState(StateUmbrellaFold.Instance);

				return;
			}

			Vector3 _pos = _attackRange.transform.position;
			Vector3 dir = (_pos - transform.position).normalized;
			float rotZ = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90;
			Collider2D[] hits = Physics2D.OverlapBoxAll(_pos, _attackRange.size, rotZ, attackLayer);

			foreach (Collider2D e in hits)
			{
				ProcessAttack(e.gameObject, _player.AttackDamage);
			}

			if (hits.Length > 0 && !_playerMovement.IsAirHolding())
			{
				// 공중 체공
				bool isAbleCount = CountAttackHolding < _playerData.AttackAirCount;
				bool isFalling = _playerMovement.CurrentMovementStateEnum == PlayerMovementState.FALL || _playerMovement.CurrentMovementStateEnum == PlayerMovementState.GLIDING;
				if (isFalling && isAbleCount)
				{
					_playerMovement.AirHolding(_playerData.AttackAirTime, true);
					CountAttackHolding++;
				}
			}
		}

		public void ExitAttackState()
		{
			_attackRange.gameObject.SetActive(false);
			//_umbrellaCollider.SetActive(true);
			IsUmbrellaFixed = false;
		}
		#endregion
		#region GLIDING
		public void EnterGlidingState()
		{
			_playerMovement.ChangeState(StatePlayerMovementGliding.Instance);
			_playerAnimation.EmptyForceAnimation(PlayerAnimationLayer.AIM);
		}

		public void UpdateGlidingState()
		{
			bool ableGliding = _playerMovement.AbleGliding && IsHoldGlidingButton && !_playerMovement.IsWind && _playerMovement.CurrentMovementStateEnum == PlayerMovementState.GLIDING;
			bool ableWindGliding = IsHoldGlidingButton && _playerMovement.IsWind && _playerMovement.CurrentMovementStateEnum == PlayerMovementState.GLIDING;
			if (ableGliding)
			{
				float y = MathF.Max(MathF.Min(_playerMovement.PlayerRigidBody.velocity.y, 0), -_playerData.UmbrellaFallVelocity);

				_playerMovement.SetVelocity(_playerMovement.PlayerRigidBody.velocity.x, y);
			}
			else if (ableWindGliding)
			{
				_playerMovement.SetVelocity(PlayerMovement.Instance.PlayerRigidBody.velocity.x, _playerData.WindGlidingSpeed);
			}
			else
			{
				// 바람 활공에 벗어나도 바람 활공 속도보다 빠르지 않으면 활공 상태를 유지함
				bool isAbleWindGliding = _playerMovement.GetVelocity().y <= _playerData.WindGlidingSpeed && IsHoldGlidingButton && _playerMovement.CurrentMovementStateEnum == PlayerMovementState.GLIDING;
				if (isAbleWindGliding)
					return;

				// if (IsHoldAttackButton)
				// 	ChangeState(StateUmbrellaUnfold.Instance);
				// else
				ChangeState(StateUmbrellaFold.Instance);
				return;
			}

			if (IsHoldAttackButton)
			{
				// 움직임 도중에 공격시 이동값 관련 초기화
				_playerMovement.SettingAirAttackMovementStateInit();
				ChangeState(StateUmbrellaAttack.Instance);
				return;
			}
		}

		public void ExitGlidingState()
		{

		}
		#endregion
		#region THROW
		public void EnterThrowState()
		{
			InitializeUmbrellaThrowOrRush();
			_mouseDotted.Active = false;
			_originThrewWorldPos = transform.position;
			IsAbleReturnUmbrella = false;
			_checks.SetActive(false);
			ReturnStick = false;

			_trailRenderer.emitting = true;

			// 공중 체공
			bool isAbleCount = CountThrowAirHolding < _playerData.UmbrellaThrowAirCount;
			bool isFalling = _playerMovement.CurrentMovementStateEnum == PlayerMovementState.FALL || _playerMovement.CurrentMovementStateEnum == PlayerMovementState.GLIDING;
			if (isFalling && isAbleCount)
			{
				_playerMovement.AirHolding(_playerData.UmbrellaThrowAirTime, true);
				CountThrowAirHolding++;
			}

			_playerAnimation.PlayForceAnimationOnce(PlayerAnimationEnum.THROW, PlayerAnimationLayer.UMBRELLA);
			_playerMovement.TurnFixed(Direction.x > 0, 0.3f);

			// 우산 회수 딜레이
			StartCoroutine(WaitAndRunFunctionRoutine(() => { IsAbleReturnUmbrella = true; }, _playerData.UmbrellaReturnAbleTime));

			_playerAnimation.PlayForceAnimation(PlayerAnimationEnum.AIM, PlayerAnimationLayer.AIM);
			_cancelAimCoroutine = StartCoroutine(WaitAndRunFunctionRoutine(() => { _playerAnimation.PlayForceAnimation(PlayerAnimationEnum.NONE, PlayerAnimationLayer.AIM); }, 0.23f));

			SoundManager.Instance.PlaySFX(SoundManager.SFXNames.Attack);
		}

		public void UpdateThrowState()
        {
            if (_returnStick)
            {
                StickHitPoint = transform.position;
                _stickUmbrella.transform.position = StickHitPoint;
                ChangeState(StateUmbrellaStick.Instance, true);
                return;
            }

            Vector2 destination = Direction * (_throwSpeed * TimeManager.Instance.GetDeltaTime());
            Vector2 SizeY = _spriteRenderer.sprite.bounds.size.y * Direction;

            float LastThrewSqrMagnitude = ((Vector2)transform.position - _originThrewWorldPos).sqrMagnitude;
            if (LastThrewSqrMagnitude >= Mathf.Pow(_maxThrowDistance, 2))
            {
                ReturnStick = true;
                StickHitPoint = (Vector2)transform.position + SizeY;
                ChangeState(StateUmbrellaStick.Instance, true);
                return;
            }

            Vector2 curPosMagnitude = (Vector2)transform.position - _previousThrowPosition;
            RaycastHit2D hit = Physics2D.Linecast(_previousThrowPosition, (Vector2)transform.position + SizeY * 2, AbleStickLayer);

            UmbrellaMove(destination, hit, SizeY);
            if(hit)
				CheckThrowHitLayer(SizeY, hit);

            // 공격
            Vector2 attackSize = umbrellaSprite[0].bounds.size;
			int layer = attackLayer & ~LayerMask.GetMask(LayerEnum.Enemy.ToString());
            Collider2D[] returnAttackList = Physics2D.OverlapBoxAll((Vector2)transform.position + destination, attackSize, _rotZ, layer);
            if (returnAttackList.Length > 0)
            {
                foreach (var e in returnAttackList)
                {
                    ProcessAttack(e.gameObject, _player.AttackDamage);
                }
            }
#if UNITY_EDITOR
            returnAttackCenterPosition = (Vector2)transform.position + destination;
            returnAttackSize = attackSize;
            testQuaternion = Quaternion.Euler(0, 0, _rotZ);
#endif

        }

        public void ExitThrowState()
		{
			_trailRenderer.emitting = false;
		}
		#endregion
		#region STICK
		public void EnterStickState()
		{
			_spriteRenderer.gameObject.SetActive(false);
			_stickUmbrella.SetCollider(HitWallVector);
			_stickUmbrella.transform.position = StickHitPoint;
		}

		public void UpdateStickState()
		{
			if (_returnStick)
			{
				// 우산을 잡고 있는데 회수하면 매달리기 취소
				if (_playerMovement.CurrentMovementStateEnum == PlayerMovementState.LEDGE && _playerMovement.LedgeTag == "Umbrella")
				{
					_playerMovement.ChangeState(StatePlayerMovementFall.Instance);
				}
				ChangeState(StateUmbrellaReturn.Instance);
				return;
			}

			bool isUmbrellaLedge = (_playerMovement.LedgeTag == "Umbrella" && _playerMovement.CurrentMovementStateEnum == PlayerMovementState.LEDGE)
					|| _playerMovement.CurrentMovementStateEnum == PlayerMovementState.BATRUSH || _playerMovement.CurrentMovementStateEnum == PlayerMovementState.RUSH;
			bool isStickWall = _stickUmbrella.GetActive();
			if (isStickWall && !isUmbrellaLedge && !_returnStick && LastOnBatRushButtonTime > 0)
			{
				StartBatRush();
			}
		}

		public void ExitStickState()
		{
			_stickUmbrella.Disable();
			_spriteRenderer.gameObject.SetActive(true);

			HitWallVector = Vector2.zero;
		}
		#endregion
		#region RETURN
		public void EnterReturnState()
		{
			_trailRenderer.emitting = true;

			_hitAttackList.Clear();
			if (!ReferenceEquals(curAbleStickObstacle, null))
			{
				curAbleStickObstacle.ReturnCall();
				curAbleStickObstacle = null;
			}

			if (!ReferenceEquals(HitObject, null))
			{
				if (HitObject.TryGetComponent<MovementInteractive>(out var movementInteractive))
				{
					movementInteractive.Remove(_stickUmbrella.gameObject);
				}
			}

			HitObject = null;
			_playerAnimation.PlayForceAnimation(PlayerAnimationEnum.AIM, PlayerAnimationLayer.AIM);
		}

		public void UpdateReturnState()
		{
			ReturnUmbrella();
		}

		public void ExitReturnState()
		{
			_trailRenderer.emitting = false;
			ReturnStick = false;
			IsHoldAttackButton = false;
			_checks.SetActive(true);
			_spriteRenderer.gameObject.SetActive(false);

			_playerAnimation.PlayForceAnimationOnce(PlayerAnimationEnum.RETURN, PlayerAnimationLayer.UMBRELLA);
		}
		#endregion

#if UNITY_EDITOR
		private Vector2 returnAttackCenterPosition = Vector2.one;
		private Vector2 returnAttackSize = Vector2.one;
		private Quaternion testQuaternion = Quaternion.Euler(0, 0, 0);
		private void OnDrawGizmos()
		{
			Matrix4x4 rotationMatrix = Matrix4x4.TRS(returnAttackCenterPosition, testQuaternion, Vector3.one);
			Gizmos.matrix = rotationMatrix;
			Gizmos.color = Color.red;
			Gizmos.DrawWireCube(Vector2.zero, returnAttackSize);
		}
#endif
	}
}