using CareerQuest.Core;
using System.Linq;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;

namespace CareerQuest.Enemy
{
    //  “G‚Ì‹““®‚ğ§Œä‚·‚éƒNƒ‰ƒX
    public sealed class EnemyManager : EnemyManagerBase<Test_Treasuer>
    {
        protected override void Awake()
        {
            base.Awake();
        }

        protected override void Start()
        {
            base.Start();
            SpawnEnemy(new Vector3(0, 0, 0));
        }

        void Update()
        {
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
                    GolemBodyTickness = golemBodyTickness,

                    GhostMoveSpeed = ghostMoveSpeed,
                    GhostSearchRadius = golemSearchRadius,
                    GhostBodyTickness = ghostBodyTickness,
                };
            }

            var searchJob = new SearchJob
            {
                InputDatas = readBuffer,
                TreasurePositions = treasureHashManager.Positions,
                CellToEntityMap = treasureHashManager.CellToEntityMap,
                CellSize = treasureHashManager.cellSize,
                GridWidth = treasureHashManager.girdWidth,
                DeltaTime = Time.deltaTime
            };
            
            JobHandle searchHandle = searchJob.Schedule(activeEnemyEntities.Count, 64);

            var moveJob = new MoveJob
            {
                InputDatas = readBuffer,
                OutputDatas = writeBuffer,
                TreasurePositions = treasureHashManager.Positions,
                TreasureTickness = treasureHashManager.Ticknesses,
                WallPositions = wallPositions,
                WallAvoidRadius = golemWallAvoidRadius,
                EnemyAvoidRadius = golemEnemyAvoidRadius,
                DeltaTime = Time.deltaTime
            };

            var moveHandle = moveJob.Schedule(activeEnemyEntities.Count, 64, searchHandle);
            moveHandle.Complete();

            for (int i = 0; i < activeEnemyEntities.Count; i++)
            {
                activeEnemyEntities[i].transform.position = writeBuffer[i].Position;
                activeEnemyEntities[i].EnemyData.State = writeBuffer[i].State;
                activeEnemyEntities[i].EnemyData.GolemAttackPower = golemAttackPower;
            }

            isUsingBufferA = !isUsingBufferA;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }
    }
}