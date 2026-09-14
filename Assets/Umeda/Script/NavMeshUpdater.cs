using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class NavMeshUpdater : MonoBehaviour
{
    public NavMeshSurface navMeshSurface;

    // --- 説明 ---
    // Obstacle = AIの進行を邪魔する障害物
    // Caving = 形状に合わせて穴掘り
    // ------------

    // --- 全体更新関数 ---
    public void UpdateNavMesh()
    {
        navMeshSurface.BuildNavMesh();
    }

    // --- 特定のオブジェクトを障害物として追加する関数 ---
    // プレイヤーが配置した橋や川のオブジェクトを渡すと、自動でObstacle設定を付与
    public void SetAsObstacle(GameObject target, bool isCarving)
    {
        // 既にNavMeshObstacleがあれば取得、なければ追加
        NavMeshObstacle obstacle = target.GetComponent<NavMeshObstacle>();
        if (obstacle == null)
        {
            obstacle = target.AddComponent<NavMeshObstacle>();
        }

        // Carvingの設定
        obstacle.carving = isCarving;

        // 必要に応じて形状を調整（メッシュを使う場合）
        // どれくらい移動したら再計算するか(動く障害物などを実装する場合に)
        obstacle.carvingMoveThreshold = 0.5f;
    }
}