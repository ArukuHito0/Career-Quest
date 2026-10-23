using UnityEngine;

namespace CareerQuest.Enemy
{
    //  “G‚Ìî•ñ
    public struct EnemyData
    {
        public EnemyID ID;          // “G‚ÌID
        public EnemyTarget Target;  // –Ú•WID
        public byte State;          // “G‚Ìó‘Ô(EnemyState)

        public int GroupVolume;  // 1ƒOƒ‹[ƒv‚Ìl”
        public byte IsLeader;    // 1:ƒŠ[ƒ_[@0:ƒtƒHƒƒ[

        public int Hp;                       // ‘Ì—Í
        public int CurrentHp;                // Œ»İ‚Ì‘Ì—Í
        public int AttackPower;              // UŒ‚—Í
        public float AttackRange;            // UŒ‚”ÍˆÍ
        public float AttackCoolDown;         // UŒ‚ŠÔŠu
        public float CurrentAttackCoolDown;  // Œ»İ‚ÌUŒ‚ŠÔŠu

        public Vector3 Position;             // À•W
        public int TargetIndex;              // ƒ^[ƒQƒbƒg‚ÌIndex
        public float SearchRadius;           // ƒ^[ƒQƒbƒg’Tõ”ÍˆÍ
        public float BodyTickness;           // ‘Ì‚ÌŒú‚³  

        public float EnemyDistance;  // “G“¯m‚Ç‚Ì‚­‚ç‚¢‹——£‚ğæ‚é‚©
    }
}