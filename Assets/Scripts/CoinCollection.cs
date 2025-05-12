using UnityEngine;

public class CoinCollection : MonoBehaviour
{
    public int CoinType;
    public SpriteRenderer spriteRenderer;

    public bool CoinBlue = false;
    public bool CoinRed = false;

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
        Debug.Log("CoinType: " + CoinType);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Debug.Log("Player collected a coin!");
            if (CoinType == 1)
            {
                PlayerStateMachine.Instance.Coins += 1;
            }
            else if (CoinType == 2)
            {
               CoinRed = true;
            }
            else if (CoinType == 3)
            {
                
                 CoinBlue = true;
                Debug.Log("Blue coin collected. Setting CoinBlue in InputReader.");
                PlayerStateMachine.Instance.RunState.SetCoinBlue(true);
                

                // Notify the InputReader that CoinBlue is true
                PlayerStateMachine.Instance.InputReader.SetCoinBlue(true);
            }

            Destroy(gameObject);
        }
    }
}