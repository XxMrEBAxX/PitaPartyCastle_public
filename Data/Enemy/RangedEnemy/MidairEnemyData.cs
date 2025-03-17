using UnityEngine;
using MyBox;

namespace UB
{
    [CreateAssetMenu(menuName = "Data/Enemy/Ranged Enemy/MidairEnemyData")]
    public class MidairEnemyData : RangedEnemyData
    {
        [OverrideLabel("순찰 범위(circle)의 반지름")] 
        [SerializeField] private float _radius = 3f;
         public float Radius => _radius;
    }
}