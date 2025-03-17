using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UB.Animation;
using UB.EFFECT;
using UB.UI;
using UnityEngine;

namespace UB
{
    public partial class Player : Singleton<Player>
    {
        public enum PlayerState
        {
            ALIVE = 0,
            DEAD,
            SPECTATE
        }
        public PlayerState CurPlayerState { get; private set; }

        #region Objects & Caching
        private Umbrella _umbrella;
        private PlayerData _playerData;
        private PlayerMovement _playerMovement;
        private CameraManager _cameraManager;
        private HitFX _hitFX;
        private InvincibleFX _invincibleFX;
        private DissolveFX _dissolveFX;
        private PlayerHPUI _hpBar;
        #endregion

        #region Unlock
        [SerializeField] private bool _unlockUmbrella;
        [SerializeField] private bool _unlockTeleport;
        [SerializeField] private bool _unlockGliding;
        [SerializeField] private bool _unlockDoubleJump;
        public bool UnlockUmbrella
        {
            get => _unlockUmbrella;
            set
            {
                _unlockUmbrella = value;
                _umbrella.ChangeUmbrellaActive(value);
            }
        }
        public bool UnlockTeleport
        {
            get => _unlockTeleport;
            set
            {
                _unlockTeleport = value;
            }
        }
        public bool UnlockGliding
        {
            get => _unlockGliding;
            set
            {
                _unlockGliding = value;
            }
        }
        public bool UnlockDoubleJump
        {
            get => _unlockDoubleJump;
            set
            {
                _unlockDoubleJump = value;
            }
        }

        #endregion

        #region PlayerStat
        private int _maxHP;
        private int _currentHP;
        public bool Invincible { get; private set; }
        public int AttackDamage { get; private set; }
        public int BatRushDamage { get; private set; }
        #endregion

        private Coroutine _invincibleRoutine;
        private Coroutine _damagedTimeRoutine;

        private void Start()
        {
            //FIXME - hpBar를 찾는 방법을 수정해야함
            _hpBar = GameObject.Find("PlayerHPUI")?.GetComponent<PlayerHPUI>();
            _playerMovement = PlayerMovement.Instance;
            _playerData = _playerMovement.PlayerDataInstance;
            _umbrella = Umbrella.Instance;
            _hitFX = GetComponentInChildren<HitFX>();
            _invincibleFX = GetComponentInChildren<InvincibleFX>();
            _dissolveFX = GetComponentInChildren<DissolveFX>();
            _cameraManager = CameraManager.Instance;

            _maxHP = _playerData.MaxHP;
            _currentHP = _maxHP;
            AttackDamage = _playerData.Damage;
            BatRushDamage = _playerData.BatRushDamage;
            UnlockUmbrella = _unlockUmbrella;
        }

        private async void Dead()
        {
            _currentHP = _maxHP;
            _playerMovement.SetVelocity(Vector2.zero);
            _playerMovement.ChangeState(StatePlayerMovementDead.Instance);
            PlayerAnimation.Instance.PlayForceAnimation(PlayerAnimationEnum.NONE);
            PlayerAnimation.Instance.PlayForceAnimationOnce(PlayerAnimationEnum.DEATH, 0, true);
            MouseDotted.Instance.SetActiveAndNotChange(false, true);
            CurPlayerState = PlayerState.DEAD;
            StopInvincible();

            await UniTask.Delay((int)(PlayerAnimation.Instance.GetCurAnimationDuration() * 1000));

            _dissolveFX.PlayHit(false);
            await UniTask.Delay((int)(_dissolveFX.Duration * 500));

            await SceneManager.Instance.FadeInAtPlayer(
            () =>
            {
                _dissolveFX.SetDissolve(0);
                _hpBar.SetHPUI(_currentHP);
                CurPlayerState = PlayerState.ALIVE;
                StageManager.Instance.DeadCurrentStage();
                transform.position = StageManager.Instance.CurrentStageSelector.SpawnPoint.position;
                _playerMovement.ChangeState(StatePlayerMovementIdle.Instance);
                MouseDotted.Instance.SetActiveAndNotChange(true, false);
                if (!_unlockUmbrella)
                    MouseDotted.Instance.Active = false;
                PlayerAnimation.Instance.PlayForceAnimation(PlayerAnimationEnum.IDLE);
            });
        }

        public void ParryEffect(Vector3 pos)
        {
            var obj = EffectManager.Instance.PlayAndGetEffect(EffectEnum.ParryFX);
            obj.transform.position = pos;
        }

        /// <summary>
        /// 플레이어에게 피해를 가합니다.
        /// </summary>
        /// <param name="sub">피해량 입니다.</param>
        /// <param name="invincible">피격 무적 적용 여부 입니다.</param>
        public void SubtractHP(int sub, bool invincible = true)
        {
            if (Invincible || CurPlayerState == PlayerState.DEAD)
                return;

            _playerMovement.PlayerAddForce(Vector2.zero);
            if (invincible)
                SetInvincible(_playerData.DamagedInvincible, true);

            _currentHP = Mathf.Clamp(_currentHP - sub, 0, int.MaxValue);

            _hpBar.SetHPUI(_currentHP);
            //StartSlowTime();

            var effect = EffectManager.Instance.PlayAndGetEffect(EffectEnum.DamagedFX);
            effect.transform.position = transform.position;

            _cameraManager.ShakeCam(CameraShakeType.Damaged, new Vector2(1, 0));
            PlayerAnimation.Instance.PlayForceAnimationOnce(PlayerAnimationEnum.DAMAGED);

            SoundManager.Instance.PlaySFX(SoundManager.SFXNames.Hit);
            _hitFX.PlayHit();

            if (_currentHP <= 0)
            {
                Umbrella.Instance.ReturnStick = true;
                Dead();
            }
        }

        private void StartSlowTime()
        {
            _damagedTimeRoutine = StartCoroutine(DamagedSlowTime());
        }

        private IEnumerator<WaitUntil> DamagedSlowTime()
        {
            if (_damagedTimeRoutine != null)
                StopCoroutine(_damagedTimeRoutine);

            _playerMovement.DenyRun = true;
            _playerMovement.SetVelocity(Vector2.zero);

            float sumDeltaTime = 0;
            var lastFrame = _playerData.DamagedTimeScale[_playerData.DamagedTimeScale.length - 1];
            float lastKeyTime = lastFrame.time;
            while (sumDeltaTime < lastKeyTime)
            {
                yield return new WaitUntil(() => { return TimeManager.Instance.AbleSetTimeScale; });
                TimeManager.Instance.SetTimeScale(_playerData.DamagedTimeScale.Evaluate(sumDeltaTime));
                sumDeltaTime += TimeManager.Instance.GetUnscaledDeltaTime();
            }
            _playerMovement.DenyRun = false;
            TimeManager.Instance.SetOriginTimeScale();
        }

        private IEnumerator<WaitForSeconds> SetInvincibleRoutine(float duration)
        {
            Invincible = true;
            yield return new WaitForSeconds(duration);
            Invincible = false;
        }

        /// <summary>
        /// duration 동안 무적 시간을 활성화 합니다.
        /// 무적 시간중에 다시 호출하면 시간을 덮어씌웁니다.
        /// </summary>
        /// <param name="duration"></param>
        public void SetInvincible(float duration, bool playFX = false)
        {
            if (_invincibleRoutine != null)
                StopCoroutine(_invincibleRoutine);

            _invincibleRoutine = StartCoroutine(SetInvincibleRoutine(duration));

            if (playFX)
                _invincibleFX.PlayFX(duration);
        }

        /// <summary>
        /// 무적을 강제로 종료합니다.
        /// </summary>
        public void StopInvincible()
        {
            if (_invincibleRoutine != null)
                StopCoroutine(_invincibleRoutine);

            Invincible = false;
            _invincibleFX.StopFX();
        }

        /// <summary>
        /// 피격 무적 시간 동안 무적이 적용됩니다.
        /// </summary>
        public void DamagedInvincible()
        {
            Debug.LogError("이제 이 함수 호출 안해도 됩니다~");
        }
    }
}