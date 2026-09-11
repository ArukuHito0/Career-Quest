using UnityEngine;

public abstract class AttackBase : MonoBehaviour
{
    // 攻撃間隔
    [SerializeField] protected float attackInterval = 1f;

    // 最後に攻撃してからの経過時間
    protected float attackTimer;


    protected virtual void Update()
    {
        // 攻撃可能になるまで時間を進める
        attackTimer += Time.deltaTime;
    }


    // 現在攻撃可能か
    public bool CanAttack()
    {
        return attackTimer >= attackInterval;
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

        // 攻撃処理を実行
        Attack(target);

        // 攻撃タイマーをリセット
        attackTimer = 0f;
    }

    // 攻撃方法は子クラス側で実装する
    protected abstract void Attack(Transform targer);
}
