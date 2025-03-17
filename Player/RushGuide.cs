using System.Data;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UB
{
    public class RushGuide : Singleton<RushGuide>
    {
        [SerializeField] private float distance = 2.5f;

        private Player _player;
        private SpriteRenderer _sprite;
        private bool _active;
        
        public Vector2 Direction { get; private set; }
        public bool Active
        {
            get { return _active; }
            set
            {
                _sprite.enabled = value;
                _active = value;
            }
        }

        protected override void Awake()
        {
            base.Awake();
            _sprite = GetComponentInChildren<SpriteRenderer>();
        }
        private void Start()
        {
            _player = Player.Instance;
        }

        private void Update()
        {
            if (_active)
            {
                Vector3 mousePos = Mouse.current.position.ReadValue();
                mousePos.z = -Camera.main.gameObject.transform.position.z;
                Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(mousePos);
                Vector2 direction = mouseWorldPos - (Vector2)_player.transform.position;
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                Direction = direction.normalized;

                _sprite.transform.localPosition = Direction * distance;
                _sprite.transform.rotation = Quaternion.Euler(0, 0, angle);

                if (_player.transform.localScale.x < 0)
                    transform.localScale = new Vector3(-1, 1, 1);
                else
                    transform.localScale = Vector3.one;
            }
        }
    }
}
