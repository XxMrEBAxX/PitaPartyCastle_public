using UnityEngine;
using Spine.Unity;

namespace UB
{
    public class JumpObstacle : MonoBehaviour
    {
        [Tooltip("점프 강도")]
        [SerializeField] private float JumpForce = 50;
        private float _interactiveTime;
        
        private SkeletonAnimation _ani;
        [SpineAnimation] public string InteractAni;

        private void Start()
        {
            _ani = gameObject.transform.GetChild(0).GetComponent<SkeletonAnimation>();
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (other.collider.tag == "Player")
            {
                if (Time.time - _interactiveTime < 0.2f || !PlayerMovement.Instance.CanMoveState())
                    return;

                _ani.AnimationState.SetAnimation(0, InteractAni, false);

                float _force = JumpForce;
                _interactiveTime = Time.time;
                PlayerMovement.Instance.SettingObstacleMovementStateInit();
                PlayerMovement.Instance.SetVelocity(PlayerMovement.Instance.GetVelocity().x, 0);
                PlayerMovement.Instance.DenyJumpTime = 0.1f;
                PlayerMovement.Instance.PlayerAddForce(Vector2.right * 0 + Vector2.up * _force, false);
            }
        }
    }

}