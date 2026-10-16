using CareerQuest.Core;
using CareerQuest.Enemy;
using System.Collections.Generic;
using UnityEngine;

public class TreasureChest : MonoBehaviour ,ISpatialEntity
{
    [SerializeField] private int maxHealth = 100;
    private int currentHealth;

    TreasureHashManager _hashManager;
    public List<int> nearbyEntities = new List<int>(64);

    public int Index { get; set; }  // この宝物の番号(一意)
    public float Tickness { get; set; }  // オブジェクトの厚さ

    // 現在HPを外部から取得
    public int CurrentHealth => currentHealth;

    // 最大HPを外部から取得
    public int MaxHealth => maxHealth;

    // HPが0になったことを通知済みかどうか
    private bool gameOverNotified;

    void Awake()
    {
        // 最大HPを現在HPに設定
        currentHealth = maxHealth;

        _hashManager = ServiceLocator.Resolve<TreasureHashManager>();
        _hashManager.Register(this);
        Tickness = 0.2f;
    }
    void Update()
    {
        // ゲームオーバー通知後は更新を行わない
        if (gameOverNotified)
            return;

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
            {
                continue;
            }
            var otherEnemy = _hashManager.ActiveEntities[index];

            //float dist = Vector3.Distance(transform.position, otherEnemy.transform.position);
        }
    }

    // お宝にダメージを与える
    public void TakeDamage(int damage)
    {
        // 0以下のダメージや、HPがなくなった後のダメージは無視する
        if (damage <= 0 || currentHealth <= 0)
            return;

        // HPを減らし、0未満にならないようにする
        currentHealth = Mathf.Max(0, currentHealth - damage);

        // HPが0になったらゲームオーバーを通知する
        if (currentHealth <= 0)
        {
            NotifyGameOver();
        }
    }

    // ゲームオーバーを一度だけ通知する
    private void NotifyGameOver()
    {
        if (gameOverNotified)
            return;

        gameOverNotified = true;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver();
        }
        else
        {
            Debug.LogError("GameManagerが見つかりません。シーン内にGameManagerを配置してください。");
        }
    }
}
