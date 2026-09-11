using CareerQuest.Core;
using UnityEngine;

namespace CareerQuest.Enemy
{
    //  敵を制御するクラス
    [DisallowMultipleComponent]
    public sealed class EnemyController : MonoBehaviour, ISpatialEntity
    {
        EnemyHashManager _hashManager;
        
        [SerializeField] EnemyID _enemyID = EnemyID.Golem;
        public EnemyData EnemyData;

        public EnemyID EnemyID { get => _enemyID; }
        
        public int Index { get; set; }  // 敵番号
        public float Tickness { get; set; }  // オブジェクトの厚さ

        [SerializeField] TreasureChest treasureChest;

        void Awake()
        {
            MyLogger.Log("登録");
            _hashManager = ServiceLocator.Resolve<EnemyHashManager>();
            _hashManager.Register(this);
        }

        void Update()
        {
            if (EnemyData.State == (byte)EnemyState.Attack)
            {
                PerformAttack();
            }
        }

        void PerformAttack()
        {
            MyLogger.Log("攻撃開始");
            treasureChest.TakeDamage();
        }
    }
}