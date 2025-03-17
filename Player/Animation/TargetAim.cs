using System.Collections.Generic;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UB
{
    public class TargetAim : MonoBehaviour
    {
        [SpineBone(dataField: "skeletonAnimation")]
        public string boneName;
        public AnimationReferenceAsset aim;
        private Bone _bone;
        private SkeletonAnimation _skeletonAnimation;
        private Camera _cam;
        private Umbrella _umbrella;
        private List<Spine.Slot> _umbrellaSpriteSlot;
        private Vector3 _boneSkeletonSpacePoint;
        private int _umbrellaAlpha = 1;
        public bool SetUmbrellaAlpha { get; set; } = true;

        private void Start()
        {
            _skeletonAnimation = GetComponent<SkeletonAnimation>();
            _umbrella = Umbrella.Instance;
            _bone = _skeletonAnimation.Skeleton.FindBone(boneName);
            //_umbrellaBone = _skeletonAnimation.Skeleton.FindBone("umb");
            _cam = Camera.main;

            _umbrellaSpriteSlot = new List<Slot>
            {
                _skeletonAnimation.skeleton.FindSlot("stick"),
                _skeletonAnimation.skeleton.FindSlot("umb_fold"),
                _skeletonAnimation.skeleton.FindSlot("stick_point")
            };

            _skeletonAnimation.UpdateLocal += NewMethod;

            //_skeletonAnimation.AnimationState.Data.DefaultMix = 0;
        }

        private void NewMethod(ISkeletonAnimation animated)
        {
            if (TimeManager.Instance.TimeScale != 0)
            {
                if (_umbrella.CurrentUmbrellaStateEnum == UmbrellaState.RETURN)
                {
                    _boneSkeletonSpacePoint = transform.InverseTransformPoint(_umbrella.transform.position);
                }
                if (_umbrella.CurrentUmbrellaStateEnum == UmbrellaState.ATTACK)
                {
                    Vector3 mousePos = Mouse.current.position.ReadValue();
                    mousePos.z = -_cam.gameObject.transform.position.z;
                    Vector2 mouseWorldPos = _cam.ScreenToWorldPoint(mousePos);
                    _boneSkeletonSpacePoint = transform.InverseTransformPoint(mouseWorldPos);
                }
            }

            _bone.SetPositionSkeletonSpace(_boneSkeletonSpacePoint);
            
            if (!ReferenceEquals(_skeletonAnimation.AnimationState.GetCurrent(1), null))
            //if (_umbrella.CurrentUmbrellaStateEnum != UmbrellaState.FOLD)
            {
                // _umbrellaBone.Rotation = 90;
                // _umbrellaBone.SetLocalPosition(Vector2.zero);
            }

            bool isNotChangeAlpha = _umbrella.CurrentUmbrellaStateEnum == UmbrellaState.THROW || _umbrella.CurrentUmbrellaStateEnum == UmbrellaState.FOLD || _umbrella.CurrentUmbrellaStateEnum == UmbrellaState.STICK;
            // 우산 알파값 조절
            if ((SetUmbrellaAlpha != (_umbrellaAlpha > 0 ? true : false) || _umbrellaSpriteSlot[1].A != _umbrellaAlpha) && isNotChangeAlpha)
            {
                if (SetUmbrellaAlpha)
                {
                    foreach (var alpha in _umbrellaSpriteSlot)
                    {
                        alpha.A = 1;
                        _umbrellaAlpha = 1;
                    }
                }
                else
                {
                    foreach (var alpha in _umbrellaSpriteSlot)
                    {
                        alpha.A = 0;
                        _umbrellaAlpha = 0;
                    }
                }
            }

        }
    }
}
