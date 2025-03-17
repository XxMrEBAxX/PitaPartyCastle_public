using System.Threading;
using Cysharp.Threading.Tasks;
using UB.Animation;
using UB.EVENT;
using Unity.VisualScripting;
using UnityEngine;

namespace UB
{
	public enum PlayerMovementState
	{
		// Active State
		IDLE = 1,
		JUMP = 1 << 1,
		FALL = 1 << 2,
		LEDGE = 1 << 3,
		// Passive State
		GLIDING = 1 << 4,
		WALLJUMP = 1 << 5,
		BATRUSH = 1 << 6,
		RUSH = 1 << 7,
		UMBRELLARUSH = 1 << 8,
		SLIDING = 1 << 9,
		DOUBLEJUMP = 1 << 10,
		DEAD = 1 << 11
	}

	public partial class PlayerMovement : Singleton<PlayerMovement>
	{
		public StateMachine<PlayerMovement> CurrentMovementState { get; private set; }
		public PlayerMovementState CurrentMovementStateEnum { get; private set; }
		public PlayerMovementState PreviousMovementStateEnum { get; private set; }

		/// <summary>
		/// 현재 무브먼트 상태에서 다른 상태로 전환합니다. <para> </para>
		/// 이 함수가 호출되고 또 다른 ChangeState 함수가 호출되지 않도록 return 하길 권장합니다.
		/// </summary>
		/// <param name="state"></param><param name="immediately">즉시 Enter함수를 호출합니다.</param>
		public void ChangeState(StateMachine<PlayerMovement> state, bool immediately = false)
		{
			CurrentMovementState.Exit(this);
			CurrentMovementState = state;
			if (immediately)
				CurrentMovementState.Enter(this);
			else
				CurrentMovementState.Init();
		}

		public void SetCurrentMovementState(PlayerMovementState movementState)
		{
			CurrentMovementStateEnum = movementState;
		}

		public void SetPreviousMovementState(PlayerMovementState movementState)
		{
			PreviousMovementStateEnum = movementState;
		}

		#region IDLE
		public void EnterIdleState()
		{
			SetGravityScale(PlayerDataInstance.GravityScale);
			InitializeCounts();
			LastOnMoveTime = IDLE_CHANGE_TIME;
		}
		public void UpdateIdleState()
		{
			// Jump
			if (CanJump() && LastPressedJumpTime > 0)
			{
				ChangeState(StatePlayerMovementJump.Instance);
				return;
			}

			// Fall
			if (LastOnGroundTime == 0)
			{
				ChangeState(StatePlayerMovementFall.Instance);
				return;
			}

			if (_moveInput.x != 0)
			{
				PlayRunFX(_groundCheckPoint.bounds.center);
				LastOnMoveTime = IDLE_CHANGE_TIME;
			}
			else
				StopRunFX();

			if (LastOnMoveTime == 0)
			{
				_playerAnimation.PlayForceAnimationOnce(PlayerAnimationEnum.IDLE2);
				LastOnMoveTime = IDLE_CHANGE_TIME;
			}
		}

		public void ExitIdleState()
		{
			StopRunFX();
		}
		#endregion
		#region Dead
		public void EnterDeadState()
		{
		}
		public void UpdateDeadState()
		{
		}

		public void ExitDeadState()
		{
		}
		#endregion
		#region JUMP
		public void EnterJumpState()
		{
			if (IsWind)
				SetGravityScale(PlayerDataInstance.GravityScale * PlayerDataInstance.WindMaxGravityMultiple);
			else
				SetGravityScale(PlayerDataInstance.GravityScale);

			Jump();

			if (PreviousMovementStateEnum != PlayerMovementState.LEDGE)
				JumpEffect();
		}

		public void UpdateJumpState()
		{
			if (PlayerRigidBody.velocity.y <= 0 || (!IsHoldJumpButton && LastOnJumpTime == 0))
			{
				ChangeState(StatePlayerMovementFall.Instance);
				return;
			}

			if (CanSliding())
			{
				ChangeState(StatePlayerMovementSliding.Instance);
				return;
			}
		}

		public void ExitJumpState()
		{
			if (!IsHoldJumpButton)
			{
				_isJumpCut = true;
			}
			else
			{
				LastOnJumpHangTime = PlayerDataInstance.JumpHangTime;
			}
		}
		#endregion
		#region DOUBLEJUMP
		public void EnterDoubleJumpState()
		{
			if (IsWind)
				SetGravityScale(PlayerDataInstance.GravityScale * PlayerDataInstance.WindMaxGravityMultiple);
			else
				SetGravityScale(PlayerDataInstance.GravityScale);

			SetVelocityInFixedUpdate(new Vector2(GetVelocity().x, 0)).Forget();
			RunActionInFixedUpdate(() => { DoubleJump(); }).Forget();

			JumpEffect();
		}

		public void UpdateDoubleJumpState()
		{
			if (PlayerRigidBody.velocity.y <= 0)
			{
				ChangeState(StatePlayerMovementFall.Instance);
				return;
			}

			if (CanSliding())
			{
				ChangeState(StatePlayerMovementSliding.Instance);
				return;
			}
		}

		public void ExitDoubleJumpState()
		{
			LastOnJumpHangTime = PlayerDataInstance.JumpHangTime;
		}
		#endregion
		#region FALL
		public void EnterFallState()
		{
			if (_holdingRoutine != null)
			{
				return;
			}

			if (_isJumpCut)
			{
				if (IsWind)
					SetGravityScale(PlayerDataInstance.GravityScale * PlayerDataInstance.JumpCutGravityMultiple * PlayerDataInstance.WindMaxGravityMultiple);
				else
					SetGravityScale(PlayerDataInstance.GravityScale * PlayerDataInstance.JumpCutGravityMultiple);
			}
			else if (LastOnJumpHangTime > 0)
			{
				SetGravityScale(PlayerDataInstance.GravityScale * PlayerDataInstance.JumpHangGravityMultiple);
			}
			else if (IsWind)
				SetGravityScale(PlayerDataInstance.GravityScale * PlayerDataInstance.WindMaxGravityMultiple);
			else
				SetGravityScale(PlayerDataInstance.GravityScale * PlayerDataInstance.FallGravityMultiple);
		}

		public void UpdateFallState()
		{
			if (LastOnGroundTime > 0 && LastOnJumpTime == 0)
			{
				_playerAnimation.PlayForceAnimation(PlayerAnimationEnum.FALL);

				ChangeState(StatePlayerMovementIdle.Instance);
				return;
			}

			if (CanSliding())
			{
				ChangeState(StatePlayerMovementSliding.Instance);
				return;
			}

			if (LastOnJumpHangTime <= 0 && PlayerRigidBody.gravityScale == PlayerDataInstance.GravityScale * PlayerDataInstance.JumpHangGravityMultiple)
			{
				SetGravityScale(PlayerDataInstance.GravityScale * PlayerDataInstance.FallGravityMultiple);
			}
		}

		public void ExitFallState()
		{
			_isJumpCut = false;
		}
		#endregion
		#region LEDGE
		public void EnterLedgeState()
		{
			SetGravityScale(0);

			_isJumpCut = false;
			LastOnGroundTime = 0;
			InitializeCounts();

			SetVelocityInFixedUpdate(Vector2.zero).Forget();

			if (LedgeRBPos != Vector2.zero)
				RunActionInFixedUpdate(() => SetPlayerPosition(LedgeRBPos)).Forget();

			IsHoldJumpButton = false;
			CanGrab = false;
			_lastLedgeTime = SLIDE_JUMP_TERM;

			if (LedgeTag == "Umbrella")
			{
				transform.SetParent(StickUmbrella.Instance.transform);
			}
			else
			{
				AddMovementInteractive();
			}

			HangAnimation();
		}

		public void UpdateLedgeState()
		{
			bool isAbleMoveKeyJump = ((_moveInput.x < 0 && !IsFacingRight) || (_moveInput.x > 0 && IsFacingRight))
							&& !(_umbrella.HitWallVector.y <= -0.01f && LedgeTag == "Umbrella") && LedgeTag == "Untagged";

			if (_moveInput.y < 0 && IsHoldJumpButton)
			{
				ChangeState(StatePlayerMovementFall.Instance);
				return;
			}
			else if (IsHoldJumpButton || (isAbleMoveKeyJump && _lastLedgeTime <= 0))
			{
				ChangeState(StatePlayerMovementJump.Instance, true);
				return;
			}

			if (LedgeTag == "Umbrella" && _umbrella.CurrentUmbrellaStateEnum != UmbrellaState.STICK)
			{
				ChangeState(StatePlayerMovementFall.Instance);
				return;
			}

			//ANCHOR - 매달리기 후 떨어지는 상황 굳이 필요한가? 무빙플랫폼 움직이면 풀리는 경우때문에 뺐음
			// if (LedgeTag == "Untagged")
			// {
			// 	RunActionInFixedUpdate(() =>
			// 	{
			// 		if (!LedgeDetection.Instance.CheckAbleLedgeAfter())
			// 		{
			// 			ChangeState(StatePlayerMovementFall.Instance);
			// 			return;
			// 		}
			// 	}).Forget();
			// }

			SetVelocityInFixedUpdate(Vector2.zero).Forget();

			// 벽 여부 상태에 따라 애니메이션 변경
			HangAnimation();
		}

		public void ExitLedgeState()
		{
			LedgeObject = null;
			// 연속 매달리기 방지
			StartCoroutine(WaitAndRunFunctionCoroutine(() => { CanGrab = true; }, GRAB_TERM));
			if (LedgeTag == "Umbrella")
			{
				_umbrella.ReturnStick = true;
				transform.SetParent(null);
			}
			RemoveMovementInteractive();

			_playerAnimation.PlayForceAnimation(PlayerAnimationEnum.FALL);
		}
		#endregion
		#region GLIDING
		public void EnterGlidingState()
		{
			LastOnJumpHangTime = 0;
			StopAirHolding();
			SetGravityScale(GLIDING_GRAVITY);
		}

		public void UpdateGlidingState()
		{
			_playerAnimation.PlayForceAnimation(PlayerAnimationEnum.GLIDING);

			UpdateFallState();
			if (_umbrella.CurrentUmbrellaStateEnum != UmbrellaState.GLIDING)
			{
				ChangeState(StatePlayerMovementFall.Instance);
				return;
			}
		}

		public void ExitGlidingState()
		{
			_playerAnimation.PlayForceAnimation(PlayerAnimationEnum.FALL);
		}
		#endregion
		#region BATRUSH
		public void EnterBatRushState()
		{
			if (_batRushPos == Vector2.zero)
			{
				ChangeState(StatePlayerMovementFall.Instance, true);
				return;
			}

			SetVelocity(Vector2.zero);
			SettingAirAttackMovementStateInit();
			_runFX.Play();
			CameraManager.Instance.SetDamping(0);
			//FIXME - BatRush 하는 동안 플레이어 위치 이탈 방지 코드인데 오류 있을 수 있음
			transform.SetParent(StickUmbrella.Instance.transform);
			CheckDirectionToFace(Umbrella.Instance.Direction.x > 0, true);

			// 이펙트
			var obj = EffectManager.Instance.PlayAndGetEffect(EffectEnum.DashFX);
			obj.transform.position = _batRushPos;

			Vector2 subPos = _batRushPos - (Vector2)transform.position;
			Vector2 dir = subPos.normalized;
			// 잔상 이펙트
			foreach (var lerpAmount in PlayerDataInstance.BatRushTrails)
			{
				Vector2 lerpPosition = new Vector2(Mathf.Lerp(transform.position.x, _batRushPos.x, lerpAmount),
				 Mathf.Lerp(transform.position.y, _batRushPos.y, lerpAmount));
				lerpPosition -= dir;

				obj = EffectManager.Instance.PlayAndGetEffect(EffectEnum.PCTrailFX);
				var main = obj.GetComponent<ParticleSystem>().main;

				obj.transform.position = lerpPosition;
				main.startLifetime = lerpAmount;
				main.startColor = new Color(lerpAmount, lerpAmount, lerpAmount);

				Vector3 scale = obj.transform.localScale;
				scale.x = subPos.x > 0 ? 1 : -1;
				obj.transform.localScale = scale;
			}

			SetPlayerPosition(_batRushPos);

			if (_batRushPos == (Vector2)_stickUmbrella.SlidingTransform.position)
			{
				_rightWallGameObject = _umbrella.HitObject;
				_leftWallGameObject = _umbrella.HitObject;
				ChangeState(StatePlayerMovementSliding.Instance, true);
				_umbrella.ForceReturnUmbrella();
			}

			_playerAnimation.PlayForceAnimation(PlayerAnimationEnum.FALL);
		}

		public void UpdateBatRushState()
		{
			SetVelocity(Vector2.zero);

			if (_umbrella.HitThrowLayer == LayerMask.GetMask(LayerEnum.Enemy.ToString()))
			{
				ChangeState(StatePlayerMovementFall.Instance, true);
			}

			if (_umbrella.HitThrowLayer == LayerMask.GetMask(LayerEnum.AbleStick.ToString()))
			{
				ChangeState(StatePlayerMovementFall.Instance, true);
				AirHolding(PlayerDataInstance.BatRushAirTime, false, true);
				_umbrella.ForceReturnUmbrella();
			}
			else if (_umbrella.HitWallVector.y < 0.01f)
			{
				LedgePosition(Vector2.zero, "Umbrella");
			}
			else
			{
				ChangeState(StatePlayerMovementFall.Instance, true);
			}
		}

		public void ExitBatRushState()
		{
			InitializeCounts();
			_umbrella.EndBatRush();
			CameraManager.Instance.DefaultDamping();
			//FIXME - 
			transform.SetParent(null);

			_umbrella.curAbleStickObstacle?.BatRushCall();
		}
		#endregion
		#region RUSH
		public void EnterRushState()
		{
			_rushGuide.Active = true;
			_grabTime = Time.time;
		}

		public void UpdateRushState()
		{
			CheckDirectionToFace(_rushGuide.Direction.x > 0, true);
			bool isAbleChangeDirection = _umbrella.IsHoldBatRushButton && ((Time.time - _grabTime) < PlayerDataInstance.BatRushGrabTime);
			if (!isAbleChangeDirection)
			{
				Vector2 dir = _rushGuide.Direction;
				ChangeState(StatePlayerMovementFall.Instance);
				PlayerAddForce(dir * PlayerDataInstance.BatRushJumpForce, false);
				_umbrella.ForceReturnUmbrella();
			}
		}

		public void ExitRushState()
		{
			_rushGuide.Active = false;
			SettingAirAttackMovementStateInit();
			DenyJumpTime = 0.2f;
		}
		#endregion
		#region UMBRELLARUSH
		public void EnterUmbrellaRushState()
		{
			transform.SetParent(_umbrella.transform);
			PlayerRigidBody.isKinematic = true;

			SetVelocity(Vector2.zero);
			SettingAirAttackMovementStateInit();
			CheckDirectionToFace(Umbrella.Instance.Direction.x > 0, true);
		}

		public void UpdateUmbrellaRushState()
		{

		}

		public void ExitUmbrellaRushState()
		{
			transform.SetParent(null);
			PlayerRigidBody.isKinematic = false;
			transform.rotation = Quaternion.Euler(0, 0, 0);

			InitializeCounts();
		}
		#endregion
		#region SLIDING
		public void EnterSlidingState()
		{
			SetVelocity(Vector2.zero);

			InitializeCounts();

			AddMovementInteractive();
		}

		public void UpdateSlidingState()
		{
			AddMovementInteractive();
			PlayRunFX(_frontWallCheckPoint.bounds.center);

			bool isAbleMoveKey = ((_moveInput.x > 0 && !IsFacingRight)
							  || (_moveInput.x < 0 && IsFacingRight))
							  && IsHoldMoveButton;
			bool isNotWallTime = LastOnGroundTime > 0 || LastOnWallTime == 0 || (LastOnSlidingLeftTime == 0 && LastOnSlidingRightTime == 0);
			if (isNotWallTime || isAbleMoveKey)
			{
				_playerAnimation.PlayForceAnimation(PlayerAnimationEnum.FALL);
				ChangeState(StatePlayerMovementFall.Instance);
				return;
			}

			if (LastPressedJumpTime > 0)
			{
				ChangeState(StatePlayerMovementWallJump.Instance, true);
				return;
			}

			if (_moveInput.y > 0)
				_playerAnimation.PlayForceAnimation(PlayerAnimationEnum.WALLUP);
			else
				_playerAnimation.PlayForceAnimation(PlayerAnimationEnum.WALLDOWN);
		}

		public void ExitSlidingState()
		{
			StopRunFX();
			RemoveMovementInteractive();
		}
		#endregion
		#region WALLJUMP
		public void EnterWallJumpState()
		{
			SetVelocityInFixedUpdate(Vector2.zero).Forget();
			RunActionInFixedUpdate(() => { WallJump(LastOnWallRightTime > 0 ? -1 : 1); }).Forget();

			if (IsWind)
				SetGravityScale(PlayerDataInstance.GravityScale * PlayerDataInstance.WindMaxGravityMultiple);
			else
				SetGravityScale(PlayerDataInstance.GravityScale);

			JumpEffect();
		}

		public void UpdateWallJumpState()
		{
			if (GetVelocity().y <= 0 || !IsHoldJumpButton)
			{
				ChangeState(StatePlayerMovementFall.Instance);
				return;
			}
		}

		public void ExitWallJumpState()
		{
			if (!IsHoldJumpButton)
			{
				_isJumpCut = true;
			}
			else
			{
				LastOnJumpHangTime = PlayerDataInstance.JumpHangTime;
			}
		}
		#endregion
	}
}