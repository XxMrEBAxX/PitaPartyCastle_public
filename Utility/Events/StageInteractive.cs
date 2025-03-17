using UnityEngine;
using UB.EVENT;

namespace UB.EVENT
{
    public enum StageInitializeType
    {
        SAVE = 0, // Save - 런타임 상태를 저장하여 저장한 값을 따라감
        CLEAR, // Clear - 클리어 시 항상 활성화 하거나 죽어있는 상태로 진입
        IGNORE // Ignore - 클리어 여부 관계 없이 처음 상태로 돌아감

        // Save 랑 Ignore 차이
        // Save 는 클리어 하지 않고 넘어가도 상태를 저장하여 유지하고
        // Ignore 은 클리어 여부 상관 없이 기본값을 유지함
    }

    public delegate void StageInteractiveEventHandler(bool clear);
    public class StageInteractive : MonoBehaviour
    {
        public event StageInteractiveEventHandler StageEventHandler;
        public StageInitializeType StageInitializeTypeInstance;
        /// <summary>
        /// Save 일 시에만 이 값을 따라감 <para></para>
        /// </summary>
        public bool IsCleared = false;
        /// <summary>
        /// 빌드 생성 시점에서 GUID 할당
        /// </summary>
        public string Guid;

        public void Initialize(bool clear)
        {
            StageEventHandler?.Invoke(clear);
        }
    }
}

namespace UB
{
    [RequireComponent(typeof(StageInteractive))]
    public abstract class StageObject : MonoBehaviour, IStageObject
    {
        public StageInteractive stage { get; set; }

        protected virtual void Awake()
        {
            stage = GetComponent<StageInteractive>();
            stage.StageEventHandler += InitializeStageObject;
        }
        public abstract void InitializeStageObject(bool clear);
    }

    interface IStageObject
    {
        StageInteractive stage { get; set; }

        public abstract void InitializeStageObject(bool clear);
    }
}
