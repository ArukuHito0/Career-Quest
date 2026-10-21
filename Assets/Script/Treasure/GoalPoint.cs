
using UnityEngine;

public class GoalPoint : MonoBehaviour
{
    // ゴール判定がすでに成功しているか
    private bool isGoalReached;

    // Triggerの範囲に何かが入ったときに呼ばれる
    private void OnTriggerEnter(Collider other)
    {
        // すでにクリア判定済みなら何もしない
        if (isGoalReached)
            return;

        // ゲーム管理オブジェクトが存在しない場合は処理しない
        if (GameManager.Instance == null)
        {
            Debug.LogError("GoalPoint：GameManagerが見つかりません。");
            return;
        }

        // すでにゲームが終了している場合は何もしない
        if (GameManager.Instance.IsGameEnded)
            return;

        // 接触したオブジェクトか親からTreasureChestを探す
        TreasureChest treasure = other.GetComponentInParent<TreasureChest>();

        // お宝以外ならクリア判定しない
        if (treasure == null)
            return;

        // お宝がゴールに到達したことを記録する
        isGoalReached = true;

        Debug.Log("お宝がゴールに到達しました！");

        // ゲームクリア処理を開始する
        GameManager.Instance.GameClear();
    }
}
