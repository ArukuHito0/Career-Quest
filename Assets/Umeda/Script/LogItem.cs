using UnityEngine;

public class LogItem : MonoBehaviour
{
    [HideInInspector]
    public bool isCollected = false;

    private void OnTriggerEnter(Collider other)
    {
        // プレイヤーが近づいてきたら回収を試みる
        if (other.CompareTag("Player") && !isCollected)
        {
            PlayerStateManager stateManager = other.GetComponent<PlayerStateManager>();
            PlayerInventory inventory = other.GetComponent<PlayerInventory>();

            // 運搬モード（Carry）のときだけ回収する
            if (stateManager != null && stateManager.IsCarryMode())
            {
                if (inventory != null)
                {
                    isCollected = true;
                    inventory.AddStackedLog(gameObject);
                }
            }
        }
    }
}