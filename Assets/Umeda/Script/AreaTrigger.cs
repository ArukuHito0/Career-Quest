using UnityEngine;

public class AreaTrigger : MonoBehaviour
{
    private ObjectToggler toggler;
    private string itemName;
    private Coroutine buildCoroutine;

    public void Initialize(ObjectToggler owner, string name)
    {
        toggler = owner;
        itemName = name;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PlayerInventory playerInventory))
        {
            if (toggler != null)
            {
                // エリアに入っている間、木材を自動で消費して組み立てるコルーチンを開始
                buildCoroutine = StartCoroutine(toggler.StartBuildingRoutine(itemName, playerInventory));
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<PlayerInventory>() != null)
        {
            // エリアから出たら納品を停止
            if (buildCoroutine != null)
            {
                StopCoroutine(buildCoroutine);
                buildCoroutine = null;
            }
        }
    }
}