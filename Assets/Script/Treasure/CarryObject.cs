using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class CarryObject : MonoBehaviour
{
    // 運搬に必要な人数
    [SerializeField] private int requiredPeople = 4;

    // プレイヤーを配置する場所
    [SerializeField] private Transform[] carryPoints;

    // 現在運搬しているプレイヤー
    private List<PlayerCarry> carriers = new();

    // お宝のNavMeshAgent
    private NavMeshAgent agent;

    // 現在運搬可能か
    private bool canCarry;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    // プレイヤーを運搬に参加させる
    public bool JoinCarry(PlayerCarry player)
    {
        // すでに参加している場合
        if (carriers.Contains(player))
            return false;

        // 運搬位置が足りない場合
        if (carriers.Count >= carryPoints.Length)
            return false;

        // プレイヤーを一覧に追加
        carriers.Add(player);

        // プレイヤーをお宝に固定
        player.AttachToObject(this, carryPoints[carriers.Count - 1]);

        // 必要人数が集まったか確認
        if (carriers.Count >= requiredPeople)
        {
            canCarry = true;

            Debug.Log("運搬可能になりました");
        }

        return true;
    }

    // お宝を移動させる
    public void MoveTo(Vector3 target)
    {
        // 運搬可能でなければ動かさない
        if (!canCarry)
            return;

        // NavMeshAgentがない場合
        if (agent == null)
            return;

        // NavMeshAgentが無効の場合
        if (!agent.enabled)
            return;

        // NavMesh上にいない場合
        if (!agent.isOnNavMesh)
            return;

        agent.SetDestination(target);
    }

    // プレイヤーを運搬一覧から外す
    public void RemoveCarrier(PlayerCarry player)
    {
        if (player == null)
            return;

        // 一覧から削除
        carriers.Remove(player);

        // 必要人数を下回ったら運搬不可
        if (carriers.Count < requiredPeople)
        {
            canCarry = false;

            // お宝の移動を停止
            StopMoving();

            Debug.Log("運搬人数が不足したため、お宝の移動を停止しました");
        }
    }

    // お宝の移動を停止
    private void StopMoving()
    {
        if (agent == null)
            return;

        if (!agent.enabled)
            return;

        if (!agent.isOnNavMesh)
            return;

        // 現在の目的地を解除
        agent.ResetPath();
    }

    // 運搬可能か
    public bool CanCarry()
    {
        return canCarry;
    }

    // 現在のお宝の位置
    public Vector3 GetPosition()
    {
        return transform.position;
    }
}