using UnityEngine;

public class TreasureHPBar : HPBarBase
{
    // ‚¨•ó‚ÌHPŠÇ—
    private TreasureChest treasureChest;

    protected override void FindHealth()
    {
        // TreasureHealth‚ğæ“¾
        treasureChest = GetComponent<TreasureChest>();
    }

    protected override int GetMaxHealth()
    {
        if (treasureChest == null)
            return 0;

        return treasureChest.MaxHealth;
    }

    protected override int GetCurrentHealth()
    {
        if (treasureChest == null)
            return 0;

        return treasureChest.CurrentHealth;
    }
}