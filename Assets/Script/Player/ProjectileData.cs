using UnityEngine;

namespace CareerQuest.Player
{
    //  投射物データ
    public struct BulletData
    {
        public byte Damage;
        public float Radius;  // 投射物半径
        public Vector3 Position;  // 座標
        public bool IsActive;
    }
}