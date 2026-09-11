using UnityEngine;

namespace CareerQuest.Enemy
{
    //  敵のパラメーター
    [System.Serializable]
    public sealed class EnemyStat
    {
        [Header("敵プレハブ")]
        public GameObject EnemyPrefab;
        [Header("パラメーター")]
        public int HP = 100;
        public int AttackPower = 5;
        public float AtackRange = 5f;
        public float MoveSpeed = 5;
        public float SearchRadius = 20f;
        [Header("衝突回避パラメーター")]
        [Tooltip("壁を回避し始める距離")]
        public float WallAvoidRadius = 10f;
        [Tooltip("敵を回避し始める距離")]
        public float EnmeyAvoidRadius = 2f;
        [Tooltip("体の厚み(衝突判定計算用)")]
        public float BodyTickness = 1f;

    }
}