using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class CarryObject : MonoBehaviour
{
    // 運搬に必要な最低人数
    [SerializeField] private int requiredPeople = 3;

    // 通常のユニットと同じ移動速度
    [SerializeField] private float normalMoveSpeed = 3.5f;

    // プレイヤーを配置する場所
    [SerializeField] private Transform[] carryPoints;

    // 現在運搬しているプレイヤー
    private List<PlayerCarry> carriers = new();

    // CarryPointを誰が使用しているか
    private Dictionary<Transform, PlayerCarry> pointUsers = new();

    // お宝のNavMeshAgent
    private NavMeshAgent agent;

    // 最大参加人数
    private int maxPeople;

    // 現在運搬可能か
    private bool canCarry;

    private void Awake()
    {
        // お宝のNavMeshAgentを取得
        agent = GetComponent<NavMeshAgent>();

        // 最低人数の2倍を最大人数にする
        maxPeople = requiredPeople * 2;

        // CarryPointの数を最大人数に合わせる
        if (carryPoints.Length < maxPeople)
            Debug.LogWarning("carryPointsの数が最大参加人数より少ないです");
    }

    // プレイヤーを運搬に参加させる
    public bool JoinCarry(PlayerCarry player)
    {
        // プレイヤーが存在しない場合
        if (player == null)
            return false;

        // すでに参加している場合
        if (carriers.Contains(player))
            return false;

        // 最大人数に達している場合
        if (carriers.Count >= maxPeople)
            return false;

        // 空いているCarryPointを探す
        Transform freePoint = GetFreeCarryPoint();

        // 空いているCarryPointがない場合
        if (freePoint == null)
            return false;

        // プレイヤーを一覧に追加
        carriers.Add(player);

        // CarryPointの使用者として登録
        pointUsers[freePoint] = player;

        // プレイヤーをお宝に固定
        player.AttachToObject(this, freePoint);

        // 運搬人数に応じて速度を更新
        UpdateMoveSpeed();

        // 必要人数が集まったか確認
        if (carriers.Count >= requiredPeople)
        {
            canCarry = true;

            Debug.Log($"運搬可能になりました 人数:{carriers.Count}");
        }

        return true;
    }

    // 空いているCarryPointを探す
    private Transform GetFreeCarryPoint()
    {
        foreach (Transform point in carryPoints)
        {
            // CarryPointが存在しない場合はスキップ
            if (point == null)
                continue;

            // まだ誰も使っていないCarryPointなら使用する
            if (!pointUsers.ContainsKey(point))
                return point;
        }

        // 空いているCarryPointがない
        return null;
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

        // 現在の人数に応じた速度に更新
        UpdateMoveSpeed();

        // 指定した場所へ移動
        agent.SetDestination(target);
    }

    // プレイヤーを運搬一覧から外す
    public void RemoveCarrier(PlayerCarry player)
    {
        if (player == null)
            return;

        // 使用しているCarryPointを探す
        Transform usedPoint = null;

        foreach (KeyValuePair<Transform, PlayerCarry> pair in pointUsers)
        {
            if (pair.Value == player)
            {
                usedPoint = pair.Key;
                break;
            }
        }

        // CarryPointを空き状態にする
        if (usedPoint != null)
            pointUsers.Remove(usedPoint);

        // 運搬一覧から削除
        carriers.Remove(player);

        // 人数に応じて速度を更新
        UpdateMoveSpeed();

        // 必要人数を下回った場合
        if (carriers.Count < requiredPeople)
        {
            canCarry = false;

            // お宝の移動を停止
            StopMoving();

            Debug.Log($"運搬人数が不足しました 人数:{carriers.Count}");
        }
        else
        {
            // 必要人数以上なら運搬可能
            canCarry = true;

            Debug.Log($"運搬人数変更 人数:{carriers.Count}");
        }
    }

    // 運搬人数に応じて移動速度を変更
    private void UpdateMoveSpeed()
    {
        if (agent == null)
            return;

        // 人数が0の場合
        if (carriers.Count <= 0)
        {
            agent.speed = 0f;
            return;
        }

        // 最低人数
        float minPeople = requiredPeople;

        // 最大人数
        float maxPeopleValue = maxPeople;

        // 現在人数を最低～最大の範囲に収める
        float currentPeople = Mathf.Clamp(carriers.Count, requiredPeople, maxPeople);

        // 最低人数を0、最大人数を1として割合を計算
        float speedRate = Mathf.InverseLerp(minPeople, maxPeopleValue, currentPeople);

        // 最低人数の速度
        float minSpeed = normalMoveSpeed * 0.5f;

        // 最低速度～通常速度の間で計算
        agent.speed = Mathf.Lerp(minSpeed, normalMoveSpeed, speedRate);
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

        // 現在の移動経路を解除
        agent.ResetPath();

        // 移動速度を0にする
        agent.velocity = Vector3.zero;
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

    // 現在の運搬人数
    public int GetCarrierCount()
    {
        return carriers.Count;
    }

    // 最大運搬人数
    public int GetMaxPeople()
    {
        return maxPeople;
    }
}