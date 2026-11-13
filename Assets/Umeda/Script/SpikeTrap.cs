using UnityEngine;
using System.Collections;

public class SpikeTrap : MonoBehaviour
{
    [Header("トラップ設定")]
    [Tooltip("チェックをオンにするとトラップが1回作動します（終了後に自動でオフになります）")]
    public bool isTrapActive = false; // ① ゲームスタート時はオフ

    [Header("参照設定")]
    [Tooltip("動かすトゲのオブジェクトのTransform")]
    [SerializeField] private Transform spikeTransform;

    [Header("位置・時間設定")]
    [Tooltip("収まった時のローカル座標（※Move Offsetとしての座標）")]
    [SerializeField] private Vector3 retractedLocalPosition = new Vector3(0f, -1.0f, 0f);

    [Tooltip("飛び出る・戻るのにかかる時間（秒）")]
    [SerializeField] private float moveDuration = 0.2f;
    [Tooltip("④ とげが出た状態を維持する時間（秒）")]
    [SerializeField] private float stayUpDuration = 1.0f;
    [Tooltip("⑦ 動作終了後のクールタイム（秒）")]
    [SerializeField] private float cooldownDuration = 2.0f;

    private Vector3 extendedPosition;      // 出ている状態の位置（エディター上で配置した初期位置）
    private Vector3 targetRetractedPos;    // 収まった状態の位置
    private bool isRunning = false;        // 動作中フラグ
    private bool isInCooldown = false;     // クールタイム中フラグ

    private void Start()
    {
        if (spikeTransform != null)
        {
            // エディター上で配置した位置を「とげが出ている状態（初期位置）」として記憶
            extendedPosition = spikeTransform.localPosition;

            // インスペクターで設定した「収まった時の座標」をセット
            targetRetractedPos = retractedLocalPosition;

            // ① ゲームスタート時にチェックをオフにし、スタートと同時にとげを「収まった状態」にする
            isTrapActive = false;
            spikeTransform.localPosition = targetRetractedPos;
        }
        else
        {
            Debug.LogError("【エラー】SpikeTransform がアタッチされていません！", this);
        }
    }

    private void Update()
    {
        // クールタイム中に無理やりチェックをオンにされた場合は、自動でオフに戻す
        if (isInCooldown && isTrapActive)
        {
            isTrapActive = false;
            return;
        }

        // ② インスペクターでチェックがオンにされた時（かつ動作中でなく、クールタイムでもない場合）
        if (isTrapActive && !isRunning && !isInCooldown)
        {
            StartCoroutine(TrapSequenceRoutine());
        }
    }

    private IEnumerator TrapSequenceRoutine()
    {
        isRunning = true;

        // ③ とげが出る（収まった位置から、エディター上で置いてあった「出ている位置」へ）
        yield return MoveSpike(targetRetractedPos, extendedPosition, moveDuration);

        // ④ クールタイム（出た状態の維持時間）
        yield return new WaitForSeconds(stayUpDuration);

        // ⑤ とげが収まる（出ている位置から「収まった位置」へ戻る）
        yield return MoveSpike(extendedPosition, targetRetractedPos, moveDuration);

        // ⑥ インスペクターのチェックが自動でオフになる
        isTrapActive = false;
        isRunning = false;

        // ⑦ クールタイム（次の作動までの待機時間）
        isInCooldown = true;
        yield return new WaitForSeconds(cooldownDuration);

        // ⑧ 再びチェックをオンにできるようになる（②が可能に）
        isInCooldown = false;
    }

    private IEnumerator MoveSpike(Vector3 fromPos, Vector3 toPos, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            spikeTransform.localPosition = Vector3.Lerp(fromPos, toPos, t);
            yield return null;
        }
        spikeTransform.localPosition = toPos;
    }
}