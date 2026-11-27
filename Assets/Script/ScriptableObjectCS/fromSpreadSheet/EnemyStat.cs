namespace CareerQuest.Enemy
{
    [System.Serializable]
    public sealed class EnemyStat
    {
        public EnemyID ID;
        public int HP;
        public int AttackPower;
        public float AttackRange;
        public float AttackCooldown;
        public float MoveSpd;
        public float SearchRadius;
        public int GroupVolume;
    }

    public enum EnemyID : byte
    {
        Golem = 0,
        Ghost = 1,
        Swarm = 2,
    }
}