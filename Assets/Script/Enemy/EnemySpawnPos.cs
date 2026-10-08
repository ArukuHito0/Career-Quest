using UnityEngine;

namespace CareerQuest.Enemy
{
    [CreateAssetMenu(fileName = "EnemySpawnData", menuName = "Game/Enemy/SpawnData")]
    public sealed class EnemySpawnData : ScriptableObject
    {
        //  湧く敵の候補と確率
        [System.Serializable]
        public struct EnemySpawnCandidate
        {
            public EnemyID EnemyID;
            [Min(3f)] public float SpawnInterval;
            [Min(0.01f)]public float Percent;
        }

        [System.Serializable]
        public struct SpawnPointData
        {
            public Vector3 Position;  // スポーン座標
            public EnemySpawnCandidate[] Candidates;  // この座標で湧く可能性がある敵たちのリスト
        }

        public SpawnPointData[] SpawnPositions;
    }

    public struct RuntimeSpawnPoint
    {
        public Vector3 Position;
        public float CurrentTimer;
        public float TargetInterval;
        public EnemySpawnData.EnemySpawnCandidate[] Candidates;
    }
}
