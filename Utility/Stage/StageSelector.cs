using System.Collections.Generic;
using MyBox;
using UB.EVENT;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Serialization;

namespace UB
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class StageSelector : MonoBehaviour
    {
        [Header("반드시 한 챕터 내에서 스테이지 이름이 겹치면 안됩니다.")]
        [OverrideLabel("스테이지 이름")]
        public string StageName;

        private BoxCollider2D _collider;
        [SerializeField] private Transform _spawnPoint;
        public Transform SpawnPoint => _spawnPoint;
        public List<StageInteractive> StageObjects { get; private set; } = new List<StageInteractive>();

        public bool IsClear;

        private void Awake()
        {
            _collider = GetComponent<BoxCollider2D>();
            if (_spawnPoint == null)
                Debug.LogError("스폰 포인트를 설정해주세요.");

            StageInteractive[] stageInteractives = FindObjectsOfType<StageInteractive>();
            foreach (StageInteractive stageInteractive in stageInteractives)
            {
                if (_collider.bounds.Contains(stageInteractive.transform.position))
                {
                    StageObjects.Add(stageInteractive);
                }
            }
        }

        public void ResetStage()
        {
            IsClear = false;
            foreach (StageInteractive stageInteractive in StageObjects)
            {
                stageInteractive.IsCleared = false;
            }

            InitializeStage();
        }
        
        public void DeadStage()
        {
            if(IsClear)
                return;
            foreach (StageInteractive stageInteractive in StageObjects)
            {
                stageInteractive.IsCleared = false;
            }
            InitializeStage();
        }

        public void InitializeStage()
        {
            Umbrella.Instance.ForceReturnUmbrella();
            
            StageManager.Instance.SaveStageData();
            StageManager.Instance.CurrentStageSelector = this;

            IsClear = StageManager.Instance.GetStageClear(StageName);
            foreach (StageInteractive stageInteractive in StageObjects)
            {
                stageInteractive.IsCleared = StageManager.Instance.GetStageObjectClear(stageInteractive.Guid);
                stageInteractive.Initialize(IsClear);
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if(Application.isPlaying)
                return;

            PrefabInstanceStatus status = PrefabUtility.GetPrefabInstanceStatus(this.gameObject);
            if (string.IsNullOrEmpty(StageName) && status != PrefabInstanceStatus.NotAPrefab)
            {
                Debug.LogError("스테이지 이름을 입력해주세요.");
            }
            GetComponent<BoxCollider2D>().isTrigger = true;
        }
#endif

        private void OnTriggerEnter2D(Collider2D other) 
        {
            bool isCurStage = StageManager.Instance.CurrentStageSelector == this;
            if (other.CompareTag("Player") && !isCurStage)
            {
                InitializeStage();
            }
        }
    }
}
