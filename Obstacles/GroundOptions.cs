using MyBox;
using UnityEngine;

namespace UB
{
    public class GroundOptions : MonoBehaviour
    {
        [OverrideLabel("Bounce 여부")]
        public bool AbleBounce = false;
        [OverrideLabel("Stick 여부")]
        public bool AbleStick = true;
        [OverrideLabel("벽 타기 여부")]
        public bool AbleSliding = false;
        [OverrideLabel("매달리기 여부")]
        public bool AbleLedge = true;
    }
}
