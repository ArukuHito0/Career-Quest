using UnityEngine;

namespace CareerQuest.Enemy
{
    //  敵の情報
    public struct EnemyData
    {
        public EnemyID ID;          // 敵のID
        public EnemyTarget Target;  // 目標ID
        public byte State;          // 敵の状態(EnemyState)

        public int GroupVolume;  // 1グループの人数
        public byte IsLeader;    // 1:リーダー　0:フォロワー

        public int Hp;                       // 体力
        public int CurrentHp;                // 現在の体力
        public int AttackPower;              // 攻撃力
        public float AttackRange;            // 攻撃範囲
        public float AttackCoolDown;         // 攻撃間隔
        public float CurrentAttackCoolDown;  // 現在の攻撃間隔

        public uint UniqueMoveFlag;           // 敵ごとの独自処理
        public float UniqueMoveTimer;         // 独自処理の時間
        public float CurrentUniqueMoveTimer;  // 現在の独自処理用タイマー  

        public Vector3 Position;             // 座標
        public int TargetIndex;              // ターゲットのIndex
        public float SearchRadius;           // ターゲット探索範囲
        public float BodyTickness;           // 体の厚さ  

        public float EnemyDistance;  // 敵同士どのくらい距離を取るか
    }
}