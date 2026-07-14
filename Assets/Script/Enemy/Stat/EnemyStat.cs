using UnityEngine;

namespace CareerQuest.Enemy
{
    //  敵のパラメーター
    [System.Serializable]
    public sealed class EnemyStat
    {
        [Header("パラメーター")]
        public int HP = 100;
        public float MoveSpeed = 5;
        public EnemyTarget Target = EnemyTarget.Treasure;
        public float SearchRadius = 20f;
    }
}