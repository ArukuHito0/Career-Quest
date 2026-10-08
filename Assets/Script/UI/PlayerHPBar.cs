using UnityEngine;

public class PlayerHPBar : HPBarBase
{
    // プレイヤーのHP管理
    private PlayerHealth playerHealth;

    protected override void FindHealth()
    {
        // 親にあるPlayerHealthを取得
        playerHealth = GetComponentInParent<PlayerHealth>();
    }

    protected override int GetMaxHealth()
    {
        if (playerHealth == null)
            return 0;

        return playerHealth.MaxHealth;
    }

    protected override int GetCurrentHealth()
    {
        if (playerHealth == null)
            return 0;

        return playerHealth.CurrentHealth;
    }
}