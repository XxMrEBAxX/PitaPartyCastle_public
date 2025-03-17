using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace UB
{
    public class ObjectPoolManager : Singleton<ObjectPoolManager>
    {
        [System.Serializable]
        private class ObjectInfo
        {
            // 오브젝트 이름
            public string objectName;
            // 오브젝트 풀에서 관리할 오브젝트
            public GameObject prefabs;
            // 몇개를 미리 생성 해놓을건지
            public int count;
        }

        // 오브젝트풀 매니저 준비 완료표시
        public bool IsReady { get; private set; }

        [SerializeField]
        private ObjectInfo[] objectInfos = null;

        // 생성할 오브젝트의 key값지정을 위한 변수
        private string objectName;

        // 오브젝트풀들을 관리할 딕셔너리
        private Dictionary<string, IObjectPool<GameObject>> objectPoolDic = new Dictionary<string, IObjectPool<GameObject>>();

        // 오브젝트풀에서 오브젝트를 새로 생성할때 사용할 딕셔너리
        private Dictionary<string, GameObject> goDic = new Dictionary<string, GameObject>();
        // 오브젝트풀에서 가지고 있는 갯수 딕셔너리
        private Dictionary<string, int> objectPoolCountDic = new Dictionary<string, int>();

        protected override void Awake()
        {
            base.Awake();
            Init();
        }


        private void Init()
        {
            IsReady = false;

            for (int idx = 0; idx < objectInfos.Length; idx++)
            {
                IObjectPool<GameObject> pool = new ObjectPool<GameObject>(CreatePooledItem, OnTakeFromPool, OnReturnedToPool,
                OnDestroyPoolObject, true, objectInfos[idx].count, objectInfos[idx].count);

                if (goDic.ContainsKey(objectInfos[idx].objectName))
                {
                    Debug.LogFormat("{0} 이미 등록된 오브젝트입니다.", objectInfos[idx].objectName);
                    return;
                }

                goDic.Add(objectInfos[idx].objectName, objectInfos[idx].prefabs);
                objectPoolDic.Add(objectInfos[idx].objectName, pool);
                objectPoolCountDic.Add(objectInfos[idx].objectName, objectInfos[idx].count);

                // 미리 오브젝트 생성 해놓기
                for (int i = 0; i < objectInfos[idx].count; i++)
                {
                    objectName = objectInfos[idx].objectName;
                    PoolAble poolAbleGo = CreatePooledItem().GetComponent<PoolAble>();
                    poolAbleGo.Pool.Release(poolAbleGo.gameObject);
                }
            }
            IsReady = true;
        }
        public void AddPool(GameObject poolObject, string poolName, int count)
        {
            if(!poolObject.GetComponent<PoolAble>())
                Debug.LogAssertion(poolName + "의 PoolAble 컴포넌트가 없습니다.");

            // 이미 존재하는 풀오브젝트이면
            if (goDic.ContainsKey(poolName))
            {
                int originCount = objectPoolCountDic[poolName];
                objectPoolDic[poolName].Clear();

                IObjectPool<GameObject> pool = new ObjectPool<GameObject>(CreatePooledItem, OnTakeFromPool, OnReturnedToPool,
                OnDestroyPoolObject, true, originCount + count, originCount + count);

                goDic.Remove(poolName);
                objectPoolDic.Remove(poolName);
                objectPoolCountDic.Remove(poolName);

                goDic.Add(poolName, poolObject);
                objectPoolDic.Add(poolName, pool);
                objectPoolCountDic.Add(poolName, originCount + count);
            }
            else
            {
                IObjectPool<GameObject> pool = new ObjectPool<GameObject>(CreatePooledItem, OnTakeFromPool, OnReturnedToPool,
                OnDestroyPoolObject, true, count, count);

                goDic.Add(poolName, poolObject);
                objectPoolDic.Add(poolName, pool);
                objectPoolCountDic.Add(poolName, count);
            }
            // 미리 오브젝트 생성 해놓기
            for (int i = 0; i < objectPoolCountDic[poolName]; i++)
            {
                objectName = poolName;
                PoolAble poolAbleGo = CreatePooledItem().GetComponent<PoolAble>();
                poolAbleGo.Pool.Release(poolAbleGo.gameObject);
            }
        }

        // 생성
        private GameObject CreatePooledItem()
        {
            GameObject poolGo = Instantiate(goDic[objectName]);
            poolGo.GetComponent<PoolAble>().Pool = objectPoolDic[objectName];
            poolGo.transform.SetParent(this.transform);
            return poolGo;
        }

        // 대여
        private void OnTakeFromPool(GameObject poolGo)
        {
            poolGo.SetActive(true);
        }

        // 반환
        private void OnReturnedToPool(GameObject poolGo)
        {
            poolGo.SetActive(false);
        }

        // 삭제
        private void OnDestroyPoolObject(GameObject poolGo)
        {
            Destroy(poolGo);
        }

        public GameObject LetsGo(string goName)
        {
            objectName = goName;

            if (goDic.ContainsKey(goName) == false)
            {
                Debug.LogErrorFormat("{0} 오브젝트풀에 등록되지 않은 오브젝트입니다.", goName);
                return null;
            }

            return objectPoolDic[goName].Get();
        }
    }
}