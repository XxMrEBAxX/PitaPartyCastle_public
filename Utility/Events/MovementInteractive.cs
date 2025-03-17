using UnityEngine;

namespace UB.EVENT
{
    public delegate void MovementInteractiveEventHandler(GameObject obj);
    public delegate void MovementInteractiveExecuteEventHandler();
    public class MovementInteractive : MonoBehaviour
    {
        public event MovementInteractiveExecuteEventHandler MovementExecuteEventHandler;
        public event MovementInteractiveEventHandler MovementAddEventHandler;
        public event MovementInteractiveEventHandler MovementRemoveEventHandler;

        public void Add(GameObject obj)
        {
            MovementAddEventHandler?.Invoke(obj);
        }

        public void Remove(GameObject obj)
        {
            MovementRemoveEventHandler?.Invoke(obj);
        }

        public void Execute()
        {
            MovementExecuteEventHandler?.Invoke();
        }
    }
}
