using UnityEngine;
using System.Collections;

public class ToggleTriggerReceiver : MonoBehaviour
{
    [HideInInspector] public ObjectToggler objectToggler;
    [HideInInspector] public string targetName;

    private Coroutine buildCoroutine;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // プレイヤーのインベントリを取得
            if (other.TryGetComponent<PlayerInventory>(out var inventory))
            {
                if (objectToggler != null)
                {
                    // ゾーンに入っている間、連続で木材を納品して橋を架けるコルーチンを開始
                    if (buildCoroutine != null) StopCoroutine(buildCoroutine);
                    buildCoroutine = StartCoroutine(objectToggler.StartBuildingRoutine(targetName, inventory));
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // ゾーンから出たら納品を一時停止
            if (buildCoroutine != null)
            {
                StopCoroutine(buildCoroutine);
                buildCoroutine = null;
            }
        }
    }
}