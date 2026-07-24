using System;
using System.Collections.Generic;
using UnityEngine;

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

    private void Start()
    {
        SetupTriggers();
    }

    // 各ターゲットオブジェクトにトリガー受信用コンポーネントを自動設定する
    private void SetupTriggers()
    {
        foreach (var item in items)
        {
            if (item.targetObject != null)
            {
                // すでにアタッチされていなければ追加する
                if (!item.targetObject.TryGetComponent<ToggleTriggerReceiver>(out var receiver))
                {
                    receiver = item.targetObject.AddComponent<ToggleTriggerReceiver>();
                }

                receiver.objectToggler = this;
                receiver.targetName = item.name;

                // トリガー用のコライダーがついているか確認（なければBoxCollider等を追加してIs Triggerにする案内など）
                if (!item.targetObject.TryGetComponent<Collider>(out var col))
                {
                    Debug.LogWarning($"{item.name} ({item.targetObject.name}) にコライダーが見つかりません。コライダーを追加してください。");
                }
                else if (!col.isTrigger)
                {
                    // 必要に応じて自動でIs Triggerにするか、警告を出す
                    Debug.Log($"{item.name} のコライダーの 'Is Trigger' を有効にしてください。");
                }
            }
        }
    }

    // トリガーから呼び出され、指定された名前のアイテムを有効化する
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
}