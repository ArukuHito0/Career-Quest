namespace CareerQuest.Enemy
{
    public enum InfoType : byte
    {
        State  = 0,  // 状態変更
        Damage = 1,  // ダメージ処理
        Speed  = 2,  // スピード変更
    }

    public struct pendingInfo
    {
        public int Index;  //  エネミーインデックス
        public InfoType InfoType;

        public EnemyState State;
        public byte Damage;
        public float MoveSpd;
    }
}