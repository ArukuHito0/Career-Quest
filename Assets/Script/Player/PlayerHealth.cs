using CareerQuest.Core;
using CareerQuest.Enemy;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour, ISpatialEntity
{
    // ユニットの状態管理
    private PlayerStateManager stateManager;

    PlayerHashManager _hashManager;
    public List<int> nearbyEntities = new List<int>(64);

    // 最大HP
    [SerializeField] private int maxHealth = 100;

    // お化け状態になっている時間
    [SerializeField] private float ghostDuration = 5f;

    // 現在HP
    private int currentHealth;

    // お化け状態か
    private bool isGhost;

    // 現在HPを外部から取得
    public int CurrentHealth => currentHealth;

    // 最大HPを外部から取得
    public int MaxHealth => maxHealth;

    // お化け状態を取得
    public bool IsGhost => isGhost;

    public int Index { get; set; }
    public float Tickness { get; set; }


    void Awake()
    {
        stateManager = GetComponent<PlayerStateManager>();

        // ゲーム開始時は最大HP
        currentHealth = maxHealth;
        _hashManager = ServiceLocator.Resolve<PlayerHashManager>();
        _hashManager.Register(this);
        Tickness = 0.2f;
    }

    private void Update()
    {
        nearbyEntities.Clear();

        int myX = Mathf.FloorToInt(transform.position.x / _hashManager.cellSize);
        int myZ = Mathf.FloorToInt(transform.position.z / _hashManager.cellSize);
        int myCellId = myX + (myZ * _hashManager.girdWidth);

        for (int dz = -1; dz <= 1; dz++)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                int targetCellId = (myX + dx) + ((myZ + dz) * _hashManager.girdWidth);

                _hashManager.GetEntitiesInCell(targetCellId, nearbyEntities);
            }
        }

        foreach (int index in nearbyEntities)
        {
            if (_hashManager.ActiveEntities[index] == this)
                continue;

            var otherEnemy = _hashManager.ActiveEntities[index];

            float dist = Vector3.Distance(transform.position, otherEnemy.transform.position);
        }
    }

    // 敵の攻撃に当たった時
    private void OnTriggerEnter(Collider other)
    {
        // お化け状態なら攻撃を受けない
        if (isGhost)
            return;

        // 当たったオブジェクトにEnemyAttackがあるか確認
        EnemyAttack enemyAttack = other.GetComponent<EnemyAttack>();

        // 敵の攻撃でなければ何もしない
        if (enemyAttack == null)
            return;

        // 敵の攻撃力を取得してダメージを受ける
        TakeDamage(enemyAttack.Damage);
    }


    // ダメージを受ける処理
    public void TakeDamage(int damage)
    {
        // お化け状態になっている場合は処理しない
        if (isGhost)
            return;

        //0以下のダメージは無視
        if (damage <= 0)
            return;

        // HPを減らす
        currentHealth -= damage;

        // HPが0未満にならないようにする
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        Debug.Log($"{gameObject.name}ダメージ:{damage} / HP:{currentHealth}/{maxHealth}");

        // HPが0ならお化け状態になる
        if (currentHealth <= 0)
            StartCoroutine(GhostState());
    }

    // 回復処理
    public void Heal(int amount)
    {
        if (isGhost)
            return;

        if (amount <= 0)
            return;

        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
    }

    // お化け状態
    private IEnumerator GhostState()
    {
        // 二重実行防止
        if (isGhost)
            yield break;

        isGhost = true;

        stateManager.SetGhost();

        Debug.Log($"{gameObject.name}がお化け状態になりました");

        // 指定時間待つ
        yield return new WaitForSeconds(ghostDuration);

        // 復活
        Revive();
    }


    // 復活
    private void Revive()
    {
        // HPを最大まで回復
        currentHealth = maxHealth;

        // 通常状態へ戻す
        isGhost = false;
        stateManager.SetAlive();

        Debug.Log($"{gameObject.name}が復活しました");
    }
}
