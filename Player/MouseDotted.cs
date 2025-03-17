using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UB
{
    public class MouseDotted : Singleton<MouseDotted>
    {
        private float _angle;
        private Umbrella _umbrella;
        private bool _active;
        private LineRenderer _lineRenderer;
        private LayerMask _NotAimLayer;
        private LayerMask _AimLayer;
        private Camera _camera;

        public Vector2 Direction { get; private set; } = Vector2.zero;
        public bool Active
        {
            get { return _active; }
            set
            {
                if(_isNotChangeActive)
                    return;
                _lineRenderer.enabled = value;
                _active = value;
            }
        }
        private bool _isNotChangeActive;

        public void SetActiveAndNotChange(bool value, bool isNotChange)
        {
            _isNotChangeActive = isNotChange;
            _lineRenderer.enabled = value;
            _active = value;
        }

        protected override void Awake()
        {
            base.Awake();
            _lineRenderer = GetComponent<LineRenderer>();
        }

        private void Start()
        {
            //FIXME - AimLayer 변수로 뺴기
            _AimLayer = LayerMask.GetMask(LayerEnum.AbleStick.ToString()) + LayerMask.GetMask(LayerEnum.Enemy.ToString());
            _umbrella = Umbrella.Instance;
            _camera = CameraManager.Instance.GetComponent<Camera>();

            _NotAimLayer = Umbrella.Instance.AbleStickLayer.value ^ _AimLayer;
            _angle = PlayerMovement.Instance.PlayerDataInstance.UmbrellaAngle;
        }

        private void Update()
        {
            AimDotted().Forget();
        }

        private async UniTaskVoid AimDotted()
        {
            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);

            if (_active && TimeManager.Instance.TimeScale != 0)
            {
                Vector3 mousePos = Mouse.current.position.ReadValue();
                mousePos.z = -_camera.gameObject.transform.position.z;
                Vector2 mouseWorldPos = _camera.ScreenToWorldPoint(mousePos);
                Vector2 mouseDirection = (mouseWorldPos - (Vector2)_umbrella.transform.position).normalized;
                _lineRenderer.SetPosition(0, _umbrella.transform.position);

                // First Step : Check AbleStick in Umbrella Distance
                var ableSticks = Physics2D.OverlapCircleAll(_umbrella.transform.position, PlayerMovement.Instance.PlayerDataInstance.UmbrellaThrowMaxDistance, _AimLayer);
                // Second Step : Check Ground(Not Obstacle Able Stick) between player and AbleStick
                float closeDistance = float.MaxValue;
                Vector2 direction = Vector2.zero;
                foreach (var ableStick in ableSticks)
                {
                    Vector2 subtractPosition = ableStick.transform.position - _umbrella.transform.position;
                    float sqrDistance = subtractPosition.sqrMagnitude;
                    Vector2 normal = subtractPosition.normalized;
                    RaycastHit2D hitNotAim = Physics2D.Raycast(_umbrella.transform.position, normal, subtractPosition.magnitude, _NotAimLayer);
                    if (!hitNotAim && closeDistance > sqrDistance)
                    {
                        if (Mathf.Abs(Vector2.Angle(mouseDirection, normal)) <= _angle * 0.5f)
                        {
                            closeDistance = sqrDistance;
                            direction = normal;
                        }
                    }
                }
                if (direction == Vector2.zero)
                {
                    direction = (mouseWorldPos - (Vector2)_umbrella.transform.position).normalized;
                }
                RaycastHit2D hit = Physics2D.Raycast(_umbrella.transform.position, direction, PlayerMovement.Instance.PlayerDataInstance.UmbrellaThrowMaxDistance, Umbrella.Instance.AbleStickLayer);
                Direction = direction;
                if (!ReferenceEquals(hit.collider, null))
                    _lineRenderer.SetPosition(1, hit.point);
                else
                    _lineRenderer.SetPosition(1, (Vector2)_umbrella.transform.position + (direction * PlayerMovement.Instance.PlayerDataInstance.UmbrellaThrowMaxDistance));

            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!EditorApplication.isPlaying)
                return;

            Handles.color = new Color(1f, 0f, 0f, 0.05f);
            Vector2 pos = _umbrella.transform.position;
            Handles.DrawSolidArc(pos, Vector3.forward, Direction, _angle * 0.5f, PlayerMovement.Instance.PlayerDataInstance.UmbrellaThrowMaxDistance);
            Handles.DrawSolidArc(pos, Vector3.forward, Direction, -_angle * 0.5f, PlayerMovement.Instance.PlayerDataInstance.UmbrellaThrowMaxDistance);
        }
#endif
    }
}
