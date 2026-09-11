using UnityEngine;
using System.Collections;

public class TreeController : MonoBehaviour
{
    [Header("倒れる設定")]
    public Transform fallTarget;
    public float fallDuration = 1.5f;     // 倒れるまでの時間
    public AnimationCurve fallCurve = AnimationCurve.Linear(0, 0, 1, 1); // 動きのカーブ

    public GameObject stumpPrefab;

    private bool isFalling = false;

    private void OnTriggerEnter(Collider other)
    {
        if (isFalling) return;

        if (other.CompareTag("Player"))
        {
            Debug.Log("プレイヤーが近づいたため、木が倒れます！");

            // プレイヤーのインベントリーに木材を追加
            if (other.TryGetComponent<PlayerInventory>(out var inventory))
            {
                inventory.AddWood(1);
            }
            else
            {
                Debug.LogWarning("プレイヤーに PlayerInventory コンポーネントが見つかりません。");
            }

            StartCoroutine(FallSequence());
        }
    }

    IEnumerator FallSequence()
    {
        isFalling = true;

        Vector3 directionToTarget = fallTarget != long.Equals(null, null) && fallTarget != null
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

        if (stumpPrefab != null)
        {
            GameObject stump = Instantiate(stumpPrefab, transform.position, transform.rotation);
            stump.transform.localScale = transform.localScale;
        }

        Destroy(gameObject);
    }
}