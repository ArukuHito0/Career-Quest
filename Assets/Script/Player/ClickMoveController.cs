using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class ClickMoveController : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private PlayerMove[] players;
    [SerializeField] private CarryObject carryObject;
    [SerializeField] private Transform clickMarker;

    private void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    private void Update()
    {
        // 押しっぱなし(isPressed)ではなく、押した瞬間(wasPressedThisFrame)に変更
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            ClickMove();
        }
    }

    private void ClickMove()
    {
        if (mainCamera == null) return;

        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (NavMesh.SamplePosition(hit.point, out NavMeshHit navHit, 2f, NavMesh.AllAreas))
            {
                // プレイヤー移動
                foreach (var player in players)
                {
                    player.MoveTo(navHit.position);
                }

                // 箱移動
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