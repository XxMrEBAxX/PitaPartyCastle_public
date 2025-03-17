using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace UB
{
    [CreateAssetMenu(menuName = "Data/Effect Data")]
    public class EffectData : ScriptableObject
    {
        [System.Serializable]
        public class EffectPrefab
        {
            [OverrideLabel("이름")]
            public string name;
            public GameObject prefab;
            [OverrideLabel("미리 생성할 수"), Range(1, 100)]
            public int poolSize = 3;
        }

        [Header("플레이어 관련")]
        [Space(10)]
        public List<EffectPrefab> playerEffects;

        [Space(20)]

        [Header("적 관련")]
        [Space(10)]
        public List<EffectPrefab> enemyEffects;

        [Space(20)]

        [Header("환경 관련")]
        [Space(10)]
        public List<EffectPrefab> environmentEffects;

        [Space(20)]

        [Header("기타")]
        [Space(10)]
        public List<EffectPrefab> otherEffects;
    }
}
