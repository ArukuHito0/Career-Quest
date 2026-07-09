using UnityEngine;
using UAssert = UnityEngine.Assertions.Assert;

namespace CareerQuest.Enemy
{
    //  敵を制御するクラス
    [DisallowMultipleComponent]
    public sealed class EnmeyContoroller : MonoBehaviour
    {
        [SerializeField] EnemyID _enemyID = EnemyID.Golem;

        [SerializeField] EnemyStatHolder _enemyStatHolder;  // ステータス保持SO
        EnemyStat _enemyStat;  // 敵のパラメーター(キャッシュ用)

        void Awake()
        {
            UAssert.IsNotNull(_enemyStatHolder, "EnemyStatHolderの参照が取得できませんでした。");

            _enemyStat = _enemyStatHolder.GetStat(_enemyID);
        }
    }
}