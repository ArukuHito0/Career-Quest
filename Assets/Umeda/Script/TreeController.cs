using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class TreeController : MonoBehaviour
{
    [Header("倒れる設定")]
    public Transform fallTarget;
    public float fallDuration = 1.5f;     // 倒れるまでの時間
    public AnimationCurve fallCurve = AnimationCurve.Linear(0, 0, 1, 1); // 動きのカーブ

    public GameObject stumpPrefab;

    private bool isFalling = false;

    void Update()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform == this.transform)
                {
                    Debug.Log("木がクリックされました！");
                    if (!isFalling) StartCoroutine(FallSequence());
                }
            }
        }
    }

    IEnumerator FallSequence()
    {
        isFalling = true;

        Vector3 directionToTarget = (fallTarget.position - transform.position).normalized;
        Quaternion startRotation = transform.rotation;

        // 倒れる方向への回転
        Quaternion targetRotation = Quaternion.LookRotation(directionToTarget) * Quaternion.Euler(90, 0, 0);

        float elapsed = 0f;
        while (elapsed < fallDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fallDuration;

            // カーブを使って自然な加速・減速を反映
            float curveValue = fallCurve.Evaluate(t);

            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, curveValue);
            yield return null;
        }

        // 幹の切り替え処理
        if (stumpPrefab != null)
        {
            // 元の木のTransform（位置と回転）をそのまま引き継ぐ
            GameObject stump = Instantiate(stumpPrefab, transform.position, transform.rotation);

            // もし幹のスケールも元の木と同じにしたい場合は以下を追加
            stump.transform.localScale = transform.localScale;
        }

        // 元のオブジェクトを削除
        Destroy(gameObject);
    }
}