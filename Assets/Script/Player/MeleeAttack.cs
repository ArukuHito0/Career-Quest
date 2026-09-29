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


        // プレイヤーから敵までの距離を計算
        float distance = Vector3.Distance(transform.position, target.position);

        Debug.Log($"攻撃距離：{distance} / 攻撃範囲：{attackRange}");

        // 攻撃範囲外なら攻撃しない
        if (distance > attackRange)
            return false;

        // 攻撃範囲を表示
        ShowAttackEffect();

        Debug.Log($"{gameObject.name}が{target.name}を近接攻撃しました");

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
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}