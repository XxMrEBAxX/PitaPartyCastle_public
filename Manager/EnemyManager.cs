using System.Collections.Generic;
using UnityEngine;

namespace UB
{
    public class EnemyManager : Singleton<EnemyManager>
    {
        // Awake에서 등록 함수로 등록하고 등록된 오브젝트를 생성하는 함수를 생성
        private List<Enemy> _enemyList = new();

        public void AddEnemy(Enemy enemy)
        {
            _enemyList.Add(enemy);
        }

        public void RemoveEnemy(Enemy enemy)
        {
            // 아직 초기화되지 않은 경우, 그냥 무시한다.
            if (_enemyList == null)
            {
                return;
            }
            
            _enemyList.Remove(enemy);
        }
        
        public List<Enemy> GetEnemies()
        {
            return _enemyList;
        }
        
        /// <summary>
        /// 원거리 적들의 Bullet을 생성해줍니다.
        /// </summary>
        /// <param name="bullet: 등록할 총알 프리팹"></param>
        /// <param name="bulletName: 생성할 총알의 이름"></param>
        /// <param name="bulletCount: 생성할 총알의 개수"></param>
        public void AddBullet(GameObject bullet, string bulletName, int bulletCount)
        {
            ObjectPoolManager.Instance.AddPool(bullet, bulletName, bulletCount);
        }
    }
}