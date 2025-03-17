using System.Collections.Generic;
using UB.EVENT;
using UnityEngine;

namespace UB
{
    public class StageManager : Singleton<StageManager>
    {
        public StageSelector CurrentStageSelector { get; set; } = null;

        private Dictionary<string, bool> _stageClear = new Dictionary<string, bool>();
        private Dictionary<string, bool> _stageObjectDateGUID = new Dictionary<string, bool>();

        protected override void Awake()
        {
            base.Awake();
        }

        public bool GetStageClear(string stageName)
        {
            if (_stageClear.ContainsKey(stageName))
                return _stageClear[stageName];

            return false;
        }

        public bool GetStageObjectClear(string GUID)
        {
            if (_stageObjectDateGUID.ContainsKey(GUID))
                return _stageObjectDateGUID[GUID];

            return false;
        }

        /// <summary>
        /// 현재 스테이지를 초기화 하고 저장합니다.
        /// </summary>
        public void ResetCurrentStage()
        {
            CurrentStageSelector.ResetStage();
        }

        /// <summary>
        /// 죽었을 시 현재 스테이지가 클리어 상태가 아니라면 초기화 하고 저장합니다.
        /// </summary>
        public void DeadCurrentStage()
        {
            CurrentStageSelector.DeadStage();
        }

        /// <summary>
        /// 현재 스테이지의 클리어 여부와 오브젝트 클리어 여부를 저장합니다.
        /// </summary>
        public void SaveStageData()
        {
            if (ReferenceEquals(CurrentStageSelector, null))
                return;

            SaveToStageClearData(CurrentStageSelector.StageName, CurrentStageSelector.IsClear);
            foreach (StageInteractive stageInteractive in CurrentStageSelector.StageObjects)
            {
                if (stageInteractive.StageInitializeTypeInstance == StageInitializeType.SAVE)
                {
                    if(string.IsNullOrEmpty(stageInteractive.Guid))
                        return;
                    SaveToStageObjectData(stageInteractive.Guid, stageInteractive.IsCleared);
                }

            }
        }


        private void SaveToStageClearData(string stageName, bool isCleared)
        {
            if (_stageClear.ContainsKey(stageName))
            {
                _stageClear[stageName] = isCleared;
            }
            else
            {
                _stageClear.Add(stageName, isCleared);
            }
        }

        private void SaveToStageObjectData(string GUID, bool isCleared)
        {
            if (_stageObjectDateGUID.ContainsKey(GUID))
            {
                _stageObjectDateGUID[GUID] = isCleared;
            }
            else
            {
                _stageObjectDateGUID.Add(GUID, isCleared);
            }
        }

        public void ResetStageData()
        {
            _stageClear.Clear();
            _stageObjectDateGUID.Clear();
            CurrentStageSelector = null;
        }
    }
}
