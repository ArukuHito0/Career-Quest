using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Pool;
using CareerQuest.Core;

namespace CareerQuest.Enemy
{
    [DisallowMultipleComponent]
    public abstract class EnemyManagerBase<T> : MonoBehaviour where T : Component
    {
        protected TreasureHashManager treasureHashManager;  // 宝物のグリッドマップ管理クラス
        protected EnemyHashManager enemyHashManager; // 敵のグリッドマップ管理クラス

        protected List<Test_Treasuer> activeTreasureEntities = new List<Test_Treasuer>();
        protected List<EnemyController> activeEnemyEntities = new List<EnemyController>();
        protected NativeArray<Vector3> wallPositions;

        [SerializeField] EnemyController _enemyPrefab;
        [SerializeField] protected int maxEnemyCount = 10;
        ObjectPool<EnemyController> _pool;

        protected NativeArray<EnemyData> bufferA;
        protected NativeArray<EnemyData> bufferB;
        protected bool isUsingBufferA = true;

        [SerializeField] EnemyID _enemyID = EnemyID.Golem;
        [SerializeField] EnemyStatHolder _enemyStatHolder;  // ステータス保持SO

        EnemyStat enemyStat;  // 敵のパラメーター(キャッシュ用)
        
        //  -- Golemステータス --  //
        protected int golemHp;                  // 体力
        protected float golemMoveSpeed;         // 移動速度
        protected float golemAttackRange;       // 移動速度
        protected float golemSearchRadius;      // 状況把握できる範囲の半径
        protected int golemAttackPower;         // 攻撃力
        protected float golemWallAvoidRadius;   // 壁を避け始める距離
        protected float golemEnemyAvoidRadius;  // 敵を避け始める距離
        protected float golemBodyTickness;      // 体の厚さ

        //  -- Ghostステータス --  //
        protected int ghostHp;                  // 体力
        protected float ghostMoveSpeed;         // 移動速度
        protected float ghostAttackRange;       // 移動速度
        protected float ghostSearchRadius;      // 状況把握できる範囲の半径
        protected int ghostAttackPower;         // 攻撃力
        protected float ghostWallAvoidRadius;   // 壁を避け始める距離
        protected float ghostEnemyAvoidRadius;  // 敵を避け始める距離
        protected float ghostBodyTickness;      // 体の厚さ

        protected virtual void Awake()
        {
            treasureHashManager = ServiceLocator.Resolve<TreasureHashManager>();
            enemyHashManager = ServiceLocator.Resolve<EnemyHashManager>();

            bufferA = new NativeArray<EnemyData>(maxEnemyCount, Allocator.Persistent);
            bufferB = new NativeArray<EnemyData>(maxEnemyCount, Allocator.Persistent);

            _pool = new ObjectPool<EnemyController>(
            createFunc: () => Instantiate(_enemyPrefab),
            actionOnGet: e => e.gameObject.SetActive(true),
            actionOnRelease: e => e.gameObject.SetActive(false),
            actionOnDestroy: e => Destroy(e.gameObject),
            defaultCapacity: 100
        );

            SetStat();
        }

        protected virtual void Start()
        {
            activeTreasureEntities = treasureHashManager.ActiveEntities;
            activeEnemyEntities = enemyHashManager.ActiveEntities;

            var wallObjects = GameObject.FindGameObjectsWithTag("Wall");
            wallPositions = new NativeArray<Vector3>(wallObjects.Length, Allocator.Persistent);
            for (int i = 0; i < wallObjects.Length; i++)
            {
                wallPositions[i] = wallObjects[i].transform.position;
            }
        }

        protected virtual void OnDestroy()
        {
            if (bufferA.IsCreated) bufferA.Dispose();
            if (bufferB.IsCreated) bufferB.Dispose();
        }

        //  生成
        public void SpawnEnemy(Vector3 position)
        {
            EnsureBufferSize(activeEnemyEntities.Count + 1);

            MyLogger.Log("敵生成");
            var enemy = _pool.Get();
            enemy.transform.position = position;
        }

        //  削除
        //public void DespawnEnemy(EnemyController enemy)
        //{
        //    int indexToRemove = enemy.DataIndex;
        //    int lastIndex = activeEnemyEntities.Count - 1;

        //    if (indexToRemove < lastIndex)
        //    {
        //        var lastEnemy = activeEnemyEntities[lastIndex];
        //        activeEnemyEntities[indexToRemove] = lastEnemy;
        //        lastEnemy.DataIndex = indexToRemove;
        //        bufferA[indexToRemove] = bufferA[lastIndex];
        //        bufferB[indexToRemove] = bufferB[lastIndex];
        //    }

        //    activeEnemyEntities.RemoveAt(lastIndex);
        //    _pool.Release(enemy);
        //}

        // バッファをリサイズするメソッドを追加
        protected void EnsureBufferSize(int count)
        {
            if (bufferA.IsCreated && bufferA.Length >= count) return;

            if (bufferA.IsCreated) bufferA.Dispose();
            if (bufferB.IsCreated) bufferB.Dispose();

            int newSize = Mathf.Max(count, maxEnemyCount);
            bufferA = new NativeArray<EnemyData>(newSize, Allocator.Persistent);
            bufferB = new NativeArray<EnemyData>(newSize, Allocator.Persistent);
        }

        void SetStat()
        {
            //  -- ゴーレムの能力値設定
            enemyStat = _enemyStatHolder.GetStat(EnemyID.Golem);
            golemHp = enemyStat.HP;
            golemMoveSpeed = enemyStat.MoveSpeed;
            golemAttackRange = enemyStat.AtackRange;
            golemSearchRadius = enemyStat.SearchRadius;
            golemBodyTickness = enemyStat.BodyTickness;
            golemWallAvoidRadius = enemyStat.WallAvoidRadius;
            golemEnemyAvoidRadius = enemyStat.EnmeyAvoidRadius;
            golemAttackPower = enemyStat.AttackPower;

            //  -- ゴーストの能力値設定
            enemyStat = _enemyStatHolder.GetStat(EnemyID.Ghost);
            ghostHp = enemyStat.HP;
            ghostMoveSpeed = enemyStat.MoveSpeed;
            ghostAttackRange = enemyStat.AtackRange;
            ghostSearchRadius = enemyStat.SearchRadius;
            ghostBodyTickness = enemyStat.BodyTickness;
            ghostWallAvoidRadius = enemyStat.WallAvoidRadius;
            ghostEnemyAvoidRadius = enemyStat.EnmeyAvoidRadius;
            ghostAttackPower = enemyStat.AttackPower;
        }
    }
}