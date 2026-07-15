using UnityEngine;
using UnityEngine.InputSystem;

public class MapEditor : MonoBehaviour
{
    public enum EditorMode { Place, Remove, Move }
    public EditorMode currentMode = EditorMode.Place;

    [Header("設定")]
    public GameObject blockPrefab;
    public GameObject arrowPrefab;
    public LayerMask handleLayer;
    public float reachDistance = 100f;

    private GameObject selectedObject;
    private GameObject activeArrows;
    private Transform draggingHandle;
    private Vector3 dragStartPoint;
    private Vector3 initialObjPos;

    void Update()
    {
        HandleModeSwitching();

        // 矢印がブロックに完全に追従するように位置を毎フレーム更新
        if (activeArrows != null && selectedObject != null)
        {
            activeArrows.transform.position = selectedObject.transform.position;
        }

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            TryExecuteAction();
        }

        if (Mouse.current != null && Mouse.current.leftButton.isPressed && draggingHandle != null)
        {
            PerformDrag();
        }
        else if (Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame)
        {
            draggingHandle = null;
        }
    }

    void HandleModeSwitching()
    {
        if (Keyboard.current.digit1Key.wasPressedThisFrame) SetMode(EditorMode.Place);
        if (Keyboard.current.digit2Key.wasPressedThisFrame) SetMode(EditorMode.Remove);
        if (Keyboard.current.digit3Key.wasPressedThisFrame) SetMode(EditorMode.Move);
    }

    void SetMode(EditorMode mode)
    {
        currentMode = mode;
        // モード切替時に矢印と選択を解除
        ClearSelection();
    }

    void ClearSelection()
    {
        if (activeArrows != null) Destroy(activeArrows);
        selectedObject = null;
        draggingHandle = null;
    }

    void TryExecuteAction()
    {
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

        // 1. ハンドル判定
        if (Physics.Raycast(ray, out RaycastHit hitHandle, reachDistance, handleLayer))
        {
            draggingHandle = hitHandle.collider.transform;
            initialObjPos = selectedObject.transform.position; // 移動開始位置を保存

            // ハンドルをクリックした時点でコントローラーに通知
            var controller = activeArrows.GetComponent<ArrowHandleController>();
            if (controller != null)
            {
                // 選択状態とドラッグ状態を同時にセット
                controller.SetSelectedHandle(draggingHandle.name);
                controller.SetActiveHandle(draggingHandle.name);
            }

            // 平面での投影計算
            Plane dragPlane = new Plane(Camera.main.transform.forward, initialObjPos);
            dragPlane.Raycast(ray, out float enter);
            dragStartPoint = ray.GetPoint(enter);
        }
        // 2. ブロック/地面判定
        else if (Physics.Raycast(ray, out RaycastHit hit, reachDistance))
        {
            // 背景（何もないところ）をクリックした場合は解除
            if (currentMode == EditorMode.Move && !hit.collider.gameObject.CompareTag("Block"))
            {
                ClearSelection();
                return;
            }

            switch (currentMode)
            {
                case EditorMode.Place:
                    Vector3 pos = hit.point + (hit.normal * 0.5f);
                    Vector3 grid = new Vector3(Mathf.Round(pos.x), Mathf.Round(pos.y), Mathf.Round(pos.z));
                    if (Physics.OverlapSphere(grid, 0.1f).Length == 0)
                        Instantiate(blockPrefab, grid, Quaternion.identity);
                    break;
                case EditorMode.Remove:
                    Destroy(hit.collider.gameObject);
                    break;
                case EditorMode.Move:
                    selectedObject = hit.collider.gameObject;
                    if (activeArrows != null) Destroy(activeArrows);
                    activeArrows = Instantiate(arrowPrefab, selectedObject.transform.position, Quaternion.identity);
                    break;
            }
        }
        else if (currentMode == EditorMode.Move)
        {
            // 空虚をクリックした場合も解除
            ClearSelection();
        }
    }

    void PerformDrag()
    {
        if (selectedObject == null || draggingHandle == null) return;

        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

        // ドラッグ中の平面をカメラの向きに合わせる
        Plane dragPlane = new Plane(Camera.main.transform.forward, initialObjPos);

        if (dragPlane.Raycast(ray, out float enter))
        {
            Vector3 currentPoint = ray.GetPoint(enter);
            Vector3 delta = currentPoint - dragStartPoint;

            Vector3 move = Vector3.zero;
            string name = draggingHandle.name;

            // 1. 軸移動 (Axis)
            if (name.Contains("HandleX")) move = Vector3.Project(delta, Vector3.right);
            else if (name.Contains("HandleY")) move = Vector3.Project(delta, Vector3.up);
            else if (name.Contains("HandleZ")) move = Vector3.Project(delta, Vector3.forward);

            // 2. 平面移動 (Plane)
            // YZ平面なら X成分は固定(0)、YとZを反映
            else if (name.Contains("PlaneYZ")) move = new Vector3(0, delta.y, delta.z);
            // ZX平面なら Y成分は固定(0)、ZとXを反映
            else if (name.Contains("PlaneZX")) move = new Vector3(delta.x, 0, delta.z);
            // XY平面なら Z成分は固定(0)、XとYを反映
            else if (name.Contains("PlaneXY")) move = new Vector3(delta.x, delta.y, 0);

            // 移動後の位置を計算してグリッドスナップ
            Vector3 newPos = initialObjPos + move;
            selectedObject.transform.position = new Vector3(
                Mathf.Round(newPos.x),
                Mathf.Round(newPos.y),
                Mathf.Round(newPos.z)
            );
        }
    }
}