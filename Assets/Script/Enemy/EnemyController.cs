using CareerQuest.Core;
using UnityEngine;
using UAssert = UnityEngine.Assertions.Assert;

namespace CareerQuest.Enemy
{
    //  敵を制御するクラス
    [DisallowMultipleComponent]
    public sealed class EnemyController : MonoBehaviour, ISpatialEntity
    {
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

            //UAssert.IsNotNull(_treasureChest, "_treasureChestの参照がありません");
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