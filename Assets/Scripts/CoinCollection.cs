using UnityEngine;

public class CoinCollection : MonoBehaviour
{
    public int CoinType;
    public SpriteRenderer spriteRenderer;

    
    void Start()
    {
        CoinType = 0;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        string spriteName = spriteRenderer.sprite.name;

        if (spriteName == "coinGold")
        {
            CoinType = 1;
        }
        else if (spriteName == "coinRed_0")
        {
            CoinType = 2;
        }
        else if (spriteName == "coinBlue_0")
        {
            CoinType = 3;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (CoinType == 1)
            {
                PlayerStateMachine.Instance.Coins += 1;
            }
            if CoinType == 2)
            {
                
            }
            if (CoinType == 3)
            {
                
            }



            Destroy(gameObject);
        }
    }
}