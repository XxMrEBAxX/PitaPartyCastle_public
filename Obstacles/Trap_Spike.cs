using System.Collections;
using MyBox;
using UnityEngine;

namespace UB
{
    public class Trap_Spike : MonoBehaviour
    {
        public Transform MovePosition;

        [Header("설정"), Space(10)]

        [OverrideLabel("데미지")]
        [SerializeField] private int _damage = 1;

        [OverrideLabel("데미지 딜레이")]
        [SerializeField] private float _delay = 0.5f;

        [OverrideLabel("히트 시 캐릭터 이동 여부")]
        [SerializeField] private bool _onHitMovePos = false;

        private float _curTime;

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (_curTime + _delay > TimeManager.Instance.GetTime() || coroutine != null)
                return;

            if (other.transform.CompareTag("Player"))
            {
                Player.Instance.StopInvincible();
                Player.Instance.SubtractHP(_damage);

                if (_onHitMovePos && Player.Instance.CurPlayerState == Player.PlayerState.ALIVE)
                {
                    coroutine = StartCoroutine(DelayMovePos());
                }

                _curTime = TimeManager.Instance.GetTime();
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if(other.tag != "Umbrella")
                return;
                
            if (other.transform.parent.TryGetComponent<StickUmbrella>(out var stickUmbrella))
            {
                if (Umbrella.Instance.CurrentUmbrellaStateEnum == UmbrellaState.STICK)
                    Umbrella.Instance.ReturnStick = true;
            }
        }

        Coroutine coroutine;

        private void OnCollisionStay2D(Collision2D other)
        {
            if (_curTime + _delay > TimeManager.Instance.GetTime() || coroutine != null)
                return;

            if (other.transform.CompareTag("Player"))
            {
                Player.Instance.StopInvincible();
                Player.Instance.SubtractHP(_damage);

                if (_onHitMovePos && Player.Instance.CurPlayerState == Player.PlayerState.ALIVE)
                {
                    coroutine = StartCoroutine(DelayMovePos());
                }

                _curTime = TimeManager.Instance.GetTime();
            }
        }

        private IEnumerator DelayMovePos()
        {
            yield return new WaitForSecondsRealtime(CameraManager.Instance.DamagePlayTime);
            Umbrella.Instance.ForceReturnUmbrella();
            PlayerMovement.Instance.SetPlayerPosition(MovePosition.position);
            coroutine = null;
        }
    }

}