using UnityEngine;

namespace CareerQuest.Enemy
{
    //  敵の情報
    public struct EnemyData
    {
        public byte State;          // 敵の状態(EnemyState)
        public float MoveSpeed;     // 移動速度
        public Vector3 Position;    // 座標
        public float SearchRadius;  // 探知範囲
        public float BodyTickness;  // 体の厚さ
        public int TargetIndex;     // ターゲットのIndex

        //  -- Jobで使用しない変数 --  //
        public int AttackPower;     // 攻撃力
    }
}