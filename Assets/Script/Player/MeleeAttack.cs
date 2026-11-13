using System.Collections;
using UnityEngine;

public class MeleeAttack : AttackBase
{
    // 近接攻撃が届く距離
    [SerializeField] private float attackRange = 2.5f;

    // 攻撃範囲を表示するオブジェクト
    [SerializeField] private GameObject attackEffect;

    // 攻撃範囲を表示する時間
    [SerializeField] private float effectDuration = 0.2f;

    // 近接攻撃
    protected override bool Attack(Transform target)
    {
        if (target == null)
            return false;

        UnitStats unitStats = GetComponent<UnitStats>();

        float currentAttackRange = unitStats != null ? unitStats.AttackRange : attackRange;

        float distance = Vector3.Distance(transform.position, target.position);

        
        Debug.Log(
    $"【攻撃判定】AttackRange={currentAttackRange}, Distance={distance}"
);

        if (distance > currentAttackRange)
        {
            Debug.Log("【攻撃失敗】範囲外");
            return false;
        }

        Debug.Log("【攻撃成功】範囲内");
        ShowAttackEffect();

        return true;
    }

    // 攻撃エフェクトを表示する
    private void ShowAttackEffect()
    {
        if (attackEffect == null)
            return;

        StopAllCoroutines();

        StartCoroutine(ShowAttackEffectCoroutine());
    }

    // 攻撃エフェクトを一定時間表示する
    private IEnumerator ShowAttackEffectCoroutine()
    {
        attackEffect.SetActive(true);

        yield return new WaitForSeconds(effectDuration);

        attackEffect.SetActive(false);
    }


    // Sceneビューで近接攻撃範囲を表示する
    private void OnDrawGizmosSelected()
    {
        //Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}