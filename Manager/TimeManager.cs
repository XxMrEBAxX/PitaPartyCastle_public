using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem.Controls;

namespace UB
{
    public class TimeManager : Singleton<TimeManager>
    {
        [SerializeField] private readonly float DefaultTimeScale = 1.0f;
        public float TimeScale { get; private set; }
        public float OriginTimeScale { get; private set; } = 1.0f;
        public float PreviousTimeScale { get; private set; }
        public bool AbleSetTimeScale { get; set; } = true;
        protected override void Awake()
        {
            base.Awake();
        }
        private void Start()
        {
            TimeScale = DefaultTimeScale;
            SetTimeScale(DefaultTimeScale);
            SetFixedDeltaTime(1);
            //TimeScale = 0;
        }

        public void SetTimeScale(float scale)
        {
            if (!AbleSetTimeScale)
                return;

            PreviousTimeScale = TimeScale;
            TimeScale = scale;
            Time.timeScale = scale;
            //SetFixedDeltaTime(scale);
        }

        public void SetFixedDeltaTime(float scale)
        {
            // 0.00185f 는 모니터 주사율 540hz 기준 ㄷㄷ
            float fixedDeltaTime = scale * 1.0f / Application.targetFrameRate;
            Time.fixedDeltaTime = Mathf.Max(fixedDeltaTime, 0.00185f);
        }

        public void SetForceTimeScale(float scale)
        {
            bool origin = AbleSetTimeScale;
            AbleSetTimeScale = true;
            SetTimeScale(scale);
            AbleSetTimeScale = origin;
        }

        public void SetOriginTimeScale()
        {
            SetTimeScale(OriginTimeScale);
        }

        public float GetDeltaTime()
        {
            return Time.deltaTime;
        }

        public float GetUnscaledDeltaTime()
        {
            return Time.unscaledDeltaTime;
        }

        public float GetTime()
        {
            return Time.time;
        }

        public float GetUnscaledTime()
        {
            return Time.unscaledTime;
        }

    }
}
