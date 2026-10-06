using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class PlayerCarry : MonoBehaviour
{
    // 現在運搬中のプレイヤー一覧
    private static List<PlayerCarry> carryingPlayers = new();

    // 全プレイヤー共通の運搬参加禁止終了時刻
    private static float globalCarryCooldownEndTime;

    // ReleaseAllCarriers実行後の運搬参加禁止時間
    [SerializeField] private float globalCarryCooldown = 1f;

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

    private void Update()
    {
        // 運搬中でなければ何もしない
        if (carryObject == null)
            return;

        // お化け状態になったら運搬解除
        if (stateManager.IsGhost())
            Release();
    }

    private void OnTriggerEnter(Collider other)
    {
        // 状態管理がない場合は何もしない
        if (stateManager == null)
            return;

        // 全体の再参加クールタイム中なら何もしない
        if (IsGlobalCarryCooldown())
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

        // すでに別のお宝を運搬中なら参加しない
        if (carryObject != null)
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

        // 全体の再参加クールタイム中なら参加しない
        if (IsGlobalCarryCooldown())
            return;

        // 運搬できない状態なら何もしない
        if (!stateManager.CanCarry())
            return;

        // お化け状態なら運搬できない
        if (stateManager.IsGhost())
            return;

        // すでに別のお宝を運搬している場合は参加しない
        if (carryObject != null)
            return;

        // お宝が存在しない場合は参加しない
        if (obj == null)
            return;

        // CarryPointが存在しない場合は参加しない
        if (point == null)
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
        // すでに運搬解除されている場合は何もしない
        if (carryObject == null)
            return;

        // 現在運搬しているお宝を保存
        CarryObject obj = carryObject;

        // 運搬対象を先に解除
        carryObject = null;

        // お宝側から運搬者を削除
        obj.RemoveCarrier(this);

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
    }

    // 全体の運搬参加禁止中か確認
    private static bool IsGlobalCarryCooldown()
    {
        return Time.time < globalCarryCooldownEndTime;
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

        // クールタイム時間
        float cooldown = 1f;

        // 運搬していたプレイヤーから設定値を取得
        foreach (PlayerCarry player in players)
        {
            if (player != null)
            {
                cooldown = player.globalCarryCooldown;
                break;
            }
        }

        // 現在時刻 + クールタイム時間を終了時刻として保存
        globalCarryCooldownEndTime = Time.time + cooldown;

        Debug.Log($"全ユニットの運搬参加を{cooldown}秒間禁止しました");
    }
}