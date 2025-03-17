using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using MyBox;
using Spine.Unity;
using UB.EFFECT;
using UB.EVENT;
using UnityEngine;

namespace UB
{
    public sealed class AbleStickObstacle : AbleStick
    {
        [OverrideLabel("재사용 대기 시간")]
        [SerializeField] private float duration = 1;

        private CircleCollider2D _circleCollider2D;
        private SkeletonAnimation _skeletonAnimation;
        private DissolveFX _dissolveFX;
        //public UnityEngine.Events.UnityEvent OnClick;
        protected override void Start()
        {
            base.Start();
            _umbrellaInteractive = GetComponent<UmbrellaInteractive>();
            _circleCollider2D = GetComponent<CircleCollider2D>();
            _skeletonAnimation = GetComponentInChildren<SkeletonAnimation>();
            _dissolveFX = GetComponentInChildren<DissolveFX>();
            _umbrellaInteractive.UmbrellaBatRushEventHandler += DisableObject;

            _skeletonAnimation.AnimationState.SetAnimation(0, "UBstuck2@BackFlip", false);
            _umbrellaInteractive.UmbrellaEventHandler
            += () =>
            {
                _skeletonAnimation.AnimationState.SetAnimation(0, "UBstuck2@Flip", false);
            };

            _umbrellaInteractive.UmbrellaReturnEventHandler
            += () =>
            {
                _skeletonAnimation.AnimationState.SetAnimation(0, "UBstuck2@BackFlip", false);
            };
        }
        public void DisableObject()
        {
            StartCoroutine(DisableObjectRoutine(duration));
            _circleCollider2D.enabled = false;
            _dissolveFX.PlayHit(false);
        }

        IEnumerator<WaitForSeconds> DisableObjectRoutine(float duration)
        {
            yield return new WaitForSeconds(duration * 0.5f);
            _dissolveFX.PlayHitReverse(false);
            yield return new WaitForSeconds(duration * 0.5f);
            _circleCollider2D.enabled = true;
        }
    }
}