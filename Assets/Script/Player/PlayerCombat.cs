using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    // d—l‚·‚éUŒ‚•û–@
    [SerializeField] private AttackBase attack;

    // õ“G”ÍˆÍ
    [SerializeField] private float searchRange = 10f;

    // õ“G‚·‚é‘ÎÛ‚ÌLayer
    [SerializeField] private LayerMask enemyLayer;

    // õ“G‚ğÀs‚·‚éŠÔŠu
    [SerializeField] private float searchInterval = 0.2f;

    // Œ»İUŒ‚‘ÎÛ‚É‚È‚Á‚Ä‚¢‚é“G
    private Transform target;

    // õ“G—pƒ^ƒCƒ}[
    private float searchTimer;

    // ƒvƒŒƒCƒ„[‚Ìó‘ÔŠÇ—
    private PlayerStateManager stateManager;

    private void Awake()
    {
        stateManager = GetComponent<PlayerStateManager>();
    }

    private void Update()
    {
        if (!stateManager.CanAttack())
            return;

        // ‚¨‰»‚¯ó‘Ô‚È‚çUŒ‚ˆ—‚ğ‚µ‚È‚¢
        if (stateManager.IsGhost())
        {
            target = null;
            return;
        }

        // ‰^”Àƒ‚[ƒh‚È‚çUŒ‚‚µ‚È‚¢
        if (stateManager.IsCarryMode())
        {
            target = null;
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

        // İ’è‚³‚ê‚Ä‚¢‚éUŒ‚•û–@‚ÅUŒ‚
        if (attack != null)
        {
            attack.TryAttack(target);
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
            // ƒvƒŒƒCƒ„[‚©‚ç“G‚Ü‚Å‚Ì‹——£‚ğŒvZ
            float distance = Vector3.SqrMagnitude(enemy.transform.position - transform.position);

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
        Gizmos.DrawWireSphere(transform.position, searchRange);
    }
}
