using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class CarryObject : MonoBehaviour
{
    [SerializeField] private int requiredPeople = 4;  // 運搬に必要な人数
    [SerializeField] private Transform[] carryPoints; // プレイヤーが立つ位置

    private List<PlayerCarry> carriers = new(); // 現在参加しているプレイヤー

    private NavMeshAgent agent;
    private bool canCarry;      // 運搬可能かどうか


    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }


    // プレイヤーを運搬に参加させる
    public bool JoinCarry(PlayerCarry player)
    {
        // すでに参加済みなら実行しない
        if (carriers.Contains(player))
            return false;

        // 立つ場所がないなら実行しない
        if (carriers.Count >= carryPoints.Length)
            return false;

        // 参加者リストへ追加
        carriers.Add(player);

        // 空いている位置に配置
        player.AttachToObject(this, carryPoints[carriers.Count - 1]);

        // 必要人数に達したら運搬可能
        if (carriers.Count >= requiredPeople)
        {
            canCarry = true;
            Debug.Log("運搬可能");
        }

        return true;
    }


    // 運搬オブジェクトを移動させる
    public void MoveTo(Vector3 target)
    {
        // 人数不足なら動かない
        if (!canCarry)
            return;

        // NavMeshAgentで移動
        agent.SetDestination(target);
    }


    // 運搬可能か確認
    public bool CanCarry()
    {
        return canCarry;
    }


    // 現在位置を取得
    public Vector3 GetPosition()
    {
        return transform.position;
    }
}