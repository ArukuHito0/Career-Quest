using CareerQuest.Core;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;

namespace CareerQuest.Enemy
{
    //  敵の挙動を制御するクラス
    public sealed class EnemyManager : EnemyManagerBase<TreasureChest1>
    {
        protected override void Awake()
        {
            base.Awake();
        }

        protected override void Start()
        {
            base.Start();
        }
        //  --  FOR ALPHA  --  //
        float timer1 = 0f;
        [SerializeField] Transform testPos1;
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
                    GhostSearchRadius = ghostSearchRadius,
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

            for (int i = 0; i < activeEnemyEntities.Count; i++)
            {
                activeEnemyEntities[i].transform.position = readBuffer[i].Position;
                activeEnemyEntities[i].EnemyData.State = readBuffer[i].State;
                activeEnemyEntities[i].EnemyData.GolemAttackPower = golemAttackPower;

                int targetIndex = readBuffer[i].TargetIndex;  // 可読性のためのにintに移してます。
                if (targetIndex >= 0 && targetIndex < treasureHashManager.ActiveEntities.Count)
                {
                    var targetChest = treasureHashManager.ActiveEntities[targetIndex];
                    if (activeEnemyEntities[i].EnemyID == EnemyID.Golem)
                    {
                        activeEnemyEntities[i].TreasureChest = targetChest;
                        activeEnemyEntities[i].SetTarget(targetChest.transform.position);
                    }
                    if (activeEnemyEntities[i].EnemyID == EnemyID.Ghost)
                    {
                        activeEnemyEntities[i].SetTarget(playerHashManager.Positions[targetIndex]);
                        Debug.Log(playerHashManager.Positions[targetIndex]);
                    }
                }
            }

            if (bulletManager == null || bulletManager.ActiveCount == 0) return;

            var collisionJob = new CollisionJob
            {
                Bullets = bulletManager.BulletBuffer,
                BulletCount = bulletManager.ActiveCount,
                Enemies = readBuffer
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


/* navmeshで代替してみるのでコメントアウト */
//var moveJob = new MoveJob
//{
//    InputDatas = readBuffer,
//    OutputDatas = writeBuffer,
//    TreasurePositions = treasureHashManager.Positions,
//    TreasureTickness = treasureHashManager.Ticknesses,
//    PlaeyrPositions = playerHashManager.Positions,
//    PlayerTickness = playerHashManager.Ticknesses,
//    WallPositions = wallPositions,
//    WallAvoidRadius = golemWallAvoidRadius,
//    EnemyAvoidRadius = golemEnemyAvoidRadius,
//    DeltaTime = Time.deltaTime
//};