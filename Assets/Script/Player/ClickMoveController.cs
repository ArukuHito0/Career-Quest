using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class ClickMoveController : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;       // 使用するカメラ
    [SerializeField] private PlayerMove[] players;    // 操作対象のプレイヤー一覧
    [SerializeField] private CarryObject carryObject; // 運搬対象のオブジェクト
    [SerializeField] private Transform clickMarker;   // クリック位置を表示するマーカー

    private void Start()
    {
        // カメラが未設定ならMainCameraを取得
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }

    private void Update()
    {
        // 左クリックで移動命令を出す
        if (Mouse.current.leftButton.isPressed)
        {
            ClickMove();
        }
    }

    // クリック地点に移動させる
    private void ClickMove()
    {
        if (mainCamera == null) return;

        // マウス位置からRayを飛ばす
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        // Rayが何かに当たったか
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // 当たった地点に近いNavMesh上の座標を取得
            if (NavMesh.SamplePosition(hit.point, out NavMeshHit navHit, 2f, NavMesh.AllAreas))
            {
                // プレイヤー移動
                foreach (var player in players)
                {
                    player.MoveTo(navHit.position);
                }
                
                // 運搬オブジェクト移動
                if (carryObject != null && carryObject.CanCarry())
                {
                    carryObject.MoveTo(navHit.position);
                }
                
                // クリック位置表示
                if (clickMarker != null)
                {
                    clickMarker.position = navHit.position;
                }
            }
        }
    }
}