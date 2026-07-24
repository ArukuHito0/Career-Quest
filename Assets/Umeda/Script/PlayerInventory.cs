using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public int woodCount = 0; // 木材の所持数

    // 木材を追加するメソッド
    public void AddWood(int amount = 1)
    {
        woodCount += amount;
        Debug.Log($"木材を手に入れました！ 現在の木材数: {woodCount}");
    }

    // 木材を消費するメソッド
    public bool ConsumeWood(int amount = 1)
    {
        if (woodCount >= amount)
        {
            woodCount -= amount;
            Debug.Log($"木材を消費しました。 残りの木材数: {woodCount}");
            return true;
        }
        Debug.Log("木材が足りません！");
        return false;
    }
}