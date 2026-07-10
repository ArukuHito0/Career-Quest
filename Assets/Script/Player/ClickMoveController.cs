using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class ClickMoveController : MonoBehaviour
{

    [SerializeField] private Camera mainCamera;

    [SerializeField] private Transform clickMarker;

    [SerializeField] private PlayerMove[] players;

    void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }

    void Update()
    {
        if (Mouse.current.leftButton.isPressed)
        {
            ClickMove();
        }
    }

    private void ClickMove()
    {
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // NavMesh上の近い位置を取得
            if (NavMesh.SamplePosition(hit.point, out NavMeshHit navHit, 2.0f, NavMesh.AllAreas))
            {
                foreach (var player in players)
                {
                    // 移動
                    player.MoveTo(navHit.position);
                }

                // マーカー移動
                if (clickMarker)
                {
                    clickMarker.position = navHit.position;
                }
            }
        }
    }
}