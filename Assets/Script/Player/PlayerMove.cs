using UnityEngine;
using UnityEngine.AI;

public class PlayerMove : MonoBehaviour
{
    //  プレイヤーのNavMeshAgent
    private NavMeshAgent agent;


    private void Awake()
    {
        // 自分についているNavMeshAgentを取得
        agent = GetComponent<NavMeshAgent>();
    }

    // 指定した座標へ移動する
    public void MoveTo(Vector3 position)
    {
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