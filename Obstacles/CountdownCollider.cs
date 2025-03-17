using UB.EVENT;
using UnityEngine;

namespace UB
{
    [RequireComponent(typeof(MovementInteractive))]
    public class CountdownCollider : MonoBehaviour
    {
        private CountdownPlatform _countdownPlatform;

        private void Start()
        {
            _countdownPlatform = GetComponentInParent<CountdownPlatform>();
        }
        
        private void OnTriggerStay2D(Collider2D other) {
            if (other.gameObject.CompareTag("Foot"))
            {
                _countdownPlatform.AddObject(Player.Instance.gameObject);
                _countdownPlatform.Step();
            }
        }

        private void OnTriggerExit2D(Collider2D other) {
            if (other.gameObject.CompareTag("Foot"))
            {
                if(PlayerMovement.Instance.CurrentMovementStateEnum != PlayerMovementState.LEDGE)
                    _countdownPlatform.RemoveObject(Player.Instance.gameObject);
            }
        }
    }
}