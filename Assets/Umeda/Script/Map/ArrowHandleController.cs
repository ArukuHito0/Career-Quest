using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class ArrowHandleController : MonoBehaviour
{
    // グループ名をプルダウンで選択するための列挙型
    public enum GroupType
    {
        PlaneYZ, PlaneZX, PlaneXY,
        HandleX, HandleY, HandleZ
    }

    [System.Serializable]
    public class ObjectGroup
    {
        public GroupType type;             // インスペクターで選択
        public List<Transform> targets;    // 複数のオブジェクトを参照
        public Color defaultColor;         // このグループの色
    }

    [Header("グループ設定")]
    public List<ObjectGroup> planeGroups = new List<ObjectGroup>();
    public List<ObjectGroup> handleGroups = new List<ObjectGroup>();

    [Header("設定")]
    public Color activeColor = Color.yellow;
    private Transform camTransform;
    private const float OFFSET = 0.125f;

    private bool isDragging = false;
    private string activeHandleName = "";
    private string selectedHandleName = "";

    void Start()
    {
        camTransform = Camera.main.transform;
        UpdatePlanePositions();
        UpdateColors();
    }

    void Update()
    {
        // ギズモのスケール制御
        if (camTransform != null)
        {
            float dist = Vector3.Distance(transform.position, camTransform.position);
            transform.localScale = Vector3.one * (dist * 0.15f);
        }

        if (Mouse.current == null) return;

        // ドラッグ終了検知
        if (Mouse.current.leftButton.wasReleasedThisFrame && isDragging)
        {
            OnDragEnd();
        }

        // 右ボタンを放した時に平面位置を再計算
        if (Mouse.current.rightButton.wasReleasedThisFrame)
        {
            UpdatePlanePositions();
        }
    }

    // 全グループを更新するメソッド
    private void UpdateColors()
    {
        UpdateGroupList(planeGroups);
        UpdateGroupList(handleGroups);
    }

    private void UpdateGroupList(List<ObjectGroup> groups)
    {
        foreach (var group in groups)
        {
            string groupName = group.type.ToString();
            bool isActive = (activeHandleName == groupName);
            bool isSelected = (selectedHandleName == groupName);

            // 連動ロジック: 平面選択時は対応する軸をハイライト
            if (selectedHandleName == "PlaneYZ" && (groupName == "HandleY" || groupName == "HandleZ")) isSelected = true;
            if (selectedHandleName == "PlaneZX" && (groupName == "HandleZ" || groupName == "HandleX")) isSelected = true;
            if (selectedHandleName == "PlaneXY" && (groupName == "HandleX" || groupName == "HandleY")) isSelected = true;

            Color targetColor = (isActive || isSelected) ? activeColor : group.defaultColor;

            // 各グループ内のターゲット全てに色を適用
            foreach (var target in group.targets)
            {
                if (target == null) continue;
                Renderer[] renders = target.GetComponentsInChildren<Renderer>();
                foreach (var rend in renders)
                {
                    rend.material.SetColor("_MainColor", targetColor);
                    if (rend.material.HasProperty("_OutlineColor"))
                        rend.material.SetColor("_OutlineColor", new Color(targetColor.r, targetColor.g, targetColor.b, 0.8f));
                }
            }
        }
    }

    public void SetSelectedHandle(string name) { selectedHandleName = name; UpdateColors(); }
    public void SetActiveHandle(string name) { activeHandleName = name; isDragging = true; UpdateColors(); }
    public void OnDragEnd() { isDragging = false; activeHandleName = ""; selectedHandleName = ""; UpdateColors(); UpdatePlanePositions(); }

    public void UpdatePlanePositions()
    {
        if (camTransform == null) camTransform = Camera.main.transform;

        Vector3 offsetToCam = camTransform.position - transform.position;
        float sx = (offsetToCam.x >= 0) ? 1f : -1f;
        float sy = (offsetToCam.y >= 0) ? 1f : -1f;
        float sz = (offsetToCam.z >= 0) ? 1f : -1f;

        Transform pYZ = transform.Find("PlaneYZ");
        if (pYZ) pYZ.localPosition = new Vector3(0, sy * OFFSET, sz * OFFSET);

        Transform pZX = transform.Find("PlaneZX");
        if (pZX) pZX.localPosition = new Vector3(sx * OFFSET, 0, sz * OFFSET);

        Transform pXY = transform.Find("PlaneXY");
        if (pXY) pXY.localPosition = new Vector3(sx * OFFSET, sy * OFFSET, 0);
    }
}