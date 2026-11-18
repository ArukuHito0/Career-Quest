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
            [Min(0.01f)] public float Percent;
        }

        [System.Serializable]
        public struct SpawnPoint
        {
            public Vector3 Position;  // スポーン座標
            [Min(0)] public byte StartSpawnCnt;  // 出現回数
            [Min(0)] public byte TresuareTriggerSpawnCnt;  // お宝取得時出現回数
            public EnemySpawnCandidate[] Candidates;  // この座標で湧く可能性がある敵たち
        }

        public SpawnPoint[] SpawnPoints;

#if UNITY_EDITOR
        void OnValidate()
        {
            if (SpawnPoints == null) return;

            for (int i = 0; i < SpawnPoints.Length; i++)
            {
                if (SpawnPoints[i].StartSpawnCnt < 0)
                {
                    SpawnPoints[i].StartSpawnCnt = 0;
                }
                if (SpawnPoints[i].TresuareTriggerSpawnCnt < 0)
                {
                    SpawnPoints[i].TresuareTriggerSpawnCnt = 0;
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
        public byte StartSpawnCnt;
        public byte TresureTriggerSpwanCnt;
        public EnemySpawnData.EnemySpawnCandidate[] Candidates;
    }
}