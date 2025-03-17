using System.Collections.Generic;
using UnityEngine;

namespace UB
{
    public partial class LedgeDetection : Singleton<LedgeDetection>
    {
        [SerializeField] private float radius;
        [SerializeField] private LayerMask[] ableLedgeLayer;
        [Tooltip("땅일 경우 타일맵 분리 때문에 잡히는 경우 배제")]
        [SerializeField] private LayerMask[] notLedgeLayer;

        private PlayerMovement _playerM;
        private Rigidbody2D _playerRB;
        private Rigidbody2D _thisRB;
        private BoxCollider2D _thisBC;
        private List<bool> isSuccessList;

        private bool _canDetected;
        private const float PREDICT_CORRECTION = 0.1f;
        private Vector2 _previousPosition;
        private Vector2 _ledgeRBPos;
        private string _ledgeTag;
        private GameObject _ledgeObject;

        private void Start()
        {
            _playerM = PlayerMovement.Instance;
            _playerRB = PlayerMovement.Instance.GetComponent<Rigidbody2D>();
            _thisRB = GetComponent<Rigidbody2D>();
            _thisBC = GetComponent<BoxCollider2D>();
            isSuccessList = new List<bool>();

            if (ableLedgeLayer.Length == 0)
                Debug.LogError("ableLedgeLayer is null!!");
        }

        private void FixedUpdate()
        {
            if (_playerM.CanLedgeState())
            {
                CheckAbleLedge();
            }
            _previousPosition = _thisRB.position + Vector2.up * PREDICT_CORRECTION;
        }

        private void CheckAbleLedge()
        {
            isSuccessList.Clear();
            foreach (var ledge in ableLedgeLayer)
            {
                isSuccessList.Add(PredictColliders(ledge));
            }
            foreach(var success in isSuccessList)
            {
                if (success)
                {
                    _playerM.IsLedgeDetected = true;
                    _playerM.LedgeObject = _ledgeObject;
                    _playerM.LedgeTag = _ledgeTag;
                    _playerM.LedgeRBPos = _ledgeRBPos;
                    _playerM.StartLedge();
                    break;
                }
            }
        }

        private bool PredictColliders(LayerMask layerMask)
        {
            if (_playerRB.velocity.y > 0)
                return false;

            Vector2 centerPos = _thisRB.position;
            centerPos.y -= PREDICT_CORRECTION;
            RaycastHit2D hitGround = Physics2D.Linecast(_previousPosition, centerPos, layerMask);
            if (!hitGround)
                return false;
            else if (hitGround.collider.TryGetComponent<GroundOptions>(out var groundOptions))
                if (!groundOptions.AbleLedge)
                    return false;

            Vector2 offset = hitGround.point - _thisRB.position;
            Vector2 triggerPredictedPos = (Vector2)_thisBC.bounds.center + offset;

            _canDetected = !Physics2D.OverlapBox(triggerPredictedPos, _thisBC.size, 0, layerMask);
            // 땅일 경우 타일맵 분리 때문에 잡히는 경우 배제
            // if (layerMask == LayerMask.GetMask("Ground"))
            // {
            //     foreach (var layer in notLedgeLayer)
            //     {
            //         _canDetected &= !Physics2D.OverlapBox(triggerPredictedPos, _thisBC.size, 0, layer);
            //     }
            // }
            if (_canDetected)
            {
                var result = Physics2D.OverlapCircle(hitGround.point, radius, layerMask);
                _ledgeObject = result.gameObject;
                _ledgeTag = result.tag;
                _ledgeRBPos = _playerRB.position + offset;
                return result;
            }
            else
            {
                return false;
            }
        }

        public bool CheckAbleLedgeAfter()
        {
            bool success = false;
            isSuccessList.Clear();
            int i = 0;
            foreach (var ledge in ableLedgeLayer)
            {
                isSuccessList.Add(PredictColliders(ledge));
            }
            for (i = 0; i < isSuccessList.Count; i++)
            {
                if (isSuccessList[i])
                {
                    success = true;
                    break;
                }
            }
            if (i.Equals(isSuccessList.Count))
                success = false;

            return success;
        }

        public void SetForceLedgePos(Vector2 pos)
        {
            _previousPosition = pos;
            _thisRB.position = pos;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(transform.position, radius);
        }
#endif
    }
}