using UnityEngine;

namespace UB.Animation
{
    public enum PlayerAnimationEnum
    {
        NONE = 0,
        IDLE,
        RUN,
        JUMP,
        FALL,
        LANDING,
        GLIDING,
        AIM,
        THROW,
        HANG,
        HANGWALL,
        ATTACK,
        RETURN,
        WALLUP,
        WALLDOWN,
        DAMAGED,
        DEATH,
        IDLE2,
        HANGCEILING
    }

    public enum PlayerAnimationLayer
    {
        MOVEMENT = 0,
        AIM,
        UMBRELLA
    }

    public class PlayerAnimation : Singleton<PlayerAnimation>
    {
        private Animator _animator;
        private PlayerMovement _playerMovement;
        private PlayerAnimationHandler _playerAnimationHandler;
        private TargetAim[] _targetAim;

        [Header("Animator Properties")]
        public string HorizontalInputProperty = "moveInputX";
        public string VerticalInputProperty = "moveInputY";
        public string VerticalSpeedProperty = "velocityY";
        public string GroundedProperty = "isGround";
        public string JumpTriggerProperty = "Jump";

        protected override void Awake()
        {
            base.Awake();
            _animator = GetComponent<Animator>();
            _targetAim = GetComponentsInChildren<TargetAim>();
            _playerAnimationHandler = GetComponent<PlayerAnimationHandler>();
        }
        private void Start()
        {
            _playerMovement = PlayerMovement.Instance;
        }

        private void Update()
        {
            _animator.SetFloat(HorizontalInputProperty, Mathf.Abs(_playerMovement.GetMoveInput().x));
            _animator.SetFloat(VerticalInputProperty, Mathf.Abs(_playerMovement.GetMoveInput().y));
            _animator.SetFloat(VerticalSpeedProperty, _playerMovement.GetVelocity().y);
            _animator.SetBool(GroundedProperty, _playerMovement.LastOnGroundTime > 0);
        }

        public void OnJumpTrigger()
        {
            _animator.SetTrigger(JumpTriggerProperty);
        }

        /// <summary>
        /// 강제로 애니메이션을 실행합니다.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="track">트랙 번호</param>
        public void PlayForceAnimation(PlayerAnimationEnum name, PlayerAnimationLayer track = 0)
        {
            _animator.Play(name.ToString(), (int)track);
            //_animator.PlayInFixedTime(name.ToString(), (int)track, ReturnCurAnimation());
        }
        /// <summary>
        /// 강제로 한번만 애니메이션을 실행합니다.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="track">트랙 번호</param>
        public void PlayForceAnimationOnce(PlayerAnimationEnum name, PlayerAnimationLayer track = 0, bool notNextAnimation = false)
        {
            _playerAnimationHandler.PlayOneShot(_playerAnimationHandler.GetAnimationForState(name.ToString()), (int)track, notNextAnimation);

            // 우산 알파값 조절
            if (name == PlayerAnimationEnum.THROW)
                SetUmbrellaAlpha(false);
            if (name == PlayerAnimationEnum.RETURN)
                SetUmbrellaAlpha(true);
            if (name == PlayerAnimationEnum.AIM)
            {
                Umbrella.Instance.CancelAimCoroutine();
            }
        }

        public void EmptyForceAnimation(PlayerAnimationLayer track = 0)
        {
            _animator.Play(PlayerAnimationEnum.NONE.ToString(), (int)track);
            _playerAnimationHandler.PlayEmptyLayer((int)track);
        }

        public void SetUmbrellaAlpha(bool isAlpha)
        {
            foreach (var target in _targetAim)
            {
                target.SetUmbrellaAlpha = isAlpha;
            }
        }

        public float GetCurAnimationDuration()
        {
            return _playerAnimationHandler.GetCurAnimationDuration();
        }
    }
}