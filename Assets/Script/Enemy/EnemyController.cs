using CareerQuest.Core;
using CareerQuest.Player;
using UnityEngine;
using UnityEngine.AI;
using UAssert = UnityEngine.Assertions.Assert;

namespace CareerQuest.Enemy
{
    //  EnemyMgrでしにくい所をするクラス
    [DisallowMultipleComponent]
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(NavMeshAgent))]
    public sealed class EnemyController : MonoBehaviour, ISpatialEntity
    {
        NavMeshAgent _navAgent;

        EnemyHashManager _hashMgr;
        EnemyManagerBase _enemyMgr;
        
        [SerializeField] EnemyID _id = EnemyID.Golem;
        public EnemyData EnemyData;
        ISpatialEntity _target;

        public EnemyManagerBase EnemyMgrBase { private get => _enemyMgr; set => _enemyMgr = value; }

        public int index;
        public EnemyID ID { get => _id; }
        
        public int Index { get => index; set => index = value; }  // 敵番号
        public float Tickness { get; set; }  // オブジェクトの厚さ

        public ISpatialEntity Target { get => _target; set => _target = value; }


        void Awake()
        {
            _hashMgr = ServiceLocator.Resolve<EnemyHashManager>();

            _navAgent = GetComponent<NavMeshAgent>();
            UAssert.IsNotNull(_navAgent, "NavMeshAgentの参照がありません");
        }

        public void Regist()
        {
            _hashMgr.Register(this);
        }

        public void SetTarget(Vector3 targetPos)
        {
            if (_navAgent == null || !_navAgent.enabled || !_navAgent.isOnNavMesh)
            {
                return;
            }
            _navAgent.SetDestination(targetPos);
        }

        public void Attack()
        {
            if(_id == EnemyID.Ghost)
            {
                
            }

            if (EnemyData.Target == EnemyTarget.Treasure)
            {
                AttackTreasure();
            }
            else if (EnemyData.Target == EnemyTarget.Player)
            {
                AttackPlayer();
            }
        }

        void AttackTreasure()
        {
            MyLogger.Log("お宝攻撃開始");
            TreasureChest treasure = _target as TreasureChest;
            treasure?.TakeDamage(EnemyData.AttackPower);
        }

        void AttackPlayer()
        {
            MyLogger.Log("プレイヤー攻撃開始");
            PlayerHealth playerHelth = _target as PlayerHealth;
            playerHelth?.TakeDamage(EnemyData.AttackPower);
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Bullet"))
            {
                if (other.TryGetComponent<IBullet>(out var bullet))
                {
                    if (_enemyMgr != null)
                    {
                        _enemyMgr.EnqueueDamage(index, bullet.Damage);
                    }
                }
            }
        }
    }
}