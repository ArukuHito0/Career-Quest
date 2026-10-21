using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI; // UIを使うため追加

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
        public Transform dropPoint;                       // 納品された木材が吸い込まれる位置

        [Header("円形UI設定")]
        public Image radialFillImage;                     // Image Type が Filled になっている円形バーのImage
        public GameObject sliderCanvasObject;             // スライダーを含むCanvasオブジェクト（ビルボード用スクリプトを推奨）
    }

    public List<ToggleItem> items = new List<ToggleItem>();

    [Header("納品・演出設定")]
    public float buildInterval = 0.08f;       // 木材を消費して構築していくテンポ（秒）
    public GameObject flyingLogPrefab;        // 納品時に背中から飛んでいく木材のプレハブ

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

            // 円形UIの初期化
            UpdateRadialUI(item);
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

                    // 円形UIの進捗を更新
                    UpdateRadialUI(targetItem);

                    // 納品位置の決定
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

                        // 完成したら円形UIを非表示にする
                        if (targetItem.sliderCanvasObject != null)
                        {
                            targetItem.sliderCanvasObject.SetActive(false);
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

    // 円形UI（Fill Amount）を更新するヘルパーメソッド
    private void UpdateRadialUI(ToggleItem item)
    {
        if (item.radialFillImage != null)
        {
            // 0〜1の割合に変換してFill Amountに代入
            float progress = (float)item.currentWoodCount / item.requiredWoodCount;
            item.radialFillImage.fillAmount = Mathf.Clamp01(progress);
        }

        // すでに完成している場合はUIを非表示にする
        if (item.isActive && item.sliderCanvasObject != null)
        {
            item.sliderCanvasObject.SetActive(false);
        }
    }

    // 背中から持っていた木材が、指定した納品場所へ飛んでいって消える演出
    private IEnumerator FlyLogToBuildPoint(GameObject logObj, Vector3 targetPos, ToggleItem item)
    {
        logObj.transform.SetParent(null);

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

            Vector3 currentPos = Vector3.Lerp(startPos, targetPos, t);
            currentPos.y += Mathf.Sin(t * Mathf.PI) * 1.0f;

            logObj.transform.position = currentPos;
            yield return null;
        }

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