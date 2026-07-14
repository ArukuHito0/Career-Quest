using CareerQuest.Core;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace CareerQuest.Enemy
{
    [DisallowMultipleComponent]
    public abstract class EnemyManagerBase<T> : MonoBehaviour where T : Component
    {
        protected TreasureHashManager _treasureHashManager;  // 宝物のグリッドマップ管理クラス
        protected EnemyHashManager _enemyHashManager; // 敵のグリッドマップ管理クラス

        protected List<Test_Treasuer> treasureEntities = new List<Test_Treasuer>();
        protected List<EnemyContoroller> enemyEntities = new List<EnemyContoroller>();
        protected NativeArray<EnemyData> enemyDatas;

        [SerializeField] EnemyID _enemyID = EnemyID.Golem;
        [SerializeField] EnemyStatHolder _enemyStatHolder;  // ステータス保持SO

        EnemyStat enemyStat;  // 敵のパラメーター(キャッシュ用)
        protected int hp;  // 体力
        protected float moveSpeed;  // 移動速度
        protected float searchRadius;  // 状況把握できる範囲の半径

        protected virtual void Awake()
        {
            _treasureHashManager = ServiceLocator.Resolve<TreasureHashManager>();
            _enemyHashManager = ServiceLocator.Resolve<EnemyHashManager>();

            enemyStat = _enemyStatHolder.GetStat(_enemyID);
            hp = enemyStat.HP;
            moveSpeed = enemyStat.MoveSpeed;
            searchRadius = enemyStat.SearchRadius;
        }
        protected virtual void Start()
        {
            treasureEntities = _treasureHashManager.Entities;
            enemyEntities = _enemyHashManager.Entities;

            enemyDatas = new NativeArray<EnemyData>(enemyEntities.Count, Allocator.Persistent);
        }

        protected virtual void OnDestroy()
        {
            if (enemyDatas.IsCreated) enemyDatas.Dispose();
        }
    }
}