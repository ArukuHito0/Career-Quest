using CareerQuest.Core;
using CareerQuest.Enemy;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerMove : MonoBehaviour, ISpatialEntity
{
    // プレイヤーのNavMeshAgent
    private NavMeshAgent agent;

    PlayerHashManager _hashManager;
    public List<int> nearbyEntities = new List<int>(64);

    // 最後に指定された移動先
    private Vector3 lastTargetPosition;

    // 移動先が設定されているか
    private bool hasTargetPosition;

    public int Index { get; set; }
    public float Tickness { get; set; }

    // プレイヤーの状態管理
    private PlayerStateManager stateManager;

    private void Awake()
    {
        // プレイヤーに付いているNavMeshAgentを取得
        agent = GetComponent<NavMeshAgent>();

        // プレイヤーの状態管理を取得
        stateManager = GetComponent<PlayerStateManager>();

        _hashManager = ServiceLocator.Resolve<PlayerHashManager>();
        _hashManager.Register(this);

        Tickness = 0.2f;
    }

    private void Update()
    {
        nearbyEntities.Clear();

        int myX = Mathf.FloorToInt(transform.position.x / _hashManager.cellSize);
        int myZ = Mathf.FloorToInt(transform.position.z / _hashManager.cellSize);
        int myCellId = myX + (myZ * _hashManager.girdWidth);

        for (int dz = -1; dz <= 1; dz++)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                int targetCellId = (myX + dx) + ((myZ + dz) * _hashManager.girdWidth);

                _hashManager.GetEntitiesInCell(targetCellId, nearbyEntities);
            }
        }

        foreach (int index in nearbyEntities)
        {
            if (_hashManager.ActiveEntities[index] == this)
                continue;

            var otherEnemy = _hashManager.ActiveEntities[index];

            float dist = Vector3.Distance(transform.position, otherEnemy.transform.position);

            if (dist < 20.0f)
            {
            }
        }
    }

    // 指定した場所を目的地として設定
    public void MoveTo(Vector3 position)
    {
        // 着替え中でも目的地は更新する
        lastTargetPosition = position;
        hasTargetPosition = true;

        // 状態管理がない場合は移動できない
        if (stateManager == null)
            return;

        // 着替え中の場合
        if (stateManager.IsChanging())
        {
            // 現在の移動を完全に停止
            StopMovement();

            // 目的地だけ更新して終了
            return;
        }

        // お化け状態など移動できない場合
        if (!stateManager.CanMove())
        {
            StopMovement();
            return;
        }

        // NavMeshAgentが無効なら何もしない
        if (!agent.enabled)
            return;

        // NavMesh上にいない場合は何もしない
        if (!agent.isOnNavMesh)
            return;

        // 指定した場所へ移動
        agent.SetDestination(position);
    }

    // 保存している目的地へ再び移動
    public void ResumeMove()
    {
        // 目的地がなければ何もしない
        if (!hasTargetPosition)
            return;

        // まだ着替え中なら移動しない
        if (stateManager != null && stateManager.IsChanging())
        {
            StopMovement();
            return;
        }

        // 移動できない状態なら何もしない
        if (stateManager != null && !stateManager.CanMove())
        {
            StopMovement();
            return;
        }

        // NavMeshAgentが無効なら何もしない
        if (!agent.enabled)
            return;

        // NavMesh上にいない場合は何もしない
        if (!agent.isOnNavMesh)
            return;

        // 保存していた最後の目的地へ移動
        agent.SetDestination(lastTargetPosition);
    }

    // プレイヤーの移動を停止
    public void StopMovement()
    {
        // NavMeshAgentがない場合
        if (agent == null)
            return;

        // NavMeshAgentが無効の場合
        if (!agent.enabled)
            return;

        // 現在の移動経路を削除
        agent.ResetPath();

        // 現在の速度を0にする
        agent.velocity = Vector3.zero;
    }

    // 現在保存している目的地を取得
    public Vector3 GetTargetPosition()
    {
        return lastTargetPosition;
    }

    // 目的地が設定されているか
    public bool HasTargetPosition()
    {
        return hasTargetPosition;
    }
}