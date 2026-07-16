using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ObjectToggler : MonoBehaviour
{
    // NavMeshUpdaterを登録（インスペクターからドラッグ＆ドロップ）
    public NavMeshUpdater navMeshUpdater;

    [Serializable]
    public class ToggleItem
    {
        public string name;
        public GameObject targetObject;
        public bool isActive;
    }

    public List<ToggleItem> items = new List<ToggleItem>();

    private void OnValidate()
    {
        if (items == null) return;

        foreach (var item in items)
        {
            if (item.targetObject != null && item.targetObject.activeSelf != item.isActive)
            {
                item.targetObject.SetActive(item.isActive);

                // 重要：ここで再計算を強制する
                // 橋を置いた直後にBuildNavMeshを呼ぶのが一番確実です
                // 全体ではなくSurfaceに指定した範囲だけが計算されます
                if (navMeshUpdater != null)
                {
                    navMeshUpdater.UpdateNavMesh();
                }
            }
        }
    }
}