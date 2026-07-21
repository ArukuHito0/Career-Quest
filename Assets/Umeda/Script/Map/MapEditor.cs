using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

[System.Serializable]
public class BlockSetting
{
    public string name;
    public bool useSurfaceAndInside;
    public GameObject surfacePrefab, insidePrefab, singlePrefab;
    public void Validate() { if (useSurfaceAndInside) { singlePrefab = null; } else { surfacePrefab = null; insidePrefab = null; } }
}

public class MapEditor : MonoBehaviour
{
    public enum EditorMode { Place, Remove, Move, Replace }
    public EditorMode currentMode = EditorMode.Place;

    [Header("編集設定")]
    public bool enableDragDrawing = true;
    public float dragInterval = 0.1f;
    public float moveDeadzone = 0.2f; // マウスの微小移動を無視するしきい値

    [Header("ドラッグ時の軸固定")]
    public bool lockX = false;
    public bool lockY = false;
    public bool lockZ = false;
    private Vector3 dragBasePosition; // 固定軸の基準となる初期位置

    [Header("ブロック設定")]
    public List<BlockSetting> blockSettings = new List<BlockSetting>();
    public int currentBlockIndex = 0;

    [Header("その他設定")]
    public GameObject arrowPrefab;
    public LayerMask handleLayer;
    public float reachDistance = 100f;

    private GameObject selectedObject;
    private GameObject activeArrows;
    private Transform draggingHandle;
    private Vector3 dragStartPoint, initialObjPos;
    private Vector3 lastPlacedPosition = new Vector3(float.MinValue, float.MinValue, float.MinValue);
    private float lastPlacedTime = 0f;

    // なぞり判定用
    private Vector3 lastMousePosition;
    private bool isDragging = false;

    private void OnValidate() { foreach (var setting in blockSettings) setting.Validate(); }

    void Update()
    {
        HandleModeSwitching();

        if (currentMode != EditorMode.Move && activeArrows != null) ClearSelection();
        if (currentMode == EditorMode.Move && activeArrows != null && selectedObject != null)
            activeArrows.transform.position = selectedObject.transform.position;

        if (Mouse.current != null)
        {
            // --- ドラッグ開始 ---
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                if (Mouse.current.leftButton.wasPressedThisFrame)
                {
                    isDragging = true;
                    lastMousePosition = Mouse.current.position.ReadValue();

                    // ドラッグ開始時のヒット位置を基準点として記録
                    Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
                    if (Physics.Raycast(ray, out RaycastHit hit, reachDistance))
                    {
                        Vector3 pos = hit.point + (hit.normal * 0.5f);
                        dragBasePosition = new Vector3(Mathf.Round(pos.x), Mathf.Round(pos.y), Mathf.Round(pos.z));
                    }

                    TryExecuteAction();
                }
            }

            // --- ドラッグ中（カーソルが止まっていたら配置しない） ---
            if (isDragging && Mouse.current.leftButton.isPressed)
            {
                Vector3 currentMousePos = Mouse.current.position.ReadValue();
                float mouseDelta = Vector3.Distance(currentMousePos, lastMousePosition);

                // マウスが一定以上動いている かつ 時間経過している場合のみ配置
                if (mouseDelta > 5f && enableDragDrawing && currentMode != EditorMode.Move && Time.time >= lastPlacedTime + dragInterval)
                {
                    TryExecuteAction();
                    lastMousePosition = currentMousePos; // 位置を更新
                }
            }

            // --- ドラッグ終了 ---
            if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                isDragging = false;
            }
        }

        if (Mouse.current != null && Mouse.current.leftButton.isPressed && draggingHandle != null) PerformDrag();
        else if (Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame) draggingHandle = null;
    }

    void TryExecuteAction()
    {
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (currentMode == EditorMode.Move && Physics.Raycast(ray, out RaycastHit hitHandle, reachDistance, handleLayer))
        {
            draggingHandle = hitHandle.collider.transform;
            initialObjPos = selectedObject.transform.position;
            var controller = activeArrows.GetComponent<ArrowHandleController>();
            if (controller != null)
            {
                controller.SetSelectedHandle(draggingHandle.name);
                controller.SetActiveHandle(draggingHandle.name);
            }
            Plane dragPlane = new Plane(Camera.main.transform.forward, initialObjPos);
            dragPlane.Raycast(ray, out float enter);
            dragStartPoint = ray.GetPoint(enter);
        }
        else if (Physics.Raycast(ray, out RaycastHit hit, reachDistance))
        {
            Vector3 pos = hit.point + (hit.normal * 0.5f);
            Vector3 grid = new Vector3(Mathf.Round(pos.x), Mathf.Round(pos.y), Mathf.Round(pos.z));

            // 軸固定の適用
            if (lockX) grid.x = dragBasePosition.x;
            if (lockY) grid.y = dragBasePosition.y;
            if (lockZ) grid.z = dragBasePosition.z;

            if (Vector3.Distance(grid, lastPlacedPosition) < moveDeadzone) return;

            switch (currentMode)
            {
                case EditorMode.Place:
                    if (Physics.OverlapSphere(grid, 0.1f).Length == 0)
                    {
                        Instantiate(GetAppropriatePrefab(grid), grid, Quaternion.identity);
                        lastPlacedPosition = grid; lastPlacedTime = Time.time;
                    }
                    break;
                case EditorMode.Remove:
                    Destroy(hit.collider.gameObject);
                    lastPlacedPosition = grid; lastPlacedTime = Time.time;
                    break;
                case EditorMode.Replace:
                    Vector3 targetPos = hit.collider.transform.position;
                    Destroy(hit.collider.gameObject);
                    Instantiate(GetAppropriatePrefab(targetPos), targetPos, Quaternion.identity);
                    lastPlacedPosition = targetPos; lastPlacedTime = Time.time;
                    break;
                case EditorMode.Move:
                    if (hit.collider.gameObject.CompareTag("Block"))
                    {
                        selectedObject = hit.collider.gameObject;
                        if (activeArrows != null) Destroy(activeArrows);
                        activeArrows = Instantiate(arrowPrefab, selectedObject.transform.position, Quaternion.identity);
                    }
                    break;
            }
        }
    }

    GameObject GetAppropriatePrefab(Vector3 position)
    {
        if (blockSettings.Count == 0 || currentBlockIndex < 0 || currentBlockIndex >= blockSettings.Count) return null;
        var setting = blockSettings[currentBlockIndex];
        if (!setting.useSurfaceAndInside) return setting.singlePrefab;
        return Physics.CheckSphere(position + Vector3.up, 0.4f) ? setting.insidePrefab : setting.surfacePrefab;
    }

    void HandleModeSwitching()
    {
        if (Keyboard.current.digit1Key.wasPressedThisFrame) SetMode(EditorMode.Place);
        if (Keyboard.current.digit2Key.wasPressedThisFrame) SetMode(EditorMode.Remove);
        if (Keyboard.current.digit3Key.wasPressedThisFrame) SetMode(EditorMode.Move);
        if (Keyboard.current.digit4Key.wasPressedThisFrame) SetMode(EditorMode.Replace);
    }

    void SetMode(EditorMode mode) { currentMode = mode; ClearSelection(); }

    void ClearSelection()
    {
        if (activeArrows != null) { DestroyImmediate(activeArrows); activeArrows = null; }
        selectedObject = null; draggingHandle = null;
#if UNITY_EDITOR
        UnityEditor.Selection.activeGameObject = null;
#endif
    }

    void PerformDrag()
    {
        if (selectedObject == null || draggingHandle == null) return;
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        Plane dragPlane = new Plane(Camera.main.transform.forward, initialObjPos);
        if (dragPlane.Raycast(ray, out float enter))
        {
            Vector3 delta = ray.GetPoint(enter) - dragStartPoint;
            Vector3 move = Vector3.zero;
            string n = draggingHandle.name;
            if (n.Contains("HandleX")) move = Vector3.Project(delta, Vector3.right);
            else if (n.Contains("HandleY")) move = Vector3.Project(delta, Vector3.up);
            else if (n.Contains("HandleZ")) move = Vector3.Project(delta, Vector3.forward);
            else if (n.Contains("PlaneYZ")) move = new Vector3(0, delta.y, delta.z);
            else if (n.Contains("PlaneZX")) move = new Vector3(delta.x, 0, delta.z);
            else if (n.Contains("PlaneXY")) move = new Vector3(delta.x, delta.y, 0);
            selectedObject.transform.position = new Vector3(
                Mathf.Round(initialObjPos.x + move.x), Mathf.Round(initialObjPos.y + move.y), Mathf.Round(initialObjPos.z + move.z));
        }
    }
}