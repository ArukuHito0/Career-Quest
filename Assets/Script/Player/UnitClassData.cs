using UnityEngine;

// ユニットの攻撃方法
public enum UnitAttackType
{
    Melee,  // 近接攻撃
    Ranged, // 遠距離攻撃
}

// クラスごとの設定データを作成できるようにする
[CreateAssetMenu(
    fileName = "NewUnitCless",
    menuName = "Units/Unit Class Data"
    )]
public class UnitClassData : ScriptableObject
{
    [Header("クラス情報")]
    public string className = "Fighter";
    [TextArea] public string description;

    [Header("基本ステータス")]
    [Min(1)] public int maxHealth = 100;
    [Min(0)] public int attackPower = 20;
    [Min(0.1f)] public float attackInterval = 1.0f;
    [Min(0.1f)] public float attackRange = 2.0f;
    [Min(0f)] public float moveSpeed = 3.5f;

    [Header("攻撃設定")]
    public UnitAttackType attackType = UnitAttackType.Melee;

    [Tooltip("遠距離攻撃で使用する弾のPrefab")]
    public GameObject projectilePrefab;

    [Tooltip("攻撃時に表示するエフェクトPrefab")]
    public GameObject attackEffectPrefab;
}