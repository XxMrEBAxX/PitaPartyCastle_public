using UB.EVENT;
using UnityEngine;

namespace UB
{
    [RequireComponent(typeof(UmbrellaInteractive))]
    public abstract class AbleStick : MonoBehaviour
    {
        protected UmbrellaInteractive _umbrellaInteractive;
        protected virtual void Start()
        {
            _umbrellaInteractive = GetComponent<UmbrellaInteractive>();
        }
    }
}
