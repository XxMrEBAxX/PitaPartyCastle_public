using System.Collections.Generic;
using UnityEngine;

namespace UB
{
    public class LedgeAbleCeiling : MonoBehaviour
    {
        public Transform LedgeTransform;

        private PlayerMovement _playerMovement;
        private BoxCollider2D _boxCollider;

        private void Start()
        {
            _playerMovement = PlayerMovement.Instance;
            _boxCollider = GetComponent<BoxCollider2D>();
        }

        private void Update()
        {

        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player") && _playerMovement.CurrentMovementStateEnum == PlayerMovementState.FALL && _playerMovement.GetVelocity().y < 0)
            {
                _playerMovement.LedgePosition((Vector2)LedgeTransform.position, gameObject.tag);
                _boxCollider.enabled = false;
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                _boxCollider.enabled = true;
            }
        }
    }
}
