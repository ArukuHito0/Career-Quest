using UnityEngine;
using System.Collections;

public class TreeController : MonoBehaviour
{
    [Header("木の耐久度設定")]
    public int maxHealth = 3;             // 倒すまでに必要な削り回数
    public float chopInterval = 0.4f;     // 削るテンポ（秒）
    public GameObject logPrefab;          // 飛ばす木材のプレハブ

    [Header("倒れる演出設定")]
    public Transform fallTarget;
    public float fallDuration = 0.8f;
    public AnimationCurve fallCurve = AnimationCurve.Linear(0, 0, 1, 1);
    public GameObject stumpPrefab;        // 切り株のプレハブ

    private int currentHealth;
    private bool isPlayerInside = false;
    private bool isFalling = false;
    private Coroutine chopCoroutine;

    void Start()
    {
        currentHealth = maxHealth;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isFalling) return;

        // プレイヤーが触れたら削るコルーチンを開始
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

    // プレイヤーが触れている間、一定間隔で木を削って木材をドロップさせる
    IEnumerator ChopRoutine()
    {
        while (isPlayerInside && currentHealth > 0 && !isFalling)
        {
            yield return new WaitForSeconds(chopInterval);

            // 木材プレハブを生成して周囲に弾き飛ばす
            DropLog();

            currentHealth--;

            // 耐久度が0になったら木を倒す
            if (currentHealth <= 0)
            {
                yield return StartCoroutine(FallSequence());
                yield break;
            }
        }
        chopCoroutine = null;
    }

    void DropLog()
    {
        if (logPrefab != null)
        {
            Vector3 spawnPos = transform.position + Vector3.up * 1.0f;
            GameObject log = Instantiate(logPrefab, spawnPos, Quaternion.identity);

            Rigidbody rb = log.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // ランダムな方向にポンと跳ね飛ばす
                Vector3 randomDir = new Vector3(Random.Range(-1f, 1f), 1.5f, Random.Range(-1f, 1f)).normalized;
                rb.AddForce(randomDir * 4.0f, ForceMode.Impulse);
            }
        }
    }

    // 木が倒れる演出
    IEnumerator FallSequence()
    {
        isFalling = true;

        Vector3 directionToTarget = fallTarget != null
            ? (fallTarget.position - transform.position).normalized
            : transform.forward;

        Quaternion startRotation = transform.rotation;
        Quaternion targetRotation = Quaternion.LookRotation(directionToTarget) * Quaternion.Euler(90, 0, 0);

        float elapsed = 0f;
        while (elapsed < fallDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fallDuration;
            float curveValue = fallCurve.Evaluate(t);

            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, curveValue);
            yield return null;
        }

        // 切り株の生成
        if (stumpPrefab != null)
        {
            GameObject stump = Instantiate(stumpPrefab, transform.position, transform.rotation);
            stump.transform.localScale = transform.localScale;
        }

        // 木本体を消去
        Destroy(gameObject);
    }
}