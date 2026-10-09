using CareerQuest.Core;
using CareerQuest.Player;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Pool;

namespace CareerQuest.Enemy
{
    [DisallowMultipleComponent]
    public abstract class EnemyManagerBase<T> : MonoBehaviour where T : Component
    {
        protected TreasureHashManager treasureHashManager;  // 宝物のグリッドマップ管理
        protected PlayerHashManager playerHashManager; // プレイヤーのグリッドマップ管理
        protected EnemyHashManager enemyHashManager; // 敵のグリッドマップ管理

        protected List<TreasureChest> activeTreasureEntities = new List<TreasureChest>();
        protected List<PlayerHealth> activePlayerEntities = new List<PlayerHealth>();
        protected List<EnemyController> activeEnemyEntities = new List<EnemyController>();
        protected NativeArray<Vector3> wallPositions;

        [SerializeField] EnemySpawnData spawnPos;
        [SerializeField] EnemyController _golemPrefab;
        [SerializeField] EnemyController _ghostPrefab;
        [SerializeField] protected int maxEnemyCount = 1000;
        Dictionary<EnemyID, ObjectPool<EnemyController>> _pools = new Dictionary<EnemyID, ObjectPool<EnemyController>>();

        protected NativeArray<EnemyData> bufferA;
        protected NativeArray<EnemyData> bufferB;
        protected bool isUsingBufferA = true;

        protected BulletManager bulletManager;

        [SerializeField] EnemyStatHolder _enemyStatHolder;  // ステータス保持SO

        EnemyStat enemyStat;  // 敵のパラメーター(キャッシュ用)
        protected List<RuntimeSpawnPoint> runtimeSpawnPoints = new List<RuntimeSpawnPoint>();

        //  -- Golemステータス --  //
        protected int golemHp;                  // 体力
        //protected float golemMoveSpeed;         // 移動速度 NavMeshを使うためコメントアウト
        protected int golemAttackPower;         // 攻撃力
        protected float golemAttackCoolDown;    // 攻撃間隔
        protected float golemAttackRange;       // 攻撃半径
        protected float golemSearchRadius;      // 状況把握できる範囲の半径
        protected float golemWallAvoidRadius;   // 壁を避け始める距離
        protected float golemEnemyAvoidRadius;  // 敵を避け始める距離
        protected float golemBodyTickness;      // 体の厚さ

        //  -- Ghostステータス --  //
        protected int ghostHp;                  // 体力
        //protected float ghostMoveSpeed;         // 移動速度 NavMeshを使うためコメントアウト
        protected int ghostAttackPower;         // 攻撃力
        protected float ghostAttackRange;       // 移動速度
        protected float ghostAttackCoolDown;    // 攻撃間隔
        protected float ghostSearchRadius;      // 状況把握できる範囲の半径
        protected float ghostWallAvoidRadius;   // 壁を避け始める距離
        protected float ghostEnemyAvoidRadius;  // 敵を避け始める距離
        protected float ghostBodyTickness;      // 体の厚さ

        protected virtual void Awake()
        {
            treasureHashManager = ServiceLocator.Resolve<TreasureHashManager>();
            playerHashManager = ServiceLocator.Resolve<PlayerHashManager>();
            enemyHashManager = ServiceLocator.Resolve<EnemyHashManager>();

            bulletManager = ServiceLocator.Resolve<BulletManager>();

            bufferA = new NativeArray<EnemyData>(maxEnemyCount, Allocator.Persistent);
            bufferB = new NativeArray<EnemyData>(maxEnemyCount, Allocator.Persistent);

            _pools[EnemyID.Golem] = new ObjectPool<EnemyController>(
                createFunc: () => Instantiate(_golemPrefab),
                actionOnGet: e => {
                    e.gameObject.SetActive(true);
                    e.Regist();
                },
                actionOnRelease: e => e.gameObject.SetActive(false),
                actionOnDestroy: e => Destroy(e.gameObject),
                defaultCapacity: 50
            );
            _pools[EnemyID.Ghost] = new ObjectPool<EnemyController>(
                createFunc: () => Instantiate(_ghostPrefab),
                actionOnGet: e => { e.gameObject.SetActive(true); e.Regist(); },
                actionOnRelease: e => e.gameObject.SetActive(false),
                actionOnDestroy: e => Destroy(e.gameObject),
                defaultCapacity: 50
            );

            if (spawnPos != null && spawnPos.SpawnPositions != null)
            {
                foreach (var point in spawnPos.SpawnPositions)
                {
                    if (point.Candidates == null || point.Candidates.Length == 0) continue;

                    float interval = point.Candidates[0].SpawnInterval;

                    runtimeSpawnPoints.Add(new RuntimeSpawnPoint
                    {
                        Position = point.Position,
                        CurrentTimer = 0f,
                        TargetInterval = interval,
                        Candidates = point.Candidates
                    });
                }
            }

            SetStat();
        }

        protected virtual void Start()
        {
            activeTreasureEntities = treasureHashManager.ActiveEntities;
            activePlayerEntities = playerHashManager.ActiveEntities;
            activeEnemyEntities = enemyHashManager.ActiveEntities;

            //var wallObjects = GameObject.FindGameObjectsWithTag("Wall");
            //wallPositions = new NativeArray<Vector3>(wallObjects.Length, Allocator.Persistent);
            //for (int i = 0; i < wallObjects.Length; i++)
            //{
            //    wallPositions[i] = wallObjects[i].transform.position;
            //}
        }

        protected virtual void OnDestroy()
        {
            if (bufferA.IsCreated) bufferA.Dispose();
            if (bufferB.IsCreated) bufferB.Dispose();
        }

        //  生成
        public void SpawnEnemy(Vector3 position, EnemyID id)
        {
            if (UnityEngine.AI.NavMesh.SamplePosition(position, out var hit, 10f, UnityEngine.AI.NavMesh.AllAreas))
            {
                position = hit.position;
            }

            EnsureBufferSize(activeEnemyEntities.Count + 1);
            
            if (!_pools.TryGetValue(id, out var targetPool))
                return;

            var enemy = targetPool.Get();

            var agent = enemy.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (agent != null)
            {
                agent.enabled = false;
                enemy.transform.position = position;
                agent.enabled = true;
                agent.Warp(position);
            }
            else
            {
                enemy.transform.position = position;
            }

            int newIndex = activeEnemyEntities.Count - 1;
            if(newIndex < 0)
            {
                newIndex = 0;
            }
            if (isUsingBufferA)
            {
                bufferA[newIndex] = CreateInitialEnemyData(enemy);
            }
            else
            {
                bufferB[newIndex] = CreateInitialEnemyData(enemy);
            }
        }

        //  削除
        public void DespawnEnemy(EnemyController enemy)
        {
            if (enemy == null) return;

            int removeIndex = enemy.Index;
            int lastIndex = activeEnemyEntities.Count - 1;

            if (_pools.TryGetValue(enemy.ID, out var targetPool))
            {
                targetPool.Release(enemy);
            }

            if (removeIndex < lastIndex)
            {
                var lastEnemy = activeEnemyEntities[lastIndex];
                activeEnemyEntities[removeIndex] = lastEnemy;
                lastEnemy.Index = removeIndex;
                bufferA[removeIndex] = bufferA[lastIndex];
                bufferB[removeIndex] = bufferB[lastIndex];
            }
            activeEnemyEntities.RemoveAt(lastIndex);
        }
        protected EnemyID LotteryEnemy(EnemySpawnData.EnemySpawnCandidate[] candidates)
        {
            float totalPercent = 0f;
            for (int i = 0; i < candidates.Length; i++)
            {
                totalPercent += candidates[i].Percent;
            }

            if (totalPercent <= 0f) return candidates[0].EnemyID;

            float randomValue = Random.Range(0f, totalPercent);
            float currentSum = 0f;

            for (int i = 0; i < candidates.Length; i++)
            {
                currentSum += candidates[i].Percent;
                if (randomValue <= currentSum)
                {
                    return candidates[i].EnemyID;
                }
            }

            return candidates[0].EnemyID;
        }

        // バッファをリサイズ
        protected void EnsureBufferSize(int count)
        {
            if (bufferA.IsCreated && bufferA.Length >= count) return;

            if (bufferA.IsCreated) bufferA.Dispose();
            if (bufferB.IsCreated) bufferB.Dispose();

            int newSize = Mathf.Max(count, maxEnemyCount);
            bufferA = new NativeArray<EnemyData>(newSize, Allocator.Persistent);
            bufferB = new NativeArray<EnemyData>(newSize, Allocator.Persistent);
        }

        protected EnemyData CreateInitialEnemyData(EnemyController entity)
        {
            if (entity.ID == EnemyID.Golem)
            {
                return new EnemyData
                {
                    ID = entity.ID,
                    State = (byte)EnemyState.Search,
                    CurrentHp = golemHp,
                    CurrentAttackCoolDown = 0f,
                    Position = entity.transform.position,
                    TargetIndex = -1,

                    //GolemMoveSpeed = golemMoveSpeed, 今はNavmeshのスピードを使ってる
                    GolemAttackPower = golemAttackPower,
                    GolemAttackCoolDown = golemAttackCoolDown,
                    GolemAttackRange = golemAttackRange,
                    GolemSearchRadius = golemSearchRadius,
                    GolemTickness = golemBodyTickness,
                };
            }
            else // Ghostステータス
            {
                return new EnemyData
                {
                    ID = entity.ID,
                    State = (byte)EnemyState.Search,
                    CurrentHp = ghostHp,
                    CurrentAttackCoolDown = 0f,
                    Position = entity.transform.position,
                    TargetIndex = -1,

                    //GhostMoveSpeed = ghostMoveSpeed, 今はNavmeshのスピードを使ってる
                    GhostAttackPower = ghostAttackPower,
                    GhostAttackCoolDown = ghostAttackCoolDown,
                    GhostAttackRange = ghostAttackRange,
                    GhostSearchRadius = ghostSearchRadius,
                    GhostTickness = ghostBodyTickness,
                };
            }
        }

        void SetStat()
        {
            //  -- ゴーレムの能力値設定
            enemyStat = _enemyStatHolder.GetStat(EnemyID.Golem);
            golemHp = enemyStat.HP;
            //golemMoveSpeed = enemyStat.MoveSpeed; NavMeshを使うためコメントアウト
            golemAttackPower = enemyStat.AttackPower;
            golemAttackRange = enemyStat.AtackRange;
            golemAttackCoolDown = enemyStat.AtackCoolDown;
            golemSearchRadius = enemyStat.SearchRadius;
            golemBodyTickness = enemyStat.BodyTickness;
            golemWallAvoidRadius = enemyStat.WallAvoidRadius;
            golemEnemyAvoidRadius = enemyStat.EnmeyAvoidRadius;

            //  -- ゴーストの能力値設定
            enemyStat = _enemyStatHolder.GetStat(EnemyID.Ghost);
            ghostHp = enemyStat.HP;
            //ghostMoveSpeed = enemyStat.MoveSpeed; NavMeshを使うためコメントアウト
            ghostAttackPower = enemyStat.AttackPower;
            ghostAttackCoolDown = enemyStat.AtackCoolDown;
            ghostAttackRange = enemyStat.AtackRange;
            ghostSearchRadius = enemyStat.SearchRadius;
            ghostBodyTickness = enemyStat.BodyTickness;
            ghostWallAvoidRadius = enemyStat.WallAvoidRadius;
            ghostEnemyAvoidRadius = enemyStat.EnmeyAvoidRadius;
        }
    }
}