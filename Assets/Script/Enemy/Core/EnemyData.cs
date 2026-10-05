using UnityEngine;

namespace CareerQuest.Enemy
{
    //  “G‚Ìî•ñ
    public struct EnemyData
    {
        public EnemyID ID;          // “G‚ÌID
        public EnemyTarget Target;  // –Ú•WID
        public byte State;          // “G‚Ìó‘Ô(EnemyState)
        public int CurrentHp;       // Œ»İ‚Ì‘Ì—Í
        public float CurrentAttackCoolDown;  // Œ»İ‚ÌUŒ‚ŠÔŠu
        public Vector3 Position;    // À•W
        public int TargetIndex;     // ƒ^[ƒQƒbƒg‚ÌIndex

        //  -- GolemStatus
        public int   GolemMaxHp;            // Å‘å‘Ì—Í
        public float GolemMoveSpeed;        // ˆÚ“®‘¬“x ¡‚ÍNavmesh‚ÌmoveSpd‚ğg‚Á‚Ä‚é
        public float GolemSearchRadius;     // ’T’m”ÍˆÍ
        public float GolemTickness;         // ‘Ì‚ÌŒú‚³
        public int   GolemAttackPower;      // UŒ‚—Í
        public float GolemAttackRange;      // UŒ‚”ÍˆÍ
        public float GolemAttackCoolDown;   // UŒ‚ŠÔŠu

        //  -- GhostStatus
        public float GhostMaxHp;           // Å‘å‘Ì—Í
        public float GhostMoveSpeed;       // ˆÚ“®‘¬“x  ¡‚ÍNavmesh‚ÌmoveSpd‚ğg‚Á‚Ä‚é
        public float GhostTickness;        // ‘Ì‚ÌŒú‚³
        public float GhostSearchRadius;    // ’T’m”ÍˆÍ
        public int   GhostAttackPower;     // UŒ‚—Í
        public float GhostAttackRange;     // ’T’m”ÍˆÍ
        public float GhostAttackCoolDown;  // UŒ‚ŠÔŠu
    }
}