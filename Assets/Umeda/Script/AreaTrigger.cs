using UnityEngine;

// トリガーとなるコライダーオブジェクトに自動アタッチされる補助クラス
public class AreaTrigger : MonoBehaviour
{
    private ObjectToggler objectToggler;
    private string targetItemName;

    public void Initialize(ObjectToggler toggler, string itemName)
    {
        objectToggler = toggler;
        targetItemName = itemName;
    }

    private void OnTriggerEnter(Collider other)
    {
        // 接触したオブジェクトが PlayerInventory を持っているか判定
        if (other.TryGetComponent<PlayerInventory>(out var playerInventory))
        {
            if (objectToggler != null)
            {
                objectToggler.TryActivateItem(targetItemName, playerInventory);
            }
        }
    }
}