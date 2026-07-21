using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using CareerQuest.Core;

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
        protected NativeArray<Vector3> wallPositions;

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
            _treasureHashManager = ServiceLocator.Resolve<TreasureHashManager>();
            _enemyHashManager = ServiceLocator.Resolve<EnemyHashManager>();

            enemyStat = _enemyStatHolder.GetStat(EnemyID.Golem);
            golemHp = enemyStat.HP;
            golemMoveSpeed = enemyStat.MoveSpeed;
            golemAttackRange = enemyStat.AtackRange;
            golemSearchRadius = enemyStat.SearchRadius;
            golemBodyTickness = enemyStat.BodyTickness;
            golemWallAvoidRadius = enemyStat.WallAvoidRadius;
            golemEnemyAvoidRadius = enemyStat.EnmeyAvoidRadius;
            golemAttackPower = enemyStat.AttackPower;

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

        protected virtual void Start()
        {
            treasureEntities = _treasureHashManager.Entities;
            enemyEntities = _enemyHashManager.Entities;

            enemyDatas = new NativeArray<EnemyData>(enemyEntities.Count, Allocator.Persistent);

            var wallObjects = GameObject.FindGameObjectsWithTag("Wall");
            wallPositions = new NativeArray<Vector3>(wallObjects.Length, Allocator.Persistent);
            for (int i = 0; i < wallObjects.Length; i++)
            {
                wallPositions[i] = wallObjects[i].transform.position;
            }
        }

        protected virtual void OnDestroy()
        {
            if (enemyDatas.IsCreated) enemyDatas.Dispose();
        }
    }
}