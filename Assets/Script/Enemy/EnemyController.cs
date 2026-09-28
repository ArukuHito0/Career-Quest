using CareerQuest.Core;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UAssert = UnityEngine.Assertions.Assert;

namespace CareerQuest.Enemy
{
    //  敵を制御するクラス
    [DisallowMultipleComponent]
    [RequireComponent(typeof(NavMeshAgent))]
    public sealed class EnemyController : MonoBehaviour, ISpatialEntity
    {
        NavMeshAgent _navAgent;

        EnemyHashManager _hashManager;
        
        [SerializeField] EnemyID _enemyID = EnemyID.Golem;
        public EnemyData EnemyData;
        TreasureChest1 _treasureChest;

        public EnemyID EnemyID { get => _enemyID; }
        
        public int Index { get; set; }  // 敵番号
        public float Tickness { get; set; }  // オブジェクトの厚さ

        public TreasureChest1 TreasureChest { get => _treasureChest; set => _treasureChest = value; }


        void Awake()
        {
            MyLogger.Log("登録");
            _hashManager = ServiceLocator.Resolve<EnemyHashManager>();
            _hashManager.Register(this);

            _navAgent = GetComponent<NavMeshAgent>();
            UAssert.IsNotNull(_navAgent, "NavMeshAgentの参照がありません");
        }

        void Update()
        {
            if (EnemyData.State == (byte)EnemyState.Attack)
            {
                if (_enemyID == EnemyID.Golem)
                {
                    AttackTreasure();
                }
                if (_enemyID == EnemyID.Ghost)
                {
                    AttackPlayer();
                }

            }
        }
        public void SetTarget(Vector3 targetPos)
        {
            MyLogger.Log("敵ターゲットセット関数作動");
            if (_navAgent.enabled && _navAgent != null)
            {
                MyLogger.Log("敵のNavMeshターゲット設定");
                _navAgent.SetDestination(targetPos);
            }
        }


        void AttackTreasure()
        {
            MyLogger.Log("お宝攻撃開始");
            _treasureChest?.TakeDamage();
        }

        void AttackPlayer()
        {
            MyLogger.Log("プレイヤー攻撃開始");
        }
    }
}