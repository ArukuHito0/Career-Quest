using CareerQuest.Core;
using CareerQuest.Enemy;
using System.Collections.Generic;
using UnityEditor.AdaptivePerformance.Editor;
using UnityEngine;
using UnityEngine.AI;

public class PlayerMove : MonoBehaviour
{
    // プレイヤーのNavMeshAgent
    private NavMeshAgent agent;

    // 最後に指定された移動先
    private Vector3 lastTargetPosition;

    // 移動先が設定されているか
    private bool hasTargetPosition;

    // 現在移動中か
    private bool isMoving;

    // 到着判定距離
    private float arrivalDistance = 1.5f;
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

        if (agent != null)
        {
            agent.stoppingDistance = arrivalDistance;
        }
    }

    private void Update()
    {
        // 移動中でなければ何もしない
        if (!isMoving)
            return;

        if (agent == null || !agent.enabled || !agent.isOnNavMesh)
            return;

        // 経路計算中は待機
        if (agent.pathPending)
            return;

        // 経路が存在しない場合
        if (!agent.hasPath)
            return;

        // 目的地までの距離を確認
        if (agent.remainingDistance <= arrivalDistance + 0.1f)
        {
            StopAtDestination();
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

        // 移動できない場合
        if (!stateManager.CanMove())
        {
            StopMovement();
            return;
        }

        // NavMeshAgentが無効なら何もしない
        if (!agent.enabled || !agent.isOnNavMesh)
            return;

        // 新しい移動指示なので移動を再開
        agent.isStopped = false;

        // 指定した場所へ移動
        if (agent.SetDestination(position))
        {
            isMoving = true;
        }
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
        if (!agent.enabled || !agent.isOnNavMesh)
            return;

        // すでに目的地に到着している場合は再移動しない
        if (Vector3.Distance(transform.position, lastTargetPosition) <= arrivalDistance)
        {
            StopAtDestination();
            return;
        }
    }

    // 目的地に到着したときの停止処理
    private void StopAtDestination()
    {
        isMoving = false;

        if (agent == null || !agent.enabled || !agent.isOnNavMesh)
            return;

        // NavMeshAgentを停止
        agent.isStopped = true;

        // 移動経路を削除
        agent.ResetPath();

        // 速度をゼロにする
        agent.velocity = Vector3.zero;
    }

    // プレイヤーの移動を停止
    public void StopMovement()
    {
        isMoving = false;

        // NavMeshAgentがない場合
        if (agent == null || !agent.enabled)
            return;

        // NavMeshAgentを停止
        agent.isStopped = true;

        if (agent.isOnNavMesh)
        {
            // 現在の移動経路を削除
            agent.ResetPath();

            // 現在の速度を0にする
            agent.velocity = Vector3.zero;
        }
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