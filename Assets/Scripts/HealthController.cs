using UnityEngine;

public class HealthController : MonoBehaviour
{
    private PlayerStateMachine playerStateMachine;
    private float damageTimer = 0f;
    private float damageInterval = 0.5f; // Damage every 0.5 seconds
    private bool isInContactWithSnail = false;
    private bool playerDead = false;
    public GameObject Player;
    
    void Start()
    {
        playerStateMachine = GetComponent<PlayerStateMachine>();
        if (playerStateMachine == null)
        {
            Debug.LogError("HealthController: Could not find PlayerStateMachine component!");
        }
        else
        {
            // Make sure health is initialized
            if (playerStateMachine.Health <= 0)
            {
                playerStateMachine.Health = 10;
            }
        }
        Player = GameObject.Find("Player");
        
    }

    void Update()
    {
        if (playerStateMachine == null) return;

        // Check for player death
        if (playerStateMachine.Health < 1 && !playerDead)
        {
            KillPlayer();
            return;
        }
        
        // If in contact with snail, reduce health every damageInterval seconds
        if (isInContactWithSnail)
        {
            damageTimer += Time.deltaTime;
            
            // Check if it's time to apply damage
            if (damageTimer >= damageInterval)
            {
                playerStateMachine.Health = Mathf.Max(0, playerStateMachine.Health - 1); // Decrease health by 1
                damageTimer = 0f; // Reset timer
                
                Debug.Log("HealthController: Health reduced to: " + playerStateMachine.Health);
                
                // Check if player died from this damage
                if (playerStateMachine.Health < 1)
                {
                    KillPlayer();
                }
            }
        }
        Player = GameObject.Find("Player");
        
        

    }

    // Handle player death
    private void KillPlayer()
    {
        playerDead = true;
        Debug.Log("Player has died!");
        
        // Destroy the player GameObject
        Destroy(Player);
    }

    // This method can be called directly from SnailScript
    public void TakeDamage(int amount)
    {
        if (playerStateMachine != null && !playerDead)
        {
            playerStateMachine.Health = Mathf.Max(0, playerStateMachine.Health - amount);
            Debug.Log("HealthController: Damage taken directly. Health: " + playerStateMachine.Health);
            
            // Check if player died from this damage
            if (playerStateMachine.Health < 1)
            {
                KillPlayer();
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (playerDead) return;
        
        Debug.Log("HealthController: Collision with " + collision.gameObject.name + " (tag: " + collision.gameObject.tag + ")");
        
        if (collision.gameObject.CompareTag("Snail"))
        {
            isInContactWithSnail = true;
            damageTimer = 0f; // Reset timer when contact begins
            Debug.Log("HealthController: Started contact with Snail");
        }
    }
    
    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Snail"))
        {
            isInContactWithSnail = false;
            damageTimer = 0f; // Reset timer when contact ends
            Debug.Log("HealthController: Ended contact with Snail");
        }
    }
    
    void OnCollisionStay2D(Collision2D collision)
    {
        // Periodically log that we're still in contact
        if (collision.gameObject.CompareTag("Snail") && Time.frameCount % 60 == 0)
        {
            Debug.Log("HealthController: Still in contact with Snail");
        }
    }
    
    // Alternative trigger-based detection if using triggers
    void OnTriggerEnter2D(Collider2D other)
    {
        if (playerDead) return;
        
        Debug.Log("HealthController: Trigger with " + other.name + " (tag: " + other.tag + ")");
        
        if (other.CompareTag("Snail"))
        {
            isInContactWithSnail = true;
            damageTimer = 0f;
            Debug.Log("HealthController: Started trigger contact with Snail");
        }
    }
    
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Snail"))
        {
            isInContactWithSnail = false;
            damageTimer = 0f;
            Debug.Log("HealthController: Ended trigger contact with Snail");
        }
    }
    
    void OnTriggerStay2D(Collider2D other)
    {
        // Periodically log that we're still in contact
        if (other.CompareTag("Snail") && Time.frameCount % 60 == 0)
        {
            Debug.Log("HealthController: Still in trigger contact with Snail");
        }
    }
}