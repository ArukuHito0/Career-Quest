using UnityEngine;
using UnityEngine.AI;

public class PlayerMove : MonoBehaviour
{
    //  プレイヤーのNavMeshAgent
    private NavMeshAgent agent;

    private PlayerStateManager stateManager;

    private void Awake()
    {
        // 自分についているNavMeshAgentを取得
        agent = GetComponent<NavMeshAgent>();
        stateManager = GetComponent<PlayerStateManager>();
    }

    // 指定した座標へ移動する
    public void MoveTo(Vector3 position)
    {
        // 着替え中なら移動できない
        if (stateManager != null && !stateManager.CanMove())
            return;

        // 運搬中などでAgentが無効なら何もしない
        if (!agent.enabled)
            return;

        // NavMesh上にいない場合も移動できない
        if (!agent.isOnNavMesh)
            return;

        // 目的地を設定
        agent.SetDestination(position);
    }
}