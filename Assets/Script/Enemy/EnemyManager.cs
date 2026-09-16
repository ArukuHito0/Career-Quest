using CareerQuest.Core;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;
using static CareerQuest.Enemy.MoveJob;

namespace CareerQuest.Enemy
{
    //  敵の挙動を制御するクラス
    public sealed class EnemyManager : EnemyManagerBase<TreasureChest1>
    {
        protected override void Awake()
        {
            base.Awake();
            InvokeRepeating("MyMethod", 2.0f, 7.0f);  // a用
            InvokeRepeating("SpawnEnmey", 2.0f, 2.0f);  // a用
        }

        protected override void Start()
        {
            base.Start();
            //SpawnEnemy(new Vector3(0, 0, 0));
        }
        //  --  FOR ALPHA  --  //
        float timer1 = 0f;
        float timer2 = 0f;
        float timer3 = 0f;
        [SerializeField] Transform testPos1;
        [SerializeField] Transform testPos2;
        [SerializeField] Transform testPos3;
        //  --  FOR ALPHA  --  //
        void Update()
        {
            //  --  FOR ALPHA  --  //

            timer1 += Time.deltaTime;
            if (timer1 >= 5f)
            {
                timer1 = 0f;
                SpawnEnemy(testPos1.position);
            }

            timer2 += Time.deltaTime;
            if (timer2 >= 3f)
            {
                timer2 = 0f;
                SpawnEnemy(testPos2.position);
            }

            timer3 += Time.deltaTime;
            if (timer3 >= 1.7f)
            {
                timer3 = 0f;
                SpawnEnemy(testPos3.position);
            }

            //  --  FOR ALPHA  --  //

            if (activeTreasureEntities.Count == 0) return;
            if (activeEnemyEntities.Count == 0) return;

            var readBuffer = isUsingBufferA ? bufferA : bufferB;
            var writeBuffer = isUsingBufferA ? bufferB : bufferA;

            for (int i = 0; i < activeEnemyEntities.Count; i++)
            {
                readBuffer[i] = new EnemyData
                {
                    Position = activeEnemyEntities[i].transform.position,
                    ID = activeEnemyEntities[i].EnemyID,

                    GolemAttackPower = golemAttackPower,
                    GolemMoveSpeed = golemMoveSpeed,
                    GolemSearchRadius = golemSearchRadius,
                    GolemTickness = golemBodyTickness,

                    GhostMoveSpeed = ghostMoveSpeed,
                    GhostSearchRadius = golemSearchRadius,
                    GhostTickness = ghostBodyTickness,
                };
            }

            var searchTreasureJob = new SearchTreasureJob
            {
                InputDatas = readBuffer,
                TreasurePositions = treasureHashManager.Positions,
                CellToEntityMap = treasureHashManager.CellToEntityMap,
                CellSize = treasureHashManager.cellSize,
                GridWidth = treasureHashManager.girdWidth,
                DeltaTime = Time.deltaTime
            };

            JobHandle searchTreasureHandle = searchTreasureJob.Schedule(activeEnemyEntities.Count, 64);

            searchTreasureHandle.Complete();

            for (int i = 0; i < activeEnemyEntities.Count; i++)
            {
                if (
                    readBuffer[i].TargetIndex <= -1 ||
                    readBuffer[i].TargetIndex >= treasureHashManager.ActiveEntities.Count
                    )
                    continue;

                activeEnemyEntities[i].TreasureChest = treasureHashManager.ActiveEntities[readBuffer[i].TargetIndex];
                MyLogger.Log(readBuffer[i].TargetIndex);
                MyLogger.Log(treasureHashManager.ActiveEntities.Count);
                MyLogger.Log(treasureHashManager.ActiveEntities[0]);
            }

            var searchPlayerJob = new SearchPlayerJob
            {
                InputDatas = readBuffer,
                PlayerPositions = playerHashManager.Positions,
                CellToEntityMap = playerHashManager.CellToEntityMap,
                CellSize = treasureHashManager.cellSize,
                GridWidth = treasureHashManager.girdWidth,
                DeltaTime = Time.deltaTime
            };

            JobHandle searchPlayerHandle = searchPlayerJob.Schedule(activeEnemyEntities.Count, 64, searchTreasureHandle);

            JobHandle combinedSearchHandle = JobHandle.CombineDependencies(searchTreasureHandle, searchPlayerHandle);
            combinedSearchHandle.Complete();
            MyLogger.Log("周囲探索完了");

            var moveJob = new MoveJob
            {
                InputDatas = readBuffer,
                OutputDatas = writeBuffer,
                TreasurePositions = treasureHashManager.Positions,
                TreasureTickness = treasureHashManager.Ticknesses,
                PlaeyrPositions = playerHashManager.Positions,
                PlayerTickness = playerHashManager.Ticknesses,
                WallPositions = wallPositions,
                WallAvoidRadius = golemWallAvoidRadius,
                EnemyAvoidRadius = golemEnemyAvoidRadius,
                DeltaTime = Time.deltaTime
            };

            var moveHandle = moveJob.Schedule(activeEnemyEntities.Count, 64, combinedSearchHandle);
            moveHandle.Complete();

            for (int i = 0; i < activeEnemyEntities.Count; i++)
            {
                activeEnemyEntities[i].transform.position = writeBuffer[i].Position;
                activeEnemyEntities[i].EnemyData.State = writeBuffer[i].State;
                activeEnemyEntities[i].EnemyData.GolemAttackPower = golemAttackPower;
            }

            if (bulletManager == null || bulletManager.ActiveCount == 0) return;

            var collisionJob = new CollisionJob
            {
                Bullets = bulletManager.BulletBuffer,
                BulletCount = bulletManager.ActiveCount,
                Enemies = writeBuffer
            };

            isUsingBufferA = !isUsingBufferA;

            MyLogger.Log("敵行動サイクル通った");
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }
    }
}