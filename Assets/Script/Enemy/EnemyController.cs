using UnityEngine;
using UAssert = UnityEngine.Assertions.Assert;
using System.Collections.Generic;
using CareerQuest.Core;

namespace CareerQuest.Enemy
{
    //  敵を制御するクラス
    [DisallowMultipleComponent]
    public sealed class EnemyContoroller : MonoBehaviour, ISpatialEntity
    {
        EnemyHashManager _enemyHashManager;  // 敵のグリッドマップマネージャー
        TreasureHashManager _treasureHashManager;  // 宝のグリッドマップマネージャー
        public List<int> nearEntities = new List<int>(64);

        [SerializeField] EnemyID _enemyID = EnemyID.Golem;
        [SerializeField] EnemyStatHolder _enemyStatHolder;  // ステータス保持SO
        
        EnemyStat _enemyStat;  // 敵のパラメーター(キャッシュ用)
        int _hp;  // 体力
        float _moveSpeed;  // 移動速度
        float _searchRadius;  // 状況把握できる範囲の半径

        public int Index { get; set; }  // 敵番号

        void Awake()
        {
            _treasureHashManager = ServiceLocator.Resolve<TreasureHashManager>();
            _enemyHashManager = ServiceLocator.Resolve<EnemyHashManager>();
            _enemyHashManager.Register(this);

            UAssert.IsNotNull(_enemyStatHolder, "EnemyStatHolderの参照が取得できませんでした。");
            _enemyStat = _enemyStatHolder.GetStat(_enemyID);
            _hp = _enemyStat.HP;
            _moveSpeed = _enemyStat.MoveSpeed;
            _searchRadius = _enemyStat.SearchRadius;
        }

        void Start()
        {
            InvokeRepeating(nameof(SearchTreasure), 0f, 0.5f);
        }

        //  周囲の宝物を探す
        void SearchTreasure()
        {
            nearEntities.Clear();

            int myX = Mathf.FloorToInt(transform.position.x / _treasureHashManager.cellSize);
            int myZ = Mathf.FloorToInt(transform.position.z / _treasureHashManager.cellSize);
            int myCellId = myX + (myZ * _treasureHashManager.girdWidth);

            for (int dz = -1; dz <= 1; dz++)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    int targetCellId = (myX + dx) + ((myZ + dz) * _treasureHashManager.girdWidth);

                    _treasureHashManager.GetEntitiesInCell(targetCellId, nearEntities);
                }
            }

            foreach (int index in nearEntities)
            {
                if (_treasureHashManager.Entities[index] == this) continue;

                var otherTreasure = _treasureHashManager.Entities[index];
                float dist = Vector3.Distance(transform.position, otherTreasure.transform.position);
                
                if (dist < _searchRadius)
                {
                    MyLogger.Log("Attack");
                }
            }
        }
    }
}