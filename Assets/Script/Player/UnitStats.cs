using UnityEngine;
using UnityEngine.AI;

public class UnitStats : MonoBehaviour
{
    [Header("ユニットのクラス設定")]
    [SerializeField] private UnitClassData classData;

    private NavMeshAgent agent;

    // 他のスクリプトからクラスデータを参照するためのプロパティ
    public UnitClassData ClassData => classData;

    // クラスデータから各ステータスを取得する
    public int MaxHealth => classData != null ? classData.maxHealth : 100;
    public int AttackPower => classData != null ? classData.attackPower : 20;
    public float AttackInterval => classData != null ? classData.attackInterval : 1f;
    public float AttackRange => classData != null ? classData.attackRange : 2f;
    public float moveSpeed => classData != null ? classData.moveSpeed : 3.5f;

    public UnitAttackType AttackType => classData != null ? classData.attackType : UnitAttackType.Melee;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        ApplyClassSettings();
    }

    // クラスデータの設定をユニットを反映する
    public void ApplyClassSettings()
    {
        if (classData == null)
        {
            Debug.LogWarning($"{gameObject.name} にクラスデータが設定されていません", this);
            return;
        }

        // NavMeshAgentの移動速度をクラス設定に合わせる
        if (agent != null)
        {
            agent.speed = classData.moveSpeed;
        }
    }

    // クラスデータを変更する場合に使用する
    public void SetClassData(UnitClassData newClassData)
    {
        if (newClassData == null)
        {
            Debug.LogWarning("設定するクラスデータがありません。", this);
            return; 
        }

        classData = newClassData;
        ApplyClassSettings();
    }

    // クラスデータを取得する
    public UnitClassData GatClassData()
    {
        return classData;
    }
}
