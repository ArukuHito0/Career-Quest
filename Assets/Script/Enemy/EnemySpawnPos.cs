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
            [Min(0.01f)]public float Percent;
        }

        [System.Serializable]
        public struct SpawnPoint
        {
            public Vector3 Position;  // スポーン座標
            [Min(0.1f)] public float SpawnInterval;
            public EnemySpawnCandidate[] Candidates;  // この座標で湧く可能性がある敵たち
        }

        public SpawnPoint[] SpawnPoints;

#if UNITY_EDITOR
        void OnValidate()
        {
            if (SpawnPoints == null) return;

            for (int i = 0; i < SpawnPoints.Length; i++)
            {
                if (SpawnPoints[i].SpawnInterval < 0.1f)
                {
                    SpawnPoints[i].SpawnInterval = 0.1f;
                }

                if (SpawnPoints[i].Candidates != null)
                {
                    for (int j = 0; j < SpawnPoints[i].Candidates.Length; j++)
                    {
                        if (SpawnPoints[i].Candidates[j].Percent < 0.01f)
                        {
                            var candidate = SpawnPoints[i].Candidates[j];
                            candidate.Percent = 0.01f;
                            SpawnPoints[i].Candidates[j] = candidate;
                        }
                    }
                }
            }
        }
#endif
    }

    public struct RuntimeSpawnPoint
    {
        public Vector3 Position;
        public float CurrentTimer;
        public float SpawnInterval;
        public EnemySpawnData.EnemySpawnCandidate[] Candidates;
    }
}
