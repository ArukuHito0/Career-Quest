using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BearTrapController : MonoBehaviour
{
    [System.Serializable]
    public class TrapPart
    {
        [Tooltip("動かす対象のパーツのTransform")]
        public Transform targetTransform;

        [Header("ローカル座標での動き設定")]
        [Tooltip("発動時のローカル位置（オフセット、または目標ローカル座標）")]
        public Vector3 targetLocalPosition;
        [Tooltip("発動時のローカル回転（オイラー角）")]
        public Vector3 targetLocalRotationEuler;

        [Header("タイミング設定")]
        [Tooltip("動きにかかる時間（秒）")]
        [Min(0.001f)]
        public float duration = 0.3f;
        [Tooltip("動きの緩急（イージングカーブ）")]
        public AnimationCurve easeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    }

    [Header("トラばさみ設定")]
    [Tooltip("インスペクター上でチェックを入れると発動します（プレイ中のみ反応）")]
    [SerializeField] private bool isTriggered = false;

    [Tooltip("動かすパーツのリスト")]
    [SerializeField] private List<TrapPart> trapParts = new List<TrapPart>();

    // 各パーツごとの「閉じる前の初期ローカル位置・回転」を保持する字典
    private Dictionary<Transform, LocalTransformData> initialTransforms = new Dictionary<Transform, LocalTransformData>();
    private bool previousTriggerState = false;
    private Coroutine currentAnimationCoroutine;

    private struct LocalTransformData
    {
        public Vector3 localPosition;
        public Quaternion localRotation;
    }

    private void Start()
    {
        // 各パーツのゲーム開始時のローカル位置・回転を記録
        foreach (var part in trapParts)
        {
            if (part.targetTransform != null && !initialTransforms.ContainsKey(part.targetTransform))
            {
                initialTransforms[part.targetTransform] = new LocalTransformData
                {
                    localPosition = part.targetTransform.localPosition,
                    localRotation = part.targetTransform.localRotation
                };
            }
        }
        previousTriggerState = isTriggered;

        if (isTriggered)
        {
            SetTrapStateImmediate(true);
        }
    }

    private void OnValidate()
    {
        // プレイ中にインスペクターのチェックボックスが変更されたとき
        if (Application.isPlaying)
        {
            if (isTriggered != previousTriggerState)
            {
                previousTriggerState = isTriggered;
                TriggerTrap(isTriggered);
            }
        }
    }

    public void SetTriggered(bool triggered)
    {
        if (isTriggered != triggered)
        {
            isTriggered = triggered;
            previousTriggerState = isTriggered;
            TriggerTrap(isTriggered);
        }
    }

    private void TriggerTrap(bool triggered)
    {
        if (currentAnimationCoroutine != null)
        {
            StopCoroutine(currentAnimationCoroutine);
        }
        currentAnimationCoroutine = StartCoroutine(AnimateTrapRoutine(triggered));
    }

    private IEnumerator AnimateTrapRoutine(bool triggered)
    {
        var animDataList = new List<PartAnimationData>();

        foreach (var part in trapParts)
        {
            if (part.targetTransform == null) continue;
            if (!initialTransforms.ContainsKey(part.targetTransform)) continue;

            var initData = initialTransforms[part.targetTransform];

            var data = new PartAnimationData();
            data.transform = part.targetTransform;
            data.part = part;

            data.startLocalPos = part.targetTransform.localPosition;
            data.startLocalRot = part.targetTransform.localRotation;

            // 開いているときは初期位置、閉じているときは「初期位置 ＋ 設定した目標ローカル値」に移動
            data.targetLocalPos = triggered ? (initData.localPosition + part.targetLocalPosition) : initData.localPosition;
            data.targetLocalRot = triggered ? (initData.localRotation * Quaternion.Euler(part.targetLocalRotationEuler)) : initData.localRotation;

            animDataList.Add(data);
        }

        // 最も長い時間を全体の基準とする
        float maxDuration = 0f;
        foreach (var data in animDataList)
        {
            if (data.part.duration > maxDuration)
                maxDuration = data.part.duration;
        }

        if (maxDuration <= 0f) maxDuration = 0.001f;

        float elapsedTime = 0f;

        while (elapsedTime < maxDuration)
        {
            elapsedTime += Time.deltaTime;

            foreach (var data in animDataList)
            {
                float t = Mathf.Clamp01(elapsedTime / data.part.duration);
                float evaluatedT = data.part.easeCurve.Evaluate(t);

                data.transform.localPosition = Vector3.Lerp(data.startLocalPos, data.targetLocalPos, evaluatedT);
                data.transform.localRotation = Quaternion.Slerp(data.startLocalRot, data.targetLocalRot, evaluatedT);
            }

            yield return null;
        }

        // 最後に完全に目標値へ合わせる
        foreach (var data in animDataList)
        {
            data.transform.localPosition = data.targetLocalPos;
            data.transform.localRotation = data.targetLocalRot;
        }
    }

    private void SetTrapStateImmediate(bool triggered)
    {
        foreach (var part in trapParts)
        {
            if (part.targetTransform == null) continue;
            if (!initialTransforms.ContainsKey(part.targetTransform)) continue;

            var initData = initialTransforms[part.targetTransform];

            Vector3 targetPos = triggered ? (initData.localPosition + part.targetLocalPosition) : initData.localPosition;
            Quaternion targetRot = triggered ? (initData.localRotation * Quaternion.Euler(part.targetLocalRotationEuler)) : initData.localRotation;

            part.targetTransform.localPosition = targetPos;
            part.targetTransform.localRotation = targetRot;
        }
    }

    private class PartAnimationData
    {
        public Transform transform;
        public TrapPart part;
        public Vector3 startLocalPos;
        public Vector3 targetLocalPos;
        public Quaternion startLocalRot;
        public Quaternion targetLocalRot;
    }
}