using UnityEngine;

public class BlockScript : MonoBehaviour
{
    private Rigidbody2D rb;
    private float ContactCoolDown;

    public GameObject[] LootList; // Array of possible loot prefabs

    private int TouchesBlock;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        ContactCoolDown = 2f;
        TouchesBlock = 0;
    }

    void Update()
    {
        ContactCoolDown -= Time.deltaTime;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TouchesBlock++;
        if (collision.gameObject.CompareTag("Player"))
        {
            rb.gravityScale = 1;

            if (ContactCoolDown < 0)
            {
                Debug.Log("Player hit the block");
                // Optional: Call state switch here
            }
        }

        if (TouchesBlock >= 2)
        {
            if (LootList.Length > 0)
            {
                int randomIndex = Random.Range(0, LootList.Length);
                Instantiate(LootList[randomIndex], transform.position, Quaternion.identity);
                Destroy(gameObject);
            }

            Destroy(gameObject);
        }
    }
}