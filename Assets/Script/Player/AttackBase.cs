using Unity.VisualScripting;
using UnityEngine;

public abstract class AttackBase : MonoBehaviour
{
    // 攻撃間隔
    [SerializeField] protected float attackInterval = 0.2f;

    // 最後に攻撃してからの経過時間
    protected float attackTimer;

    // ユニットのクラス設定を取得する
    private UnitStats unitStats;

    protected virtual void Start()
    {
        unitStats = GetComponent<UnitStats>();
    }

    protected virtual void Update()
    {
        // 攻撃可能になるまで時間を進める
        attackTimer += Time.deltaTime;
    }

    // 実際に使用する攻撃間隔を取得する
    protected float GetAttackInterval()
    {
        // クラスデータがある場合は、クラスデータの攻撃間隔を使用する
        if (unitStats != null && unitStats.ClassData != null)
        {
            return unitStats.ClassData.attackInterval;
        }

        // クラスデータがない場合はInspecterの設定値を使用する
        return attackInterval;
    }

    // 現在攻撃可能か
    public bool CanAttack()
    {
        return attackTimer >= GetAttackInterval();
    }


    // 攻撃を実行
    public void TryAttack(Transform target)
    {

        //ターゲットが存在しない場合は何もしない
        if (target == null)
            return;

        // 攻撃インターバル中
        if (!CanAttack())
            return;

        // 攻撃が成功したか確認
        bool attacked = Attack(target);

        // 攻撃した場合だけタイマーをリセット
        if(attacked)
            attackTimer = 0f;
    }

    // 攻撃方法は子クラス側で実装する
    protected abstract bool Attack(Transform targer);
}
