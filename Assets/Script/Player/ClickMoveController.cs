
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ClickMoveController : MonoBehaviour
{
    [Header("カメラ")]
    [SerializeField] private Camera mainCamera;

    [Header("お宝")]
    [SerializeField] private CarryObject carryObject;

    [Header("クリックマーカー")]
    [SerializeField] private Transform clickMarker;

    [Header("ユニット選択UI")]
    [SerializeField] private UnitSelectionUI selectionUI;

    [Header("移動設定")]
    [SerializeField] private LayerMask groundLayer = ~0;
    [SerializeField] private float sampleDistance = 2f;

    private PlayerMove[] players;

    private void Start()
    {
        // プレイヤーのユニットを取得
        players = PlayerUnitRegistry.GetAllPlayerMoves();

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (selectionUI == null)
        {
            Debug.LogError("UnitSelectionUIが設定されていません！");
        }

        if (players == null || players.Length == 0)
        {
            Debug.LogWarning("操作可能なユニットが見つかりません。");
        }
    }

    private void Update()
    {
        // マウスの左クリックを押した瞬間だけ処理
        if (Mouse.current == null ||
            !Mouse.current.leftButton.isPressed)
        {
            return;
        }

        // UIをクリックした場合は移動指示を出さない
        if (IsPointerOverUI())
        {
            return;
        }

        ClickMove();
    }

    private bool IsPointerOverUI()
    {
        if (EventSystem.current == null)
        {
            return false;
        }

        var eventData = new PointerEventData(EventSystem.current)
        {
            position = Mouse.current.position.ReadValue()
        };

        var results = new System.Collections.Generic.List<RaycastResult>();

        EventSystem.current.RaycastAll(eventData, results);

        foreach (RaycastResult result in results)
        {
            if (result.gameObject.GetComponent<Graphic>() != null ||
                result.gameObject.GetComponentInParent<Graphic>() != null)
            {
                return true;
            }
        }

        return false;
    }

    private void ClickMove()
    {
        if (mainCamera == null)
        {
            Debug.LogWarning("Main Cameraが設定されていません。");
            return;
        }

        if (selectionUI == null)
        {
            Debug.LogWarning("UnitSelectionUIが設定されていません。");
            return;
        }

        // カメラからクリック位置へRayを飛ばす
        Ray ray = mainCamera.ScreenPointToRay(
            Mouse.current.position.ReadValue()
        );

        // 地面に当たったか確認
        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer))
        {
            return;
        }

        // NavMesh上の移動可能な位置を取得
        if (!UnityEngine.AI.NavMesh.SamplePosition(
            hit.point,
            out UnityEngine.AI.NavMeshHit navHit,
            sampleDistance,
            UnityEngine.AI.NavMesh.AllAreas))
        {
            return;
        }

        Vector3 targetPosition = navHit.position;

        // 選択されているユニットだけ移動
        if (players != null)
        {
            foreach (PlayerMove player in players)
            {
                if (player == null)
                {
                    continue;
                }

                if (!selectionUI.IsTarget(player))
                {
                    continue;
                }

                player.MoveTo(targetPosition);
            }
        }

        // AllまたはCarrier選択時だけお宝を移動
        if (carryObject != null && carryObject.CanCarry() && selectionUI != null && selectionUI.IsTreasureTargetSelected())
        {
            carryObject.MoveTo(navHit.position);
        }


        // クリックマーカーを移動先に表示
        if (clickMarker != null)
        {
            clickMarker.position = targetPosition;
        }
    }
}
