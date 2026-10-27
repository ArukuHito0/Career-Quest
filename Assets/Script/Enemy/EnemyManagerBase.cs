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
        [SerializeField] EnemyController _swarmPrefab;
        [SerializeField] protected int maxEnemyCount = 1000;
        Dictionary<EnemyID, ObjectPool<EnemyController>> _pools = new Dictionary<EnemyID, ObjectPool<EnemyController>>();

        protected NativeArray<EnemyData> bufferA;
        protected NativeArray<EnemyData> bufferB;
        protected bool isUsingBufferA = true;

        protected BulletManager bulletManager;

        [SerializeField] EnemyStatHolder _enemyStatHolder;  // ステータス保持SO
        NativeArray<EnemyData> _statCache;
        
        protected List<RuntimeSpawnPoint> runtimeSpawnPoints = new List<RuntimeSpawnPoint>();

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
            _pools[EnemyID.Swarm] = new ObjectPool<EnemyController>(
                createFunc: () => Instantiate(_swarmPrefab),
                actionOnGet: e => { e.gameObject.SetActive(true); e.Regist(); },
                actionOnRelease: e => e.gameObject.SetActive(false),
                actionOnDestroy: e => Destroy(e.gameObject),
                defaultCapacity: 50
            );

            if (spawnPos != null && spawnPos.SpawnPoints != null)
            {
                foreach (var point in spawnPos.SpawnPoints)
                {
                    if (point.Candidates == null || point.Candidates.Length == 0) continue;

                    float interval = point.SpawnInterval;

                    runtimeSpawnPoints.Add(new RuntimeSpawnPoint
                    {
                        Position = point.Position,
                        CurrentTimer = 0f,
                        SpawnInterval = interval,
                        Candidates = point.Candidates
                    });
                }
            }

            int enumLength = System.Enum.GetValues(typeof(EnemyID)).Length;
            _statCache = new NativeArray<EnemyData>(enumLength, Allocator.Persistent);

            foreach (EnemyID id in System.Enum.GetValues(typeof(EnemyID)))
            {
                var stat = _enemyStatHolder.GetStat(id);
                int index = (int)id;

                _statCache[index] = new EnemyData
                {
                    ID = id,
                    Target = EnemyTarget.None,
                    State = (byte)EnemyState.Invincible,
                    GroupVolume = stat.GroupVolume,

                    Hp = stat.HP,
                    AttackPower = stat.AttackPower,
                    AttackRange = stat.AtackRange,
                    AttackCoolDown = stat.AtackCoolDown,

                    TargetIndex = -1,
                    SearchRadius = stat.SearchRadius,
                    BodyTickness = stat.BodyTickness,
                };
            }
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

            if (_statCache.IsCreated) _statCache.Dispose();
        }

        //  生成
        public void SpawnEnemy(Vector3 position, EnemyID id)
        {
            if (!_pools.TryGetValue(id, out var targetPool))
                return;

            int volume = _statCache[(int)id].GroupVolume;
            if (volume <= 0)
            { volume = 1; }
            
            if (activeEnemyEntities.Count + volume > maxEnemyCount)
            {
                return;
            }
            EnsureBufferSize(activeEnemyEntities.Count + volume);

            if (UnityEngine.AI.NavMesh.SamplePosition(position, out var hit, 10f, UnityEngine.AI.NavMesh.AllAreas))
            {
                position = hit.position;
            }

            for (int cnt = 0; cnt < volume; cnt++)
            {
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
                if (newIndex < 0)
                {
                    newIndex = 0;
                }

                var enemyData = CreateInitialEnemyData(enemy);
                enemyData.IsLeader = (cnt == 0) ? (byte)1 : (byte)0;
                enemy.EnemyData = enemyData;

                if (isUsingBufferA)
                {
                    bufferA[newIndex] = enemyData;
                }
                else
                {
                    bufferB[newIndex] = enemyData;
                }
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
            int index = (int)entity.ID;
            var stat = _statCache[index];

            return new EnemyData
            {
                ID = entity.ID,
                Target = EnemyTarget.None,
                State = (byte)EnemyState.Invincible,
                GroupVolume = stat.GroupVolume,
                Hp = stat.Hp,
                CurrentHp = stat.Hp,
                AttackPower = stat.AttackPower,
                AttackRange = stat.AttackRange,
                AttackCoolDown = stat.AttackCoolDown,
                CurrentAttackCoolDown = 0f,
                Position = entity.transform.position,
                TargetIndex = -1,
                SearchRadius = stat.SearchRadius,
                BodyTickness = stat.BodyTickness
            };
        }
    }
}