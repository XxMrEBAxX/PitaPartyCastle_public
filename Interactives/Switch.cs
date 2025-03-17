using UnityEngine;
using UB.EVENT;
using System;
using Spine.Unity;
using DG.Tweening;

namespace UB
{
    [RequireComponent(typeof(UmbrellaInteractive))]
    [RequireComponent(typeof(PlayerAttackInteractive))]
    public sealed class Switch : StageObject, IElectronicObject
    {
        [Tooltip("초기화 시 스위치 상태")]
        [SerializeField] private bool _active = false;

        [Tooltip("영구 지속 상태")]
        [SerializeField] private bool _activeInfinity = true;

        [Tooltip("타이머의 활성화 여부입니다.")]
        [SerializeField] private bool _isTimerActive = false;

        [Tooltip("타이머의 시간입니다.")]
        [SerializeField] private float _timerTime = 3;

        [Tooltip("AbleStick의 여부입니다.")]
        [SerializeField] private bool _isAbleStick = false;

        private UmbrellaInteractive _umbrellaInteractive;
        private PlayerAttackInteractive _playerAttackInteractive;
        //private SpriteRenderer _spriteRenderer;
        private float _curTime = 0;
        private bool _originActive;
        public bool Active { get { return _active; } set { _active = value; } }

        public Action CallActive { get; set; }


        private SkeletonAnimation _ani;
        [SpineAnimation] public string spin;
        [SerializeField] private float timeScaleValue;


        private void Start()
        {
            _umbrellaInteractive = GetComponent<UmbrellaInteractive>();
            _playerAttackInteractive = GetComponent<PlayerAttackInteractive>();
            //_spriteRenderer = GetComponent<SpriteRenderer>();

            _ani = gameObject.transform.GetChild(0).GetComponent<SkeletonAnimation>();
            //_ani.AnimationState.Complete += delegate 
            //{
            //    if(_ani.AnimationName == startSpin)
            //    {
            //        _ani.AnimationState.SetAnimation(0, spin, true);
            //    }
            //};

            Initializing();
        }

        private void Update()
        {
            if (_isTimerActive && !_isAbleStick && _curTime > 0)
            {
                _curTime -= Time.deltaTime;
                if (_curTime <= 0)
                {
                    OnActive();
                }
            }
        }

        private void Initializing()
        {
            //if (_active)
            //    _spriteRenderer.color = Color.red;
            //else
            //    _spriteRenderer.color = Color.gray;
            
            _ani.AnimationState.SetAnimation(0, spin, true);

            if (_active)
                _ani.timeScale = timeScaleValue;
            else
                _ani.timeScale = 0;

            if (_isAbleStick)
            {
                gameObject.layer = LayerMask.NameToLayer("AbleStick");
                _umbrellaInteractive.UmbrellaEventHandler += ActiveCheck;
                _umbrellaInteractive.UmbrellaReturnEventHandler += ActiveCheck;
            }
            else
            {
                gameObject.layer = LayerMask.NameToLayer("Switch");
                _playerAttackInteractive.AttackEventHandler += ActiveCheck;
            }

            _originActive = _active;
        }

        private void ActiveCheck()
        {
            if (_originActive != _active && _activeInfinity || (_curTime > 0 && _isTimerActive))
                return;

            OnActive();

            _curTime = _timerTime;
        }

        private void OnActive()
        {
            if (_active)
            {
                _active = false;
                //_spriteRenderer.color = Color.gray;
                //_ani.AnimationState.SetAnimation(0, EndSpin, false);

                //두투윈 시퀀스 1 > 0
                DOTween.To(() => _ani.timeScale, x => _ani.timeScale = x, 0, 0.3f);
            }
            else
            {
                _active = true;
                //_spriteRenderer.color = Color.red;
                //_ani.AnimationState.SetAnimation(0, startSpin, false);

                //두투윈 시퀀스 0 > 1
                DOTween.To(() => _ani.timeScale, x => _ani.timeScale = x, timeScaleValue, 0.3f);
            }
            if (CallActive != null)
                CallActive();

            stage.IsCleared = true;
        }

        private void Reset()
        {
            _active = _originActive;

            //if (_active)
            //    _spriteRenderer.color = Color.red;
            //else
            //    _spriteRenderer.color = Color.gray;

            if (_active)
                _ani.timeScale = timeScaleValue;
            else
                _ani.timeScale = 0;

            _curTime = 0;
        }

        public override void InitializeStageObject(bool clear)
        {
            Reset();

            if (stage.StageInitializeTypeInstance == StageInitializeType.SAVE)
            {
                if (stage.IsCleared)
                    OnActive();
            }
            else if (stage.StageInitializeTypeInstance == StageInitializeType.CLEAR)
            {
                if (clear)
                    OnActive();
            }
        }
    }
}
