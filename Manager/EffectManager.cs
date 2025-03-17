using System;
using System.Collections.Generic;
using UnityEngine;

namespace UB
{
    public class EffectManager : Singleton<EffectManager>
    {
        /// <summary>
        /// 이펙트 데이터
        /// </summary>
        public EffectData EffectDataInstance;
        
        private Action _previousAddPoolAction;

        private void Start()
        {
            _previousAddPoolAction?.Invoke();

            AddPoolList();
        }

        private void AddPoolList()
        {
            // player Effects Add
            foreach (var effect in EffectDataInstance.playerEffects)
            {
                ObjectPoolManager.Instance.AddPool(effect.prefab, effect.name, effect.poolSize);
            }
            // enemy Effects Add
            foreach (var effect in EffectDataInstance.enemyEffects)
            {
                ObjectPoolManager.Instance.AddPool(effect.prefab, effect.name, effect.poolSize);
            }
            // environment Effects Add
            foreach (var effect in EffectDataInstance.environmentEffects)
            {
                ObjectPoolManager.Instance.AddPool(effect.prefab, effect.name, effect.poolSize);
            }
            // other Effects Add
            foreach (var effect in EffectDataInstance.otherEffects)
            {
                ObjectPoolManager.Instance.AddPool(effect.prefab, effect.name, effect.poolSize);
            }
        }

        /// <summary>
        /// 이펙트를 생성합니다.
        /// </summary>
        /// <param name="name">(data에 등록된 이름을 사용)</param>
        public GameObject PlayAndGetEffect(EffectEnum name)
        {
            
            var obj = ObjectPoolManager.Instance.LetsGo(name.ToString());
            return obj;
        }

        /// <summary>
        /// AddPool 이전의 실행될 액션을 추가합니다.
        /// </summary>
        /// <param name="action"></param>
        public void AddPreviousAddPoolAction(Action action)
        {
            _previousAddPoolAction += action;
        }
    }
}
