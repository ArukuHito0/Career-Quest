using CareerQuest.Core;
using UnityEngine;
using UnityEngine.AI;
using UAssert = UnityEngine.Assertions.Assert;

namespace CareerQuest.Enemy
{
    //  EnemyMgrでしにくい所をするクラス
    [DisallowMultipleComponent]
    [RequireComponent(typeof(NavMeshAgent))]
    public sealed class EnemyController : MonoBehaviour, ISpatialEntity
    {
        NavMeshAgent _navAgent;

        EnemyHashManager _hashManager;
        
        [SerializeField] EnemyID _enemyID = EnemyID.Golem;
        public EnemyData EnemyData;
        ISpatialEntity _target;

        public EnemyID EnemyID { get => _enemyID; }
        
        public int Index { get; set; }  // 敵番号
        public float Tickness { get; set; }  // オブジェクトの厚さ

        public ISpatialEntity Target { get => _target; set => _target = value; }


        void Awake()
        {
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
                else if (_enemyID == EnemyID.Ghost)
                {
                    AttackPlayer();
                }
            }
        }

        public void SetTarget(Vector3 targetPos)
        {
            if (_navAgent.enabled && _navAgent != null)
            {
                _navAgent.SetDestination(targetPos);
            }
        }

        void AttackTreasure()
        {
            MyLogger.Log("お宝攻撃開始");
            TreasureChest treasure = _target as TreasureChest;
            treasure?.TakeDamage(EnemyData.GolemAttackPower);
        }

        void AttackPlayer()
        {
            MyLogger.Log("プレイヤー攻撃開始");
            PlayerHealth playerHelth = _target as PlayerHealth;
            playerHelth?.TakeDamage(EnemyData.GhostAttackPower);
        }
    }
}