using UnityEngine;

namespace UB.EVENT
{
    public delegate void UmbrellaInteractiveEventHandler();
    public class UmbrellaInteractive : MonoBehaviour
    {
        public event UmbrellaInteractiveEventHandler UmbrellaEventHandler;
        public event UmbrellaInteractiveEventHandler UmbrellaBatRushEventHandler;
        public event UmbrellaInteractiveEventHandler UmbrellaReturnEventHandler;

        public void Call()
        {
            UmbrellaEventHandler?.Invoke();
        }

        public void BatRushCall()
        {
            UmbrellaBatRushEventHandler?.Invoke();
        }

        public void ReturnCall()
        {
            UmbrellaReturnEventHandler?.Invoke();
        }
    }
}