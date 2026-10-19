using System.Collections;
using UnityEngine;

public class Tree : MonoBehaviour
{
    [Header("木の耐久度とドロップ設定")]
    public int maxHealth = 5;
    public float chopInterval = 0.3f; // 削る間隔（秒）
    public GameObject logPrefab;      // ドロップさせる木材のプレハブ

    private int currentHealth;
    private bool isPlayerInside = false;
    private Coroutine chopCoroutine;

    void Start()
    {
        currentHealth = maxHealth;
    }

    private void OnTriggerEnter(Collider other)
    {
        // プレイヤーが近づいたら削るコルーチン開始
        if (other.CompareTag("Player") || other.TryGetComponent<PlayerInventory>(out _))
        {
            isPlayerInside = true;
            if (chopCoroutine == null)
            {
                chopCoroutine = StartCoroutine(ChopRoutine());
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") || other.GetComponent<PlayerInventory>() != null)
        {
            isPlayerInside = false;
            if (chopCoroutine != null)
            {
                StopCoroutine(chopCoroutine);
                chopCoroutine = null;
            }
        }
    }

    private IEnumerator ChopRoutine()
    {
        while (isPlayerInside && currentHealth > 0)
        {
            yield return new WaitForSeconds(chopInterval);
            CutTree();
        }
        chopCoroutine = null;
    }

    void CutTree()
    {
        currentHealth--;

        // 木材プレハブを生成して地面にドロップさせる
        if (logPrefab != null)
        {
            Vector3 spawnPos = transform.position + Vector3.up * 1.0f;
            GameObject log = Instantiate(logPrefab, spawnPos, Quaternion.identity);

            Rigidbody rb = log.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // ランダムな方向にポンと跳ね飛ばす
                Vector3 randomDir = new Vector3(Random.Range(-1f, 1f), 1.5f, Random.Range(-1f, 1f)).normalized;
                rb.AddForce(randomDir * 3.5f, ForceMode.Impulse);
            }
        }

        // 耐久度が0になったら木を非表示（または消去）にする
        if (currentHealth <= 0)
        {
            gameObject.SetActive(false);
        }
    }
}