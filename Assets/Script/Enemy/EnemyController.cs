using CareerQuest.Core;
using UnityEngine;

namespace CareerQuest.Enemy
{
    //  敵を制御するクラス
    [DisallowMultipleComponent]
    public sealed class EnemyContoroller : MonoBehaviour, ISpatialEntity
    {
        EnemyHashManager _hashManager;

        public EnemyData EnemyData;

        [SerializeField] EnemyID _enemyID = EnemyID.Golem;
        
        public int Index { get; set; }  // 敵番号
        public int Tickness { get; set; }  // オブジェクトの厚さ

        [SerializeField] TreasureChest treasureChest;

        void Awake()
        {
            _hashManager = ServiceLocator.Resolve<EnemyHashManager>();
            _hashManager.Register(this);
        }

        void Start()
        {
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