using System.Collections.Generic;
using System.Collections;
using UB.EVENT;
using UnityEngine;
using Spine.Unity;

namespace UB
{
    public class CountdownPlatform : MonoBehaviour
    {
        [SerializeField] private GameObject countdownCollider;
        
        [Header("Countdown Times")]
        [Tooltip("밟은 플랫폼이 사라지는 데 걸리는 시간 (초)")]
        [SerializeField] private float disappearCountdown;
        [Tooltip("사라진 플랫폼이 다시 나타나는 데 걸리는 시간 (초)")]
        [SerializeField] private float appearCountdown;

        private SkeletonAnimation _animator;
        [SpineAnimation] public string pop;

        private bool _isStepped;
        private MovementInteractive _movementInteractive;
        private List<GameObject> _enterObjectList;

        private void Start()
        {
            _enterObjectList = new List<GameObject>();
            _animator = transform.GetChild(0).GetChild(0).GetComponentInChildren<SkeletonAnimation>();
            _movementInteractive = GetComponentInChildren<MovementInteractive>();
            _movementInteractive.MovementExecuteEventHandler += Step;
            _movementInteractive.MovementAddEventHandler += AddObject;
            _movementInteractive.MovementRemoveEventHandler += RemoveObject;
            _isStepped = false;
        }

        public void Step()
        {
            if (_isStepped == false)
            {
                _isStepped = true;
                StartCoroutine(DisappearRoutine());
            }
        }

        public void AddObject(GameObject obj)
        {
            if (!_enterObjectList.Contains(obj))
                _enterObjectList.Add(obj);
        }

        public void RemoveObject(GameObject obj)
        {
            _enterObjectList.Remove(obj);
        }

        private void ClearEnterObjects()
        {
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
        }

        private IEnumerator DisappearRoutine()
        {
            _animator.AnimationState.SetAnimation(0, pop, false);
            yield return new WaitForSeconds(disappearCountdown);
            countdownCollider.SetActive(false);
            ClearEnterObjects();
            yield return new WaitForSeconds(appearCountdown);
            countdownCollider.SetActive(true);
            _animator.AnimationState.SetEmptyAnimation(0, 0);
            _isStepped = false;
        }
    }
}