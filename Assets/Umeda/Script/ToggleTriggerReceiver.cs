using UnityEngine;

public class ToggleTriggerReceiver : MonoBehaviour
{
    [HideInInspector] public ObjectToggler objectToggler;
    [HideInInspector] public string targetName;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // プレイヤーがインベントリーを持っていて木材を持っているか確認
            if (other.TryGetComponent<PlayerInventory>(out var inventory) && inventory.woodCount > 0)
            {
                if (objectToggler != null)
                {
                    // 該当するアイテムを有効化し、木材を消費する
                    bool success = objectToggler.ActivateItemByName(targetName);
                    if (success)
                    {
                        inventory.ConsumeWood(1);
                        Debug.Log($"{targetName} を有効化しました！");
                    }
                }
            }
        }
    }
}