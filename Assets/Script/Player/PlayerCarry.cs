using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class PlayerCarry : MonoBehaviour
{
    // 現在運搬中のプレイヤー一覧
    private static List<PlayerCarry> carryingPlayers = new();

    // 現在運搬中のオブジェクト
    private CarryObject carryObject;

    // プレイヤーのNavMeshAgent
    private NavMeshAgent agent;

    // プレイヤーの状態管理
    private PlayerStateManager stateManager;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        stateManager = GetComponent<PlayerStateManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        // 状態管理がない場合は何もしない
        if (stateManager == null)
            return;

        // 運搬できない状態なら何もしない
        if (!stateManager.CanCarry())
            return;

        // お化け状態なら運搬できない
        if (stateManager.IsGhost())
            return;

        // 護衛モードなら運搬しない
        if (!stateManager.IsCarryMode())
            return;

        // 親オブジェクトからCarryObjectを探す
        CarryObject obj = other.GetComponentInParent<CarryObject>();

        // 運搬オブジェクトだった場合は運搬参加
        if (obj != null)
            obj.JoinCarry(this);
    }

    // お宝の運搬位置へプレイヤーを固定
    public void AttachToObject(CarryObject obj, Transform point)
    {
        // 状態管理がない場合は何もしない
        if (stateManager == null)
            return;

        // 運搬できない状態なら何もしない
        if (!stateManager.CanCarry())
            return;

        // お化け状態なら運搬できない
        if (stateManager.IsGhost())
            return;

        carryObject = obj;

        // 運搬中のプレイヤー一覧に追加
        if (!carryingPlayers.Contains(this))
            carryingPlayers.Add(this);

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

        // プレイヤーをお宝の子オブジェクトにする
        transform.SetParent(obj.transform);
    }

    // 運搬中か確認
    public bool IsCarrying()
    {
        return carryObject != null;
    }

    // 運搬しているお宝を取得
    public CarryObject GetCarryObject()
    {
        return carryObject;
    }

    // 運搬解除
    public void Release()
    {
        // 現在運搬しているお宝がある場合
        if (carryObject != null)
            carryObject.RemoveCarrier(this);

        // 親子関係を解除
        transform.SetParent(null);

        if (agent != null)
        {
            // NavMeshAgentを再有効化
            agent.enabled = true;

            // 現在位置をNavMeshAgentに同期
            if (agent.isOnNavMesh)
                agent.Warp(transform.position);
        }

        // 運搬中のプレイヤー一覧から削除
        carryingPlayers.Remove(this);

        // 運搬状態を解除
        carryObject = null;
    }

    // 現在運搬中の全プレイヤーを解除
    public static void ReleaseAllCarriers()
    {
        // 運搬中のプレイヤーをコピーしておく
        List<PlayerCarry> players = new(carryingPlayers);

        // 全プレイヤーを解除
        foreach (PlayerCarry player in players)
        {
            if (player != null)
                player.Release();
        }

        // 念のため一覧を空にする
        carryingPlayers.Clear();

        Debug.Log("運搬中の全プレイヤーを解除しました");
    }
}