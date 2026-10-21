using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [Header("スタック設定")]
    public Transform stackPoint;        // 背中の積み上げ起点
    public float logHeight = 0.25f;     // 木材1本あたりの厚み
    public float flyDuration = 0.25f;   // 積み上がる時の時間

    [HideInInspector]
    public List<GameObject> stackedLogs = new List<GameObject>();

    public int woodCount => stackedLogs.Count;

    private PlayerStateManager stateManager;

    void Start()
    {
        stateManager = GetComponent<PlayerStateManager>();
    }

    void Update()
    {
        // 運搬モード（Carry）ではない、または着替え中・お化け状態などの場合、持てないため全ドロップ
        if (stateManager != null)
        {
            if (!stateManager.IsCarryMode() || stateManager.IsChanging() || !stateManager.IsAlive())
            {
                if (stackedLogs.Count > 0)
                {
                    DropAllLogs();
                }
            }
        }
    }

    // 背中に木材を追加
    public void AddStackedLog(GameObject logObj)
    {
        if (stateManager != null && !stateManager.IsCarryMode())
        {
            DropSingleLog(logObj);
            return;
        }

        Rigidbody rb = logObj.GetComponent<Rigidbody>();
        if (rb != null) Destroy(rb);

        Collider col = logObj.GetComponent<Collider>();
        if (col != null) col.enabled = false;

        logObj.transform.SetParent(stackPoint);

        int currentCount = stackedLogs.Count;
        stackedLogs.Add(logObj);

        StartCoroutine(AnimateLogToStack(logObj, currentCount));
    }

    private System.Collections.IEnumerator AnimateLogToStack(GameObject logObj, int index)
    {
        Vector3 startPos = logObj.transform.localPosition;
        Quaternion startRot = logObj.transform.localRotation;
        Vector3 targetPos = new Vector3(0, index * logHeight, 0);

        float elapsed = 0f;
        while (elapsed < flyDuration)
        {
            // 【重要】アニメーション中にドロップされて親が変わっていたら、即座に中断する
            if (logObj == null || logObj.transform.parent != stackPoint) yield break;

            elapsed += Time.deltaTime;
            float t = elapsed / flyDuration;

            Vector3 currentLocal = Vector3.Lerp(startPos, targetPos, t);
            currentLocal.y += Mathf.Sin(t * Mathf.PI) * 0.3f;

            logObj.transform.localPosition = currentLocal;
            logObj.transform.localRotation = Quaternion.Slerp(startRot, Quaternion.identity, t);
            yield return null;
        }

        if (logObj != null && logObj.transform.parent == stackPoint)
        {
            logObj.transform.localPosition = targetPos;
            logObj.transform.localRotation = Quaternion.identity;
        }
    }

    // 保持している木材をすべて地面にドロップさせる
    public void DropAllLogs()
    {
        while (stackedLogs.Count > 0)
        {
            GameObject logObj = stackedLogs[stackedLogs.Count - 1];
            stackedLogs.RemoveAt(stackedLogs.Count - 1);

            DropSingleLog(logObj);
        }
        Debug.Log("モード変更または状態異常のため、すべての木材をドロップしました！");
    }

    // 1つの木材をプレイヤーの足元に物理挙動つきで吐き出す
    private void DropSingleLog(GameObject logObj)
    {
        if (logObj == null) return;

        // 現在の正確なワールド座標と回転を保持
        Vector3 currentWorldPos = logObj.transform.position;
        Quaternion currentWorldRot = logObj.transform.rotation;

        // 親子関係を解除
        logObj.transform.SetParent(null);

        // 正しいワールド座標を再適用
        logObj.transform.position = currentWorldPos;
        logObj.transform.rotation = currentWorldRot;

        // コライダーとRigidbodyを再有効化して地面に落とす
        Collider col = logObj.GetComponent<Collider>();
        if (col != null) col.enabled = true;

        Rigidbody rb = logObj.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = logObj.AddComponent<Rigidbody>();
        }
        rb.isKinematic = false;

        // 回収済みフラグをリセット（また拾えるようにする）
        if (logObj.TryGetComponent<LogItem>(out var logItem))
        {
            logItem.isCollected = false;
        }

        // プレイヤーの前方へ放り出す
        Vector3 dropForce = (transform.forward + Vector3.up * 0.8f).normalized * 4.0f;
        rb.AddForce(dropForce, ForceMode.Impulse);
    }

    public GameObject PopLog()
    {
        if (stackedLogs.Count == 0) return null;

        int lastIndex = stackedLogs.Count - 1;
        GameObject topLog = stackedLogs[lastIndex];
        stackedLogs.RemoveAt(lastIndex);
        return topLog;
    }

    public bool ConsumeWood(int amount = 1)
    {
        if (stackedLogs.Count >= amount)
        {
            for (int i = 0; i < amount; i++)
            {
                GameObject log = PopLog();
                if (log != null)
                {
                    Destroy(log);
                }
            }
            return true;
        }
        return false;
    }
}