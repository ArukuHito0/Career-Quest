using System;
using System.Collections.Generic;
using UnityEngine;

public class ObjectToggler : MonoBehaviour
{
    public NavMeshUpdater navMeshUpdater;

    [Serializable]
    public class ToggleItem
    {
        public string name;
        public GameObject targetObject;
        public bool isActive;
        public int requiredWoodCount; // 追加: 必要な木材数
        public List<Collider> triggerColliders = new List<Collider>(); // 追加: 複数のトリガー用コライダー
    }

    public List<ToggleItem> items = new List<ToggleItem>();

    private void Start()
    {
        SetupTriggers();
    }

    // 各トリガーコライダーに受信用コンポーネントを自動設定する
    private void SetupTriggers()
    {
        foreach (var item in items)
        {
            foreach (var col in item.triggerColliders)
            {
                if (col != null)
                {
                    // コライダーのisTriggerを自動で有効化（必要に応じて）
                    if (!col.isTrigger)
                    {
                        col.isTrigger = true;
                    }

                    // すでにAreaTriggerがアタッチされていなければ追加する
                    if (!col.TryGetComponent<AreaTrigger>(out var triggerReceiver))
                    {
                        triggerReceiver = col.gameObject.AddComponent<AreaTrigger>();
                    }

                    triggerReceiver.Initialize(this, item.name);
                }
            }

            // targetObject の初期アクティブ状態の同期
            if (item.targetObject != null)
            {
                item.targetObject.SetActive(item.isActive);
            }
        }
    }

    // トリガーから呼び出され、木材の消費と有効化を判定するメソッド
    public bool TryActivateItem(string itemName, PlayerInventory playerInventory)
    {
        foreach (var item in items)
        {
            if (item.name == itemName && !item.isActive)
            {
                // 木材の所持数が足りているか確認
                if (playerInventory.woodCount >= item.requiredWoodCount)
                {
                    // 木材を消費
                    if (playerInventory.ConsumeWood(item.requiredWoodCount))
                    {
                        item.isActive = true;
                        if (item.targetObject != null)
                        {
                            item.targetObject.SetActive(true);
                        }

                        // NavMeshの再計算
                        if (navMeshUpdater != null)
                        {
                            navMeshUpdater.UpdateNavMesh();
                        }

                        Debug.Log($"{item.name} が有効化されました。");
                        return true;
                    }
                }
                else
                {
                    Debug.Log($"{item.name} を有効化するには木材が {item.requiredWoodCount} 個必要です。（現在: {playerInventory.woodCount}個）");
                }
                return false;
            }
        }
        return false;
    }

    private void OnValidate()
    {
        if (items == null) return;

        foreach (var item in items)
        {
            if (item.targetObject != null && item.targetObject.activeSelf != item.isActive)
            {
                item.targetObject.SetActive(item.isActive);

                if (navMeshUpdater != null)
                {
                    navMeshUpdater.UpdateNavMesh();
                }
            }
        }
    }

    public bool ActivateItemByName(string itemName)
    {
        foreach (var item in items)
        {
            if (item.name == itemName && !item.isActive)
            {
                item.isActive = true;
                if (item.targetObject != null)
                {
                    item.targetObject.SetActive(true);
                }

                // NavMeshの再計算
                if (navMeshUpdater != null)
                {
                    navMeshUpdater.UpdateNavMesh();
                }

                return true; // 有効化成功
            }
        }
        return false; // 既に有効か、見つからなかった
    }
}