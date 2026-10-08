using UnityEngine;

namespace CareerQuest.Enemy
{
    [CreateAssetMenu(fileName = "EnemySpawnPos", menuName = "Game/Enemy/SpawnPos")]
    public sealed class EnemySpawnPos : ScriptableObject
    {
        public Vector3[] SpawnPositions;
        void OnDrawGizmosSelected()
        {
            if (SpawnPositions == null) return;

            for (int i = 0; i < SpawnPositions.Length; i++)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(SpawnPositions[i], 50f);
            }
        }
    }
}
