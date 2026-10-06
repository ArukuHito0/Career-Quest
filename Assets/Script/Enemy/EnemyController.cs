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
        
        [SerializeField] EnemyID _id = EnemyID.Golem;
        public EnemyData EnemyData;
        ISpatialEntity _target;

        public int index;

        public EnemyID ID { get => _id; }
        
        public int Index { get => index; set => index = value; }  // 敵番号
        public float Tickness { get; set; }  // オブジェクトの厚さ

        public ISpatialEntity Target { get => _target; set => _target = value; }


        void Awake()
        {
            _hashManager = ServiceLocator.Resolve<EnemyHashManager>();

            _navAgent = GetComponent<NavMeshAgent>();
            UAssert.IsNotNull(_navAgent, "NavMeshAgentの参照がありません");
        }

        public void Regist()
        {
            _hashManager.Register(this);
        }

        public void SetTarget(Vector3 targetPos)
        {
            MyLogger.Log(123456);
            if (_navAgent.enabled && _navAgent != null)
            {
                MyLogger.WarningLog(123456);
                _navAgent.SetDestination(targetPos);
            }
        }
        public void Attack()
        {
            if (_id == EnemyID.Golem)
            {
                AttackTreasure();
            }
            else if (_id == EnemyID.Ghost)
            {
                AttackPlayer();
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