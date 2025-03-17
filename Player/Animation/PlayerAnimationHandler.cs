using System.Collections.Generic;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.Animations;

namespace UB.Animation
{
    public class PlayerAnimationHandler : MonoBehaviour
    {
        public SkeletonAnimation[] AnimationData;
        public List<StateNameToAnimationReference> StatesAndAnimations = new List<StateNameToAnimationReference>();
        public List<AnimationTransition> Transitions = new List<AnimationTransition>(); // Alternately, an AnimationPair-Animation Dictionary (commented out) can be used for more efficient lookups.

        [System.Serializable]
        public class StateNameToAnimationReference
        {
            public string StateName;
            public AnimationReferenceAsset Animation;
        }

        [System.Serializable]
        public class AnimationTransition
        {
            public AnimationReferenceAsset From;
            public AnimationReferenceAsset To;
            public AnimationReferenceAsset Transition;
        }

        public Spine.Animation TargetAnimation { get; private set; }
        public float LastOnDenyAnimationChangeTime { get; set; } = 0;

        private void Awake()
        {
            // Initialize AnimationReferenceAssets
            foreach (StateNameToAnimationReference entry in StatesAndAnimations)
            {
                entry.Animation.Initialize();
            }
            foreach (AnimationTransition entry in Transitions)
            {
                entry.From.Initialize();
                entry.To.Initialize();
                entry.Transition.Initialize();
            }

            // Build Dictionary
            //foreach (AnimationTransition entry in transitions) {
            //	transitionDictionary.Add(new AnimationStateData.AnimationPair(entry.from.Animation, entry.to.Animation), entry.transition.Animation);
            //}
        }

        private void Update()
        {
            LastOnDenyAnimationChangeTime = Mathf.Clamp(LastOnDenyAnimationChangeTime - Time.deltaTime, 0, float.MaxValue);
        }

        /// <summary>Plays an animation based on the state name.</summary>
        public void PlayAnimationForState(string stateShortName, int layerIndex)
        {
            PlayAnimationForState(StringToHash(stateShortName), layerIndex);
        }

        /// <summary>Plays an animation based on the hash of the state name.</summary>
        public void PlayAnimationForState(int shortNameHash, int layerIndex)
        {
            Spine.Animation foundAnimation = GetAnimationForState(shortNameHash);
            if (foundAnimation == null)
                return;

            PlayNewAnimation(foundAnimation, layerIndex);
        }

        /// <summary>Gets a Spine Animation based on the state name.</summary>
        public Spine.Animation GetAnimationForState(string stateShortName)
        {
            Spine.Animation temp = GetAnimationForState(StringToHash(stateShortName));
            if(temp == null)
                Debug.LogError("애니메이션을 찾을 수 없습니다! : " + stateShortName);
            return temp;
        }

        /// <summary>Gets a Spine Animation based on the hash of the state name.</summary>
        public Spine.Animation GetAnimationForState(int shortNameHash)
        {
            StateNameToAnimationReference foundState = StatesAndAnimations.Find(entry => StringToHash(entry.StateName) == shortNameHash);
            return (foundState == null) ? null : foundState.Animation;
        }

        /// <summary>Play an animation. If a transition animation is defined, the transition is played before the target animation being passed.</summary>
        public void PlayNewAnimation(Spine.Animation target, int layerIndex)
        {                
            Spine.Animation transition = null;
            Spine.Animation current = null;

            current = GetCurrentAnimation(layerIndex);
            if (current != null)
                transition = TryGetTransition(current, target);

            if (transition != null)
            {
                foreach (var anim in AnimationData)
                {
                    anim.AnimationState.SetAnimation(layerIndex, transition, false);
                    anim.AnimationState.AddAnimation(layerIndex, target, true, 0f);
                }

            }
            else
            {
                foreach (var anim in AnimationData)
                {
                    anim.AnimationState.SetAnimation(layerIndex, target, true);
                }
            }

            
            if(layerIndex == 0)
                this.TargetAnimation = target;
        }

        /// <summary>Play a non-looping animation once then continue playing the state animation.</summary>
        public void PlayOneShot(Spine.Animation oneShot, int layerIndex, bool notNextAnimation = false)
        {
            foreach (var anim in AnimationData)
            {
                Spine.AnimationState state = anim.AnimationState;
                state.SetAnimation(layerIndex, oneShot, false);

                if(notNextAnimation)
                    break;

                Spine.Animation transition = TryGetTransition(oneShot, TargetAnimation);
                if (transition != null)
                    state.AddAnimation(layerIndex, transition, false, 0f);

                if(layerIndex == 0)
                    state.AddAnimation(0, this.TargetAnimation, true, oneShot.Duration);
                else
                    state.AddEmptyAnimation(layerIndex, 0, 0);
            }
        }
        public void PlayEmptyLayer(int layerIndex)
        {
            foreach (var anim in AnimationData)
            {
                Spine.AnimationState state = anim.AnimationState;
                state.SetEmptyAnimation(layerIndex, 0);
            }
        }

        private Spine.Animation TryGetTransition(Spine.Animation from, Spine.Animation to)
        {
            foreach (AnimationTransition transition in Transitions)
            {
                if (transition.From.Animation == from && transition.To.Animation == to)
                {
                    return transition.Transition.Animation;
                }
            }
            return null;

            //Spine.Animation foundTransition = null;
            //transitionDictionary.TryGetValue(new AnimationStateData.AnimationPair(from, to), out foundTransition);
            //return foundTransition;
        }

        private Spine.Animation GetCurrentAnimation(int layerIndex)
        {
            TrackEntry currentTrackEntry = AnimationData[0].AnimationState.GetCurrent(layerIndex);
            return (currentTrackEntry != null) ? currentTrackEntry.Animation : null;
        }

        private int StringToHash(string s)
        {
            return Animator.StringToHash(s);
        }

        public float GetCurAnimationDuration()
        {
            return TargetAnimation.Duration;
        }
    }

}
