using UnityEngine;

public class PlayerLogCollector : MonoBehaviour
{
    private PlayerInventory inventory;
    private PlayerStateManager stateManager;

    void Start()
    {
        inventory = GetComponent<PlayerInventory>();
        stateManager = GetComponent<PlayerStateManager>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        // 護衛モード中、または着替え中などの場合は木材を拾えないようにする
        if (stateManager != null && !stateManager.IsCarryMode())
        {
            return;
        }

        if (collision.gameObject.CompareTag("Log"))
        {
            LogItem logItem = collision.gameObject.GetComponent<LogItem>();
            if (logItem != null && !logItem.isCollected)
            {
                logItem.isCollected = true;

                if (inventory != null)
                {
                    inventory.AddStackedLog(collision.gameObject);
                }
            }
        }
    }
}