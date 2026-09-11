using UnityEngine;

public class TreasureChest : MonoBehaviour
{
    [SerializeField] private float carrentHealth = 100f;
    public float takeDamage;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        takeDamage = 10f;   // とりあえずダメージを10で固定
    }

    // Update is called once per frame
    void Update()
    {
        if (carrentHealth < 0)
        {
            Destroy(gameObject);
        }
    }

    public void TakeDamage()
    {
        carrentHealth -= takeDamage;
    }
}
