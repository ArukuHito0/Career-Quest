using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    // この攻撃が与えるダメージ
    [SerializeField] private int damage = 20;

    // ダメージを外部から取得
    public int Damage => damage;
}