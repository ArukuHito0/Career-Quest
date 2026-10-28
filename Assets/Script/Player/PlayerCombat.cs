using CareerQuest.Core;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("õ“Gİ’è")]
    [SerializeField] private float searchRange = 80f;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private float searchInterval = 0.2f;

    // Œ»İUŒ‚‘ÎÛ‚É‚È‚Á‚Ä‚¢‚é“G
    private Transform target;

    // õ“G—pƒ^ƒCƒ}[
    private float searchTimer;

    // ƒvƒŒƒCƒ„[‚Ìó‘ÔŠÇ—
    private PlayerStateManager stateManager;

    // ƒNƒ‰ƒX•ÊƒXƒe[ƒ^ƒX
    private UnitStats unitStats;

    // UŒ‚ƒRƒ“ƒ|[ƒlƒ“ƒg
    private MeleeAttack meleeAttack;
    private RangedAttack rangedAttack;

    private void Awake()
    {
        stateManager = GetComponent<PlayerStateManager>();
        unitStats = GetComponent<UnitStats>();

        meleeAttack = GetComponent<MeleeAttack>();
        rangedAttack = GetComponent<RangedAttack>();
    }

    private void Update()
    {
        if (!stateManager.CanAttack())
            return;

        // ‚¨‰»‚¯ó‘Ô‚È‚çUŒ‚ˆ—‚ğ‚µ‚È‚¢
        if (stateManager.IsGhost())
        {
            target = null;
            searchTimer = 0f;
            return;
        }

        // ‰^”Àƒ‚[ƒh‚È‚çUŒ‚‚µ‚È‚¢
        if (stateManager.IsCarryMode())
        {
            target = null;
            return;
        }

        // ƒNƒ‰ƒXİ’è‚ª‚È‚¯‚ê‚ÎUŒ‚‚µ‚È‚¢
        if (unitStats == null || unitStats.ClassData == null)
        {
            return;
        }

        // õ“Gƒ^ƒCƒ}[‚ği‚ß‚é
        searchTimer += Time.deltaTime;

        // ˆê’èŠÔ‚²‚Æ‚É“G‚ğ’T‚·
        if (searchTimer >= searchInterval)
        {
            searchTimer = 0f;
            SearchEnemy();
        }

        // “G‚ª‚¢‚È‚¯‚ê‚ÎUŒ‚‚µ‚È‚¢
        if (target == null)
        {
            return;
        }


        // UŒ‚‘ÎÛ‚ªõ“G”ÍˆÍŠO‚È‚çUŒ‚‘ÎÛ‚©‚çŠO‚·
        float distance = Vector3.Distance(transform.position, target.position);

        if (distance > searchRange)
        {
            target = null;
            return;
        }

        // ƒNƒ‰ƒXİ’è‚É‰‚¶‚ÄUŒ‚•û–@‚ğØ‚è‘Ö‚¦‚é
        switch (unitStats.AttackType)
        {
            case UnitAttackType.Melee:
                if (meleeAttack != null)
                {
                    meleeAttack.TryAttack(target);
                }
                break;

            case UnitAttackType.Ranged:
                if (rangedAttack != null)
                {
                    rangedAttack.TryAttack(target);
                }
                break;
        }

    }


    // õ“G”ÍˆÍ“à‚©‚çˆê”Ô‹ß‚¢“G‚ğ’T‚·
    private void SearchEnemy()
    {
        // w’è‚µ‚½”¼Œa“à‚É‚ ‚éEnemyƒŒƒCƒ„[‚ÌCollider‚ğæ“¾
        Collider[] enemies = Physics.OverlapSphere(transform.position, searchRange, enemyLayer);

        // “G‚ªŒ©‚Â‚©‚ç‚È‚©‚Á‚½ê‡
        if (enemies.Length == 0)
        {
            target = null;
            return;
        }

        // ˆê”Ô‹ß‚¢“G
        Transform nearestEnemy = null;

        // Œ»İŒ©‚Â‚©‚Á‚Ä‚¢‚éÅ’Z‹——£
        float nearestDistance = Mathf.Infinity;

        foreach (Collider enemy in enemies)
        {
            if (enemy == null) continue;

            float distance = (enemy.transform.position - transform.position).sqrMagnitude;

            // ¡‚Ü‚ÅŒ©‚Â‚¯‚½“G‚æ‚è‹ß‚¯‚ê‚ÎXV
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestEnemy = enemy.transform;
            }
        }

        // ˆê”Ô‹ß‚¢“G‚ğUŒ‚‘ÎÛ‚É‚·‚é
        target = nearestEnemy;
    }

    // Sceneƒrƒ…[‚Åõ“G”ÍˆÍ‚ğ•\¦‚·‚é
    private void OnDrawGizmosSelected()
    {
        //Gizmos.DrawWireSphere(transform.position, searchRange);
    }
}
