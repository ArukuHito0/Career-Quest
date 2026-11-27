using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Pool;
using CareerQuest.Core;

namespace CareerQuest.Enemy
{
    [DisallowMultipleComponent]
    public abstract class EnemyManagerBase : MonoBehaviour
    {
        protected TreasureHashManager treasureHashManager;  // 宝物のグリッドマップ管理
        protected PlayerHashManager playerHashManager; // プレイヤーのグリッドマップ管理
        protected EnemyHashManager enemyHashManager; // 敵のグリッドマップ管理

        protected List<TreasureChest> activeTreasureEntities = new List<TreasureChest>();
        protected List<PlayerHealth> activePlayerEntities = new List<PlayerHealth>();
        protected List<EnemyController> activeEnemyEntities = new List<EnemyController>();
        protected NativeArray<Vector3> wallPositions;


        //[SerializeField] EnemyStatHolder _enemyStatHolder;  // ステータス保持SO
        [SerializeField] EnemyStatsDatabase _statsDb;  // ステータス保持Dd
        NativeArray<EnemyData> _statCache;

        [SerializeField] EnemySpawnData spawnPos;
        [SerializeField] EnemyController _golemPrefab;
        [SerializeField] EnemyController _ghostPrefab;
        [SerializeField] EnemyController _swarmPrefab;
        [SerializeField] protected int maxEnemyCount = 1000;
        Dictionary<EnemyID, ObjectPool<EnemyController>> _pools = new Dictionary<EnemyID, ObjectPool<EnemyController>>();

        protected NativeArray<EnemyData> bufferA;
        protected NativeArray<EnemyData> bufferB;
        protected bool isUsingBufferA = true;

        protected readonly List<pendingInfo> pendingInfo = new List<pendingInfo>(128);
        protected List<RuntimeSpawnPoint> runtimeSpawnPoints = new List<RuntimeSpawnPoint>();

        readonly float spawnInterval = 0.2f;

        protected virtual void Awake()
        {
            ServiceLocator.Register(this);
            treasureHashManager = ServiceLocator.Resolve<TreasureHashManager>();
            playerHashManager = ServiceLocator.Resolve<PlayerHashManager>();
            enemyHashManager = ServiceLocator.Resolve<EnemyHashManager>();

            //bulletManager = ServiceLocator.Resolve<BulletManager>();

            bufferA = new NativeArray<EnemyData>(maxEnemyCount, Allocator.Persistent);
            bufferB = new NativeArray<EnemyData>(maxEnemyCount, Allocator.Persistent);

            _pools[EnemyID.Golem] = new ObjectPool<EnemyController>(
                createFunc: () => Instantiate(_golemPrefab),
                actionOnGet: e => {
                    e.gameObject.SetActive(true);
                    e.Regist();
                    e.EnemyMgrBase = this;
                },
                actionOnRelease: e => e.gameObject.SetActive(false),
                actionOnDestroy: e => Destroy(e.gameObject),
                defaultCapacity: 50
            );
            _pools[EnemyID.Ghost] = new ObjectPool<EnemyController>(
                createFunc: () => Instantiate(_ghostPrefab),
                actionOnGet: e => {
                    e.gameObject.SetActive(true);
                    e.Regist();
                    e.EnemyMgrBase = this;
                },
                actionOnRelease: e => e.gameObject.SetActive(false),
                actionOnDestroy: e => Destroy(e.gameObject),
                defaultCapacity: 50
            );
            _pools[EnemyID.Swarm] = new ObjectPool<EnemyController>(
                createFunc: () => Instantiate(_swarmPrefab),
                actionOnGet: e => {
                    e.gameObject.SetActive(true);
                    e.Regist();
                    e.EnemyMgrBase = this;
                },
                actionOnRelease: e => e.gameObject.SetActive(false),
                actionOnDestroy: e => Destroy(e.gameObject),
                defaultCapacity: 50
            );

            if (spawnPos != null && spawnPos.SpawnPoints != null)
            {
                foreach (var point in spawnPos.SpawnPoints)
                {
                    if (
                       (point.Candidates == null || point.Candidates.Length == 0) 
                       ||(point.StartSpawnCnt == 0 && point.TresuareTriggerSpawnCnt == 0)
                       )
                        continue;

                    runtimeSpawnPoints.Add(new RuntimeSpawnPoint
                    {
                        Position = point.Position,
                        StartSpawnCnt = point.StartSpawnCnt,
                        TreasureTriggerSpwanCnt = point.TresuareTriggerSpawnCnt,
                        Candidates = point.Candidates
                    });
                }
            }

            int enumLength = System.Enum.GetValues(typeof(EnemyID)).Length;
            _statCache = new NativeArray<EnemyData>(enumLength, Allocator.Persistent);

            foreach (EnemyID id in System.Enum.GetValues(typeof(EnemyID)))
            {
                var stat = _statsDb.GetStat(id);
                int index = (int)id;

                _statCache[index] = new EnemyData
                {
                    ID = id,
                    Target = EnemyTarget.None,
                    State = (byte)EnemyState.Invincible,
                    GroupVolume = stat.GroupVolume,

                    Hp = stat.HP,
                    MoveSpd = stat.MoveSpd,
                    AttackPower = stat.AttackPower,
                    AttackRange = stat.AttackRange,
                    AttackCoolDown = stat.AttackCooldown,

                    TargetIndex = -1,
                    SearchRadius = stat.SearchRadius,
                    //BodyTickness = stat.BodyTickness,
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

            ServiceLocator.Unregister(this);
        }

        public IEnumerator SpawnEnemy(GameState state)
        {
            for (int i = 0; i < runtimeSpawnPoints.Count; i++)
            {
                var pointData = runtimeSpawnPoints[i];
                Vector3 pos = pointData.Position;
                EnemyID selectedID = LotteryEnemy(pointData.Candidates);
                int volume = Mathf.Max(1, _statCache[(int)selectedID].GroupVolume);

                if (activeEnemyEntities.Count + volume > maxEnemyCount)
                {
                    MyLogger.WarningLog("敵の最大数を超過するため、生成をしません");
                    continue;
                }

                EnsureBufferSize(activeEnemyEntities.Count + volume);

                if (UnityEngine.AI.NavMesh.SamplePosition(pos, out var hit, 10f, UnityEngine.AI.NavMesh.AllAreas))
                {
                    pos = hit.position;
                }

                if (!_pools.TryGetValue(selectedID, out var targetPool))
                { continue; }

                byte spawnLimit = state switch
                {
                    GameState.DontGetTreasure => pointData.StartSpawnCnt,
                    GameState.GetTreasure => pointData.TreasureTriggerSpwanCnt,
                    _ => 0
                };

                if (spawnLimit <= 0)
                {
                    MyLogger.ErrorLog($"{i} : 生成数が0以下なので生成しません。");
                    continue;
                }

                MyLogger.Log("敵生成開始");
                for (byte spawnCnt = 0; spawnCnt < spawnLimit; spawnCnt++)
                {
                    for (int cnt = 0; cnt < volume; cnt++)
                    {
                        var enemy = targetPool.Get();

                        var agent = enemy.GetComponent<UnityEngine.AI.NavMeshAgent>();
                        if (agent != null)
                        {
                            agent.enabled = false;
                            enemy.transform.position = pos;
                            agent.enabled = true;
                            agent.Warp(pos);
                        }
                        else
                        {
                            enemy.transform.position = pos;
                        }

                        int newIndex = activeEnemyEntities.Count - 1;
                        if (newIndex < 0)
                        {
                            newIndex = 0;
                        }

                        var enemyData = CreateInitialEnemyData(enemy);
                        enemyData.IsLeader = (cnt == 0) ? (byte)1 : (byte)0;
                        enemy.EnemyData = enemyData;
                        enemy.NavAgent.speed = enemyData.MoveSpd;

                        if (isUsingBufferA)
                        {
                            bufferA[newIndex] = enemyData;
                        }
                        else
                        {
                            bufferB[newIndex] = enemyData;
                        }

                        runtimeSpawnPoints[i] = pointData;
                        yield return new WaitForSeconds(spawnInterval);
                    }
                }
            }
            yield return null;
        }
        ////  生成
        //public void SpawnEnemy(GameState state)
        //{
        //    for (int i = 0; i < runtimeSpawnPoints.Count; i++)
        //    {
        //        var pointData = runtimeSpawnPoints[i];
        //        Vector3 pos = pointData.Position;
        //        EnemyID selectedID = LotteryEnemy(pointData.Candidates);
        //        int volume = Mathf.Max(1, _statCache[(int)selectedID].GroupVolume);

        //        if (activeEnemyEntities.Count + volume > maxEnemyCount)
        //        {
        //            MyLogger.WarningLog("敵の最大数を超過するため、生成をしません");
        //            return;
        //        }

        //        EnsureBufferSize(activeEnemyEntities.Count + volume);
                
        //        if (UnityEngine.AI.NavMesh.SamplePosition(pos, out var hit, 10f, UnityEngine.AI.NavMesh.AllAreas))
        //        {
        //            pos = hit.position;
        //        }

        //        if (!_pools.TryGetValue(selectedID, out var targetPool))
        //        { continue; }

        //        byte spawnLimit = state switch
        //        {
        //            GameState.DontGetTreasure => pointData.StartSpawnCnt,
        //            GameState.GetTreasure => pointData.TreasureTriggerSpwanCnt,
        //            _ => 0
        //        };

        //        if (spawnLimit <= 0)
        //        {
        //            MyLogger.ErrorLog($"{i} : 生成数が0以下なので生成しません。");
        //            return; 
        //        }

        //        MyLogger.Log("敵生成開始");
        //        for (byte spawnCnt = 0; spawnCnt < spawnLimit; spawnCnt++)
        //        {
        //            for (int cnt = 0; cnt < volume; cnt++)
        //            {
        //                var enemy = targetPool.Get();

        //                var agent = enemy.GetComponent<UnityEngine.AI.NavMeshAgent>();
        //                if (agent != null)
        //                {
        //                    agent.enabled = false;
        //                    enemy.transform.position = pos;
        //                    agent.enabled = true;
        //                    agent.Warp(pos);
        //                }
        //                else
        //                {
        //                    enemy.transform.position = pos;
        //                }

        //                int newIndex = activeEnemyEntities.Count - 1;
        //                if (newIndex < 0)
        //                {
        //                    newIndex = 0;
        //                }

        //                var enemyData = CreateInitialEnemyData(enemy);
        //                enemyData.IsLeader = (cnt == 0) ? (byte)1 : (byte)0;
        //                enemy.EnemyData = enemyData;
        //                enemy.NavAgent.speed = enemyData.MoveSpd;

        //                if (isUsingBufferA)
        //                {
        //                    bufferA[newIndex] = enemyData;
        //                }
        //                else
        //                {
        //                    bufferB[newIndex] = enemyData;
        //                }

        //                runtimeSpawnPoints[i] = pointData;
        //            }
        //        }
        //    }
        //}

        //  受けたダメージを記録(処理競合防止)
        public void EnqueueDamage(int index, byte dmg)
        {
            pendingInfo.Add(new pendingInfo
            {
                Index = index,
                InfoType = InfoType.Damage,
                Damage = dmg
            });
        }
        //  状態変化を記録(処理競合防止)
        public void ChangeState(int index, EnemyState state)
        {
            pendingInfo.Add(new pendingInfo
            {
                Index = index,
                InfoType = InfoType.State,
                State = state
            });
        }
        //  移動速度変化を記録(処理競合防止)
        public void ChangeSpd(int index, float spd)
        {
            pendingInfo.Add(new pendingInfo
            {
                Index = index,
                InfoType = InfoType.Speed,
                MoveSpd = spd
            });
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
                MoveSpd = stat.MoveSpd,
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