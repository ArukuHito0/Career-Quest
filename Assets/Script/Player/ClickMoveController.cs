
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System.Collections.Generic;

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

    // UI判定用のリスト
    private readonly List<RaycastResult> uiResults = new();

    private void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    private void Update()
    {
        if (Mouse.current == null)
            return;

        // 左クリックした瞬間だけ判定する
        if (!Mouse.current.leftButton.isPressed)
            return;

        // クリック位置にUIがあれば移動しない
        if (IsPointerOverUI())
            return;

        ClickMove();
    }

    // マウス位置にUIがあるか確認
    private bool IsPointerOverUI()
    {
        if (EventSystem.current == null)
            return false;

        PointerEventData pointerData = new PointerEventData(EventSystem.current);
        pointerData.position = Mouse.current.position.ReadValue();

        uiResults.Clear();
        EventSystem.current.RaycastAll(pointerData, uiResults);

        // UI上のクリックだけを判定する
        foreach (RaycastResult result in uiResults)
        {
            if (result.module is UnityEngine.UI.GraphicRaycaster)
                return true;
        }

        return false;
    }

    // クリックした場所を目的地にする
    private void ClickMove()
    {
        if (mainCamera == null)
            return;

        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (!Physics.Raycast(ray, out RaycastHit hit))
            return;

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
