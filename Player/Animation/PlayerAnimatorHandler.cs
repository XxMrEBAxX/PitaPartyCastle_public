using UnityEngine;

namespace UB.Animation
{
    public class PlayerAnimatorHandler : StateMachineBehaviour
    {
        private PlayerAnimationHandler _animationHandler;
		private bool _isInitialized;

		public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{	
			if (!_isInitialized)
			{
				_animationHandler = animator.GetComponent<PlayerAnimationHandler>();
				_isInitialized = true;
			}

			if(stateInfo.IsName("NONE"))
			{
				if(layerIndex == 0)
					return;
				_animationHandler.PlayEmptyLayer(layerIndex);
				return;
			}
			
			_animationHandler.PlayAnimationForState(stateInfo.shortNameHash, layerIndex);
		}
    }
}
