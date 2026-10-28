namespace CareerQuest.Enemy
{
    public struct DamageInfo
    {
        public int Index;  //  エネミーインデックス
        public byte Damage;
    }

    public struct StateInfo
    {
        public int Index;  //  エネミーインデックス
        public EnemyState State;
    }
}