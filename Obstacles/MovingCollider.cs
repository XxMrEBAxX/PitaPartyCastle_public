using UnityEngine;
using UB.EVENT;

namespace UB
{
    public class MovingCollider : MonoBehaviour
    {
        private MovingPlatform _movingPlatform;
        private void Start()
        {
            _movingPlatform = GetComponentInParent<MovingPlatform>();
        }

        private void OnTriggerStay2D(Collider2D other) {
            if (other.gameObject.CompareTag("Foot"))
            {
                _movingPlatform.AddObject(Player.Instance.gameObject);
            }
        }

        private void OnTriggerExit2D(Collider2D other) {
            if (other.gameObject.CompareTag("Foot"))
            {
                if(PlayerMovement.Instance.CurrentMovementStateEnum != PlayerMovementState.LEDGE)
                    _movingPlatform.RemoveObject(Player.Instance.gameObject);
            }
        }
    }
}