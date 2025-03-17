using UnityEngine;
using UnityEngine.Pool;

namespace UB
{
    public class PoolAble : MonoBehaviour
    {
        public IObjectPool<GameObject> Pool { get; set; }
        
        public void ReleaseObject()
        {
            Pool.Release(gameObject);
            gameObject.transform.SetParent(ObjectPoolManager.Instance.transform);
        }
    }
}