using UnityEngine;

namespace CareerQuest.Enemy
{
    //  “G‚Ìî•ñ
    public struct EnemyData
    {
        public EnemyID ID;          // “G‚ÌID
        public byte State;          // “G‚Ìó‘Ô(EnemyState)
        public Vector3 Position;    // À•W
        public int TargetIndex;     // ƒ^[ƒQƒbƒg‚ÌIndex

        //  -- GolemStatus
        public float GolemMoveSpeed;        // ˆÚ“®‘¬“x
        public float GolemSearchRadius;     // ’T’m”ÍˆÍ
        public float GolemBodyTickness;     // ‘Ì‚ÌŒú‚³
        public int GolemAttackPower;        // UŒ‚—Í
        public int GolemAttackRange;        // UŒ‚”ÍˆÍ

        //  -- GhostStatus
        public float GhostMoveSpeed;     // ˆÚ“®‘¬“x
        public float GhostSearchRadius;  // ’T’m”ÍˆÍ
        public float GhostAttackRange;   // ’T’m”ÍˆÍ
        public float GhostBodyTickness;  // ‘Ì‚ÌŒú‚³
    }
}