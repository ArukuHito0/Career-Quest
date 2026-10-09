namespace CareerQuest.Enemy
{
    public struct BulletHitData
    {
        public int BulletIndex; // 当たった弾のインデックス
        public int EnemyIndex;  // 当たった敵のインデックス
        public int Damage;  // キャッシュ用
    }
}