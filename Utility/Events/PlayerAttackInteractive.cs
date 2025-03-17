using UnityEngine;

namespace UB.EVENT
{
    public delegate void PlayerAttackDamageEventHandler(int damage, Vector2 dir);
    public delegate void PlayerAttackEventHandler();
    public class PlayerAttackInteractive : MonoBehaviour
    {
        public event PlayerAttackEventHandler AttackEventHandler;
        public event PlayerAttackEventHandler AttackThrowEventHandler;
        public event PlayerAttackEventHandler AttackReturnEventHandler;
        public event PlayerAttackDamageEventHandler AttackDamageEventHandler;

        public void Damage(int damage, Vector2 dir)
        {
            AttackDamageEventHandler?.Invoke(damage, dir);
        }

        public void Call()
        {
            AttackEventHandler?.Invoke();
        }

        public void ThrowCall()
        {
            AttackThrowEventHandler?.Invoke();
        }

        public void ReturnCall()
        {
            AttackReturnEventHandler?.Invoke();
        }
    }
}
