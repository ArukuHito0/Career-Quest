using System;
using System.Collections.Generic;
using System.Collections;
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
        public int requiredWoodCount = 10;                // それぞれの橋や建築に必要な木材数
        [HideInInspector] public int currentWoodCount = 0;  // 現在納品された木材数
        public List<Collider> triggerColliders = new List<Collider>();
        public Transform dropPoint;                       // 納品された木材が吸い込まれる（またはドロップする）位置（未設定ならトリガー中心）
    }

    public List<ToggleItem> items = new List<ToggleItem>();

    [Header("納品・演出設定")]
    public float buildInterval = 0.08f;       // 木材を消費して構築していくテンポ（秒）
    public GameObject flyingLogPrefab;        // 納品時に背中から飛んでいく木材のプレハブ（省略可）

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
                    if (!col.isTrigger)
                    {
                        col.isTrigger = true;
                    }

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
                if (item.isActive)
                {
                    item.currentWoodCount = item.requiredWoodCount;
                }
            }
        }
    }

    // プレイヤーがトリガーに入った時にAreaTriggerから呼ばれるコルーチン開始用メソッド
    public IEnumerator StartBuildingRoutine(string itemName, PlayerInventory playerInventory)
    {
        ToggleItem targetItem = items.Find(x => x.name == itemName);
        if (targetItem == null || targetItem.isActive) yield break;

        // 必要な木材に達するまで、またはプレイヤーの木材がなくなるまで1個ずつ消費して建造する
        while (!targetItem.isActive && targetItem.currentWoodCount < targetItem.requiredWoodCount)
        {
            if (playerInventory != null && playerInventory.stackedLogs.Count > 0)
            {
                // 背中から木材（GameObject）を1つ取り出す
                GameObject logToConsume = playerInventory.PopLog();

                if (logToConsume != null)
                {
                    targetItem.currentWoodCount++;

                    // 納品位置の決定（dropPointが未設定ならトリガーやターゲットの位置を基準にする）
                    Vector3 targetPos = targetItem.dropPoint != null
                        ? targetItem.dropPoint.position
                        : (targetItem.targetObject != null ? targetItem.targetObject.transform.position : transform.position);

                    // 背中からその場所へ木材が飛んでいく演出コルーチンを実行
                    StartCoroutine(FlyLogToBuildPoint(logToConsume, targetPos, targetItem));

                    // 必要数に達したら完了
                    if (targetItem.currentWoodCount >= targetItem.requiredWoodCount)
                    {
                        targetItem.isActive = true;
                        if (targetItem.targetObject != null)
                        {
                            targetItem.targetObject.SetActive(true);
                        }

                        // NavMeshの再計算
                        if (navMeshUpdater != null)
                        {
                            navMeshUpdater.UpdateNavMesh();
                        }

                        Debug.Log($"{targetItem.name} が完全に完成しました！");
                    }
                }
            }
            else
            {
                // 木材がなくなったら納品ループを抜ける
                break;
            }

            yield return new WaitForSeconds(buildInterval);
        }
    }

    // 背中から持っていた木材が、指定した納品場所へ飛んでいって消える（またはドロップする）演出
    private IEnumerator FlyLogToBuildPoint(GameObject logObj, Vector3 targetPos, ToggleItem item)
    {
        // 親子関係を解除してワールド座標で動かす
        logObj.transform.SetParent(null);

        // 当たり判定や物理挙動を無効化
        Rigidbody rb = logObj.GetComponent<Rigidbody>();
        if (rb != null) Destroy(rb);
        Collider col = logObj.GetComponent<Collider>();
        if (col != null) col.enabled = false;

        Vector3 startPos = logObj.transform.position;
        float duration = 0.2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // 放物線を描きながらターゲット位置へ吸い込まれる
            Vector3 currentPos = Vector3.Lerp(startPos, targetPos, t);
            currentPos.y += Mathf.Sin(t * Mathf.PI) * 1.0f;

            logObj.transform.position = currentPos;
            yield return null;
        }

        // 到着したら木材を消去（またはその場所に積んでいくことも可能）
        Destroy(logObj);
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