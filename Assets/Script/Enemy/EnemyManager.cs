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
        }

        void Update()
        {
            if (treasureEntities.Count == 0) return;
            if (enemyEntities.Count == 0) return;

            for (int i = 0; i < enemyEntities.Count; i++)
            {
                enemyDatas[i] = new EnemyData
                {
                    Position = enemyEntities[i].transform.position,
                    ID = enemyEntities[i].EnemyID,

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
                Datas = enemyDatas,
                TreasurePositions = _treasureHashManager.Positions,
                CellToEntityMap = _treasureHashManager.CellToEntityMap,
                CellSize =_treasureHashManager.cellSize,
                GridWidth =_treasureHashManager.girdWidth,
                DeltaTime = Time.deltaTime
            };

            JobHandle searchHandle = searchJob.Schedule(enemyEntities.Count, 64);
            
            var moveJob = new MoveJob
            {
                Datas = enemyDatas,
                TreasurePositions = _treasureHashManager.Positions,
                TreasureTickness = _treasureHashManager.Ticknesses,
                WallPositions = wallPositions,
                WallAvoidRadius = golemWallAvoidRadius,
                EnemyAvoidRadius = golemEnemyAvoidRadius,
                DeltaTime = Time.deltaTime
            };

            var moveHandle = moveJob.Schedule(enemyEntities.Count, 64, searchHandle);
            moveHandle.Complete();

            for (int i = 0; i < enemyEntities.Count; i++)
            {
                enemyEntities[i].transform.position = enemyDatas[i].Position;
                enemyEntities[i].EnemyData.State = enemyDatas[i].State;
                enemyEntities[i].EnemyData.GolemAttackPower = golemAttackPower;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }
    }
}