using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class ClickMoveController : MonoBehaviour
{
    // メインカメラ
    [SerializeField] private Camera mainCamera;

    // プレイヤー一覧
    [SerializeField] private PlayerMove[] players;

    // お宝
    [SerializeField] private CarryObject carryObject;

    // クリック位置を表示するマーカー
    [SerializeField] private Transform clickMarker;

    private void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    private void Update()
    {
        // マウスがない場合は何もしない
        if (Mouse.current == null)
            return;

        // 左クリック中
        if (Mouse.current.leftButton.isPressed)
            ClickMove();
    }

    // クリックした場所を目的地にする
    private void ClickMove()
    {
        // マウス位置からRayを飛ばす
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        // Rayが何にも当たらなければ何もしない
        if (!Physics.Raycast(ray, out RaycastHit hit))
            return;

        // 当たった場所からNavMesh上の位置を探す
        if (!NavMesh.SamplePosition(hit.point, out NavMeshHit navHit, 2f, NavMesh.AllAreas))
            return;

        // 全プレイヤーに目的地を設定
        foreach (PlayerMove player in players)
        {
            if (player != null)
                player.MoveTo(navHit.position);
        }

        // お宝が運搬可能なら移動
        if (carryObject != null && carryObject.CanCarry())
            carryObject.MoveTo(navHit.position);

        // クリック位置を表示
        if (clickMarker != null)
            clickMarker.position = navHit.position;
    }
}