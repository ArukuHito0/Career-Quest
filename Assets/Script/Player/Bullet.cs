using CareerQuest.Player;
using UnityEngine;

public class Bullet : MonoBehaviour , IBullet
{
    //  ’e‚Ìƒ_ƒ[ƒW
    [SerializeField] private float damage = 5f;

    // ’e‚ÌˆÚ“®‘¬“x
    [SerializeField] private float speed = 10f;

    // ’e‚ª”ò‚×‚éÅ‘å‹——£
    [SerializeField] private float maxDistance = 20f;

    //  ’e‚Ì‘å‚«‚³(“–‚½‚è”»’è—p)
    [SerializeField] private float tickness = 0.5f;


    // ”­Ë‚µ‚½ˆÊ’u
    private Vector3 startPosition;

    // ’e‚ª”ò‚Ô•ûŒü
    private Vector3 direction;

    public byte Damage { get; }
    public bool IsActive { get; }
    public float Tickness { get; }
    //  ’e‚ÌŒú‚İ(“–‚½‚è”»’è—p)
    public Vector3 Position { get; }

    private void Start()
    {
        // ”­ËˆÊ’u‚ğ‹L˜^
        startPosition = transform.position;
    }

    void OnEnable()
    {
        BulletManager.ActiveBullets.Add(this);
    }

    private void Update()
    {
        // ˆÚ“®
        transform.position += direction * speed * Time.deltaTime;

        // ”­Ë’n“_‚©‚ç‚Ì‹——£‚ğŒvZ
        float distance = Vector3.Distance(startPosition, transform.position);

        // Å‘å‹——£‚Ü‚Å”ò‚ñ‚¾‚çíœ
        if (distance >= maxDistance)
        {
            Destroy(gameObject);
        }
    }

    void OnDisable()
    {
        BulletManager.ActiveBullets.Remove(this);
    }

    // ”­Ë•ûŒü‚ğİ’è
    public void SetTarget(Transform target)
    {
        if (target == null)
            return;

        // ”­Ë‚Ì“G•ûŒü‚ğ‹L˜^
        direction = (target.position - transform.position).normalized;

        // ’e‚ÌŒü‚«‚ğˆÚ“®•ûŒü‚ÖŒü‚¯‚é
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }

    // ‰½‚©‚É“–‚½‚Á‚½
    private void OnTriggerEnter(Collider other)
    {
        // Enemyƒ^ƒO‚Ì“G‚É“–‚½‚Á‚½
        if (other.CompareTag("Enemy"))
        {
            Destroy(gameObject);
        }
    }
}
