using UnityEngine;

namespace CareerQuest.Enemy
{
    //  敵の情報
    public struct EnemyData
    {
        public float MoveSpeed;            // 移動速度
        public float SearchRadius;         // 探知範囲

        public int TargetIndex;            // ターゲットのIndex
        public Vector3 Position;           // 座標
        public Vector3 Velocity;           // 移動速度
        public Vector3 AvoidanceVelocity;  // 回避速度

        public byte State;
        public float StateTimer;
    }
}