using UnityEngine;
using UnityEngine.AI;

public class PlayerCarry : MonoBehaviour
{
    private CarryObject carryObject; // 現在運搬中のオブジェクト
    private NavMeshAgent agent;      // プレイヤーのNavMeshAgent

    // プレイヤーの状態管理
    private PlayerStateManager stateManager;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        stateManager = GetComponent<PlayerStateManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        // 運搬できない状態なら何もしない
        if (!stateManager.CanCarry())
            return;

        // お化け状態なら運搬できない
        if (stateManager.IsGhost())
        {
            return;
        }

        // 護衛モードなら運搬しない
        if (!stateManager.IsCarryMode())
        {
            return;
        }

        // 親オブジェクトからCarryObjectを探す
        CarryObject obj = other.GetComponentInParent<CarryObject>();

        // 運搬オブジェクトだった場合は運搬参加
        if (obj != null)
        {
            obj.JoinCarry(this);
        }
    }


    public void AttachToObject(CarryObject obj, Transform point)
    {
        // 運搬できない状態なら何もしない
        if (!stateManager.CanCarry())
            return;

        // お化け状態なら運搬できない
        if (stateManager.IsGhost())
        {
            return;
        }

        carryObject = obj;

        NavMeshAgent agent = GetComponent<NavMeshAgent>();

        if (agent != null)
        {
            // 移動中の経路を削除
            agent.ResetPath();

            // NavMeshAgentを停止
            agent.enabled = false;
        }

        // 指定された位置へ移動
        transform.position = point.position;
        transform.rotation = point.rotation;

        // プレイヤーを宝物の子オブジェクトにする
        transform.SetParent(obj.transform);
    }


    // 運搬中か確認
    public bool IsCarrying()
    {
        return carryObject != null;
    }


    // 運搬解除
    public void Release()
    {
        // 親子関係を解除
        transform.SetParent(null);

        NavMeshAgent agent = GetComponent<NavMeshAgent>();

        if (agent != null)
        {
            // NavMeshAgentを再有効化
            agent.enabled = true;

            // 現在位置をNavMeshAgentに同期
            agent.Warp(transform.position);
        }

        carryObject = null;
    }
}