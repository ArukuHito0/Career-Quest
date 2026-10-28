using CareerQuest.Core;
using CareerQuest.Player;
using UnityEngine;
using UnityEngine.Rendering;

public class RangedAttack : AttackBase
{
    // ”­Ë‚·‚é’e‚ÌPrefab
    [SerializeField] private GameObject bulletPrefab;

    // ’e‚ğ”­Ë‚·‚éˆÊ’u
    [SerializeField] private Transform firePoint;

    // ‰“‹——£UŒ‚
    protected override bool Attack(Transform target)
    {
        MyLogger.Log("ƒvƒŒƒCƒ„[‰“‹——£UŒ‚");

        if (target == null || firePoint == null)
            return false;

        // ƒNƒ‰ƒXİ’è‚©‚ç’ePrefab‚ğæ“¾‚·‚é
        UnitStats unitStats = GetComponent<UnitStats>();

        GameObject currentBulletPrefab = bulletPrefab;

        if (unitStats != null &&
            unitStats.ClassData != null &&
            unitStats.ClassData.projectilePrefab != null)
        {
            currentBulletPrefab = unitStats.ClassData.projectilePrefab;
        }

        // ’ePrefab‚ªİ’è‚³‚ê‚Ä‚¢‚È‚¯‚ê‚ÎUŒ‚‚µ‚È‚¢
        if (currentBulletPrefab != null)
        {
            return false;
        }

        GameObject bulletObject = Instantiate(currentBulletPrefab, firePoint.position, firePoint.rotation);

        // ’e‚ÉUŒ‚‘ÎÛ‚ğ“n‚·
        Bullet bullet = bulletObject.GetComponent<Bullet>();

        if (bullet != null)
        {
            bullet.SetTarget(target);
        }

        return true;
    }
}
