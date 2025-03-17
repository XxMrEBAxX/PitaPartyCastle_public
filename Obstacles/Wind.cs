using UnityEngine;

namespace UB
{
    public class Wind : MonoBehaviour
    {
        private void OnTriggerStay2D(Collider2D other)
        {
            if (other.tag == "Player")
            {
                PlayerMovement.Instance.IsWind = true;
                if (Umbrella.Instance.IsHoldGlidingButton && Umbrella.Instance.CurrentUmbrellaStateEnum == UmbrellaState.GLIDING)
                {
                    PlayerMovement.Instance.IsObstacle = true;
                }
                else
                {
                    PlayerMovement.Instance.IsObstacle = false;
                }
            }
        }
        private void OnTriggerExit2D(Collider2D other)
        {
            if (PlayerMovement.Instance.IsObstacle)
            {
                PlayerMovement.Instance.IsObstacle = false;

            }
            if (other.tag == "Player")
            {
                PlayerMovement.Instance.IsWind = false;
            }
        }
    }

}