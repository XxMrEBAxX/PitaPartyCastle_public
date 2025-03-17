using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UB.EVENT;
using MyBox;

namespace UB
{
    public class Door : StageObject
    {
        // 문이 열리는 방향
        private enum _direction
        {
            UP,
            DOWN,
            LEFT,
            RIGHT
        }

        [OverrideLabel("초기 문 상태")]
        [SerializeField] private bool _open = false;

        [OverrideLabel("영구 지속 상태")]
        [SerializeField] private bool _openInfinity = false;

        [OverrideLabel("문이 열리는 방향")]
        [SerializeField] private _direction _openDirection = _direction.UP;

        [Header("Legacy"), Tooltip("Legacy 코드이므로 사용 X")]
        [SerializeField] private Switch[] _switches;

        [Tooltip("문이 열리기 위한 활성 오브젝트들")]
        [SerializeField] private List<GameObject> _electronics;

        [OverrideLabel("문 열리고 닫히는 시간")]
        [SerializeField] private float _duration = 1f;

        [OverrideLabel("문 애니메이션")]
        [SerializeField] private Ease _ease = Ease.InOutBounce;

        private GameObject _door;
        private SpriteRenderer _spriteRenderer;
        private bool _originOpen;
        private List<IElectronicObject> _listElectronics = new List<IElectronicObject>();

        protected override void Awake()
        {
            base.Awake();

            #region Legacy
            foreach (var sw in _switches)
            {
                if (_electronics.Contains(sw.gameObject))
                    continue;
                _electronics.Add(sw.gameObject);
            }
            #endregion

            foreach (var e in _electronics)
            {
                if(e.TryGetComponent<IElectronicObject>(out var electronicObject))
                {
                    _listElectronics.Add(electronicObject);
                    electronicObject.CallActive += UpdateAction;
                }
            }
        }
        private void Start()
        {
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            _door = _spriteRenderer.gameObject;

            _originOpen = _open;

        }

        private void UpdateAction()
        {
            if (_listElectronics.Count == 0)
                return;

            bool isAllOn = true;
            foreach (var sw in _listElectronics)
            {
                if (!sw.Active)
                {
                    isAllOn = false;
                    break;
                }
            }

            Active(isAllOn ? !_originOpen : _originOpen);
        }

        private void Active(bool value)
        {
            if(value == _open || (_openInfinity && _originOpen != _open))
                return;

            Animation(value);
            _open = value;
            stage.IsCleared = true;
        }

        private void Animation(bool value)
        {
            _door.transform.DOKill();

            TweenCallback func = () => 
            { 
                _door.SetActive(!value);
                _door.transform.DOComplete(true);
            };
            int sign = 1;
            if(!value)
            {
                sign = 0;
                func();
            }
            
            switch (_openDirection)
            {
                case _direction.UP:
                    _door.transform.DOLocalMoveY(sign * _spriteRenderer.bounds.size.y * _door.transform.localScale.y, _duration).SetEase(_ease).OnComplete(func);
                    break;
                case _direction.DOWN:
                    _door.transform.DOLocalMoveY(-sign * _spriteRenderer.bounds.size.y * _door.transform.localScale.y, _duration).SetEase(_ease).OnComplete(func);
                    break;
                case _direction.LEFT:
                    _door.transform.DOLocalMoveX(-sign * _spriteRenderer.bounds.size.x * _door.transform.localScale.x, _duration).SetEase(_ease).OnComplete(func);
                    break;
                case _direction.RIGHT:
                    _door.transform.DOLocalMoveX(sign * _spriteRenderer.bounds.size.x * _door.transform.localScale.x, _duration).SetEase(_ease).OnComplete(func);
                    break;
            }
        }

        private void OnDestroy() 
        {
            _door.transform.DOKill();
        }

        private void Reset()
        {
            _open = _originOpen;
            Animation(_open);
        }

        public override void InitializeStageObject(bool clear)
        {
            Reset();

            if(stage.StageInitializeTypeInstance == StageInitializeType.SAVE)
            {
                if(stage.IsCleared)
                    Active(!_open);
            }
            else if(stage.StageInitializeTypeInstance == StageInitializeType.CLEAR)
            {
                if(clear)
                    Active(!_open);
            }

            UpdateAction();
        }

        private void OnValidate() {
            foreach(var e in _electronics)
            {
                if(e == null)
                    continue;

                if(!e.TryGetComponent<IElectronicObject>(out var electronicObject))
                {
                    Debug.LogError($"{e.name}은 IElectronicObject를 상속받지 않았습니다.");
                    _electronics.Remove(e);
                    break;
                }
            }
        }
    }
}
