using System;
using System.Collections.Generic;
using DG.Tweening;
using MyBox;
using UB.EFFECT;
using UB.EVENT;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine.Splines;

namespace UB
{
    [RequireComponent(typeof(SplineContainer))]
    public class MovingPlatform : MonoBehaviour
    {
        public enum MoveType
        {
            Once,
            Loop,
            PingPong
        }

        public GameObject Platform;
        [Range(0, 1)] public float LerpAmount;

        [Header("설정"), Space(10)]

        [Tooltip("이동을 완료하는 시간"), OverrideLabel("이동 시간")]
        [SerializeField] private float _duration = 5;

        [Tooltip("움직임을 결정합니다. (sin, cos 함수 그래프 등)"), OverrideLabel("이동 그래프")]
        [SerializeField] private Ease _ease = Ease.Linear;

        [Tooltip("Once : 한번만 움직입니다.\nLoop : 항상 움직입니다.\nPingPong : 한번 왕복합니다."), OverrideLabel("움직임 종류")]
        [SerializeField] private MoveType _moveType;

        [Tooltip("움직임이 끝난 뒤 파괴 여부입니다."), OverrideLabel("파괴 여부")] [SerializeField]
        private bool _isDestroy;

        private SplineContainer _splineContainer;
        private MovementInteractive _movementInteractive;
        private SpriteRenderer _spriteRenderer;
        private List<GameObject> _enterObjectList;
        private bool _isPlaying;
        private Vector2 _originPos;
        private Vector2 _previousPosition;

        private void Start()
        {
            _splineContainer = GetComponent<SplineContainer>();
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            _movementInteractive = GetComponentInChildren<MovementInteractive>();
            _enterObjectList = new List<GameObject>();
            _materialPropertyBlock = new MaterialPropertyBlock();

            _movementInteractive.MovementAddEventHandler += AddObject;
            _movementInteractive.MovementRemoveEventHandler += RemoveObject;

            var pos = Platform.transform.position;
            _originPos = pos;
            _previousPosition = pos;

            if (_moveType == MoveType.Loop)
            {
                _isPlaying = true;
                DOTween.To(() => LerpAmount, x => LerpAmount = x, 1, _duration).SetLoops(-1, LoopType.Yoyo)
                    .SetEase(_ease);
            }
        }

        private void FixedUpdate()
        {
            if (_isPlaying)
            {
                Platform.transform.position = _splineContainer.EvaluatePosition(LerpAmount);
                if (_enterObjectList.Count > 0)
                {
                    foreach (var obj in _enterObjectList)
                    {
                        obj.transform.position += Platform.transform.position - (Vector3)_previousPosition;
                    }
                }

                _previousPosition = Platform.transform.position;
            }
        }

        private void Play()
        {
            if (!_isPlaying)
            {
                if (_moveType == MoveType.Once)
                {
                    DOTween.To(() => LerpAmount, x => LerpAmount = x, 1, _duration).SetEase(_ease)
                        .OnComplete(() => Stop());
                }
                else if (_moveType == MoveType.PingPong)
                {
                    DOTween.To(() => LerpAmount, x => LerpAmount = x, 1, _duration).SetEase(_ease)
                        .OnComplete(() => DOTween.To(() => LerpAmount, x => LerpAmount = x, 0, _duration).SetEase(_ease)
                            .OnComplete(() => Stop()));
                }

                _previousPosition = Platform.transform.position;
                _isPlaying = true;
            }
        }

        private MaterialPropertyBlock _materialPropertyBlock;
        private static readonly int Lerp = Shader.PropertyToID("_Lerp");

        private void Stop()
        {
            if (_isPlaying)
            {
                DOTween.Kill(this);

                if (_isDestroy && _moveType == MoveType.Once)
                {
                    DOTween.To(x =>
                        {
                            _spriteRenderer.GetPropertyBlock(_materialPropertyBlock);
                            _materialPropertyBlock.SetFloat(Lerp, x);
                            _spriteRenderer.SetPropertyBlock(_materialPropertyBlock);
                        }, 1, 0, 1)
                        .SetEase(Ease.Linear)
                        .OnComplete(() =>
                        {
                            ClearEnterObjects();
                            
                            DOTween.To(x =>
                                {
                                    _spriteRenderer.GetPropertyBlock(_materialPropertyBlock);
                                    _materialPropertyBlock.SetFloat(Lerp, x);
                                    _spriteRenderer.SetPropertyBlock(_materialPropertyBlock);
                                }, 0, 1, 1)
                                .SetEase(Ease.Linear)
                                .OnComplete(() =>
                                {
                                    _isPlaying = false;
                                });
                        });
                    return;
                }

                _isPlaying = false;
            }
        }

        private void ClearEnterObjects()
        {
            DOTween.Kill(this);
            bool isUmbrella = false;
            bool isPlayer = false;
            for (int i = 0; i < _enterObjectList.Count; i++)
            {
                if (_enterObjectList[i].TryGetComponent<StickUmbrella>(out var stickUmbrella))
                {
                    isUmbrella = true;
                }

                if (_enterObjectList[i].TryGetComponent<PlayerMovement>(out var playerMovement))
                {
                    isPlayer = true;
                }
            }

            if (isUmbrella)
                Umbrella.Instance.ReturnStick = true;

            if (isPlayer)
                PlayerMovement.Instance.PlayerAddForce(Vector2.zero);

            _enterObjectList.Clear();

            LerpAmount = 0;
            Platform.transform.position = _originPos;
        }

        private void OnDestroy()
        {
            DOTween.Kill(this);
        }

        private void OnDisable()
        {
            DOTween.Kill(this);
        }

        public void AddObject(GameObject obj)
        {
            if (!_enterObjectList.Contains(obj))
                _enterObjectList.Add(obj);

            if (_moveType != MoveType.Loop)
                Play();
        }

        public void RemoveObject(GameObject obj)
        {
            _enterObjectList.Remove(obj);
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (!EditorApplication.isPlaying)
            {
                Platform.transform.position = GetComponent<SplineContainer>().EvaluatePosition(LerpAmount);
            }
        }
#endif
    }
}