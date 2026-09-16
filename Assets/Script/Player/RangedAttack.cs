using CareerQuest.Player;
using UnityEngine;

public class RangedAttack : AttackBase
{
    // ”­Ë‚·‚é’e‚ÌPrefab
    [SerializeField] private GameObject bulletPrefab;

    // ’e‚ğ”­Ë‚·‚éˆÊ’u
    [SerializeField] private Transform firePoint;

    // ‰“‹——£UŒ‚
    protected override void Attack(Transform target)
    {
        // ’e‚â”­Ë’n“_‚ªİ’è‚³‚ê‚Ä‚¢‚È‚¯‚ê‚ÎUŒ‚‚µ‚È‚¢
        if (bulletPrefab == null || firePoint == null)
            return;

        // ’e‚ğ¶¬
        GameObject bulletObject = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        // ¶¬‚µ‚½’e‚ÌBulletƒXƒNƒŠƒvƒg‚ğæ“¾
        Bullet bullet = bulletObject.GetComponent<Bullet>();

        // ’e‚ÉUŒ‚‘ÎÛ‚ğ“n‚·
        if (bullet != null)
        {
            bullet.SetTarget(target);
        }
    }
}
