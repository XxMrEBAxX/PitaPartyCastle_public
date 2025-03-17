using UnityEngine;

namespace UB
{
    public class AllManager : Singleton<AllManager>
    {
        public SoundManager soundManager;
        private GameObject soundManagerPrefab;
        public string sMName = "SoundManager";
        // Start is called before the first frame update

        private void OnValidate()
        {
            //FIXME - 매니저 프리팹 관리 필요
            soundManagerPrefab = Resources.Load<GameObject>("Prefabs/" + sMName);

            //SoundManager
            if (soundManager == null)
            {
                var find = GameObject.Find(sMName);
                if (find != null)
                {
                    soundManager = find.GetComponent<SoundManager>();
                    find.transform.SetParent(transform);
                    Debug.Log(sMName + "를 찾았습니다!");
                }
                else
                {
                    soundManager = Instantiate(soundManagerPrefab, transform.position, transform.rotation, transform).GetComponent<SoundManager>();
                    soundManager.gameObject.name = sMName;
                    Debug.Log(sMName + "를 생성하였습니다!");
                }
            }
        }

        protected override void Awake()
        {
            base.Awake();
            if(Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            else
            {
                DontDestroyOnLoad(gameObject);
            }
        }
    }

}