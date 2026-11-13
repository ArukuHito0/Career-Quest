using UnityEngine;
using CareerQuest.Player;

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
    bool _isActive = true;

    // ”­Ë‚µ‚½ˆÊ’u
    private Vector3 startPosition;

    // ’e‚ª”ò‚Ô•ûŒü
    private Vector3 direction;

    public byte Damage { get => (byte)damage;}
    public Vector3 Position { get => this.transform.position; }

    private void Start()
    {
        // ”­ËˆÊ’u‚ğ‹L˜^
        startPosition = transform.position;
    }

    void Update()
    {
        transform.position += direction * speed * Time.deltaTime;

        float distance = Vector3.Distance(startPosition, transform.position);
        if (distance >= maxDistance)
        {
            Destroy(this.gameObject);
        }
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

    public void Destroy()
    {
        Destroy(this.gameObject);
    }

    //‰½‚©‚É“–‚½‚Á‚½
    private void OnTriggerEnter(Collider other)
    {
        // Enemyƒ^ƒO‚Ì“G‚É“–‚½‚Á‚½
        if (other.CompareTag("Enemy"))
        {
            Destroy(gameObject);
        }
    }
}
