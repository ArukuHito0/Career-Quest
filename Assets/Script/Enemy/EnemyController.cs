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

        public EnemyID EnemyID { get => _enemyID; }
        
        public int Index { get; set; }  // 敵番号
        public float Tickness { get; set; }  // オブジェクトの厚さ

        void Awake()
        {
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
        }
    }
}