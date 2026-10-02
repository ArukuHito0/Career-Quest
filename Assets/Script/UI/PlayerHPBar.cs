using UnityEngine;
using UnityEngine.UI;

public class PlayerHPBar : MonoBehaviour
{
    // HPを表示するslider
    [SerializeField] private Slider hpSlider;

    // プレイヤーのHP管理
    private PlayerHealth playerHealth;

    private void Awake()
    {
        // 親にあるPlayerHealthを取得
        playerHealth = GetComponentInParent<PlayerHealth>();

        if (playerHealth == null)
        {
            Debug.Log($"{gameObject.name}: PlayerHealthが見つかりません");
            return;
        }

        // HPバーの最大値を設定
        hpSlider.maxValue = playerHealth.MaxHealth;

        // 現在HPを設定
        hpSlider.value = playerHealth.CurrentHealth;

    }


    private void Update()
    {
        if (playerHealth == null && hpSlider)
            return;

        // 現在HPをHPバーに反映
        hpSlider.value = playerHealth.CurrentHealth;
    }
}
