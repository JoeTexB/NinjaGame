using UnityEngine;

public class SnailScript : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float detectionRange = 5f;
    [SerializeField] private LayerMask obstacleLayer;
    
    [Header("Combat")]
    [SerializeField] private int damageAmount = 1;
    [SerializeField] private float damageInterval = 0.5f; // Damage every 0.5 seconds
    
    private Transform playerTransform;
    private Rigidbody2D rb;
    private bool isFacingRight = true;
    private bool isDead = false;
    
    private float lastDamageTime = 0f;
    private HealthController playerHealth;
    private PlayerStateMachine playerStateMachine;
    
    void Start()
    {
        // Set the correct tag
        if (gameObject.tag != "Snail")
        {
            gameObject.tag = "Snail";
            Debug.Log("SnailScript: Setting tag to 'Snail'");
        }
        
        // Get or add Rigidbody2D component
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 1f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
        
        // Find player if not assigned
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
                
                // Get the health controller
                playerHealth = player.GetComponent<HealthController>();
                playerStateMachine = player.GetComponent<PlayerStateMachine>();
                
                if (playerHealth == null && playerStateMachine == null)
                {
                    Debug.LogWarning("SnailScript: Player doesn't have HealthController or PlayerStateMachine component");
                }
            }
        }
        
        // Add BoxCollider2D if not present
        if (GetComponent<BoxCollider2D>() == null)
        {
            BoxCollider2D collider = gameObject.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(1f, 0.8f);
            collider.offset = new Vector2(0f, 0.4f);
            collider.isTrigger = false; // Make sure it's not a trigger
        }
    }

    void Update()
    {
        if (isDead) return;
        
        if (playerTransform != null)
        {
            float distanceToPlayer = Mathf.Abs(playerTransform.position.x - transform.position.x);
            
            // Check if player is within detection range
            if (distanceToPlayer <= detectionRange)
            {
                MoveTowardsPlayer();
            }
            else
            {
                // Stop when player is out of range
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            }
        }
    }
    
    void MoveTowardsPlayer()
    {
        if (playerTransform == null) return;
        
        float direction = Mathf.Sign(playerTransform.position.x - transform.position.x);
        
        // Use Transform-based movement for reliability
        transform.Translate(new Vector3(direction * moveSpeed * Time.deltaTime, 0, 0));
        
        // Flip sprite based on movement direction
        if ((direction > 0 && !isFacingRight) || (direction < 0 && isFacingRight))
        {
            Flip();
        }
    }
    
    void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }
    
    // Active damage approach: the snail damages the player directly
    void OnCollisionStay2D(Collision2D collision)
    {
        // Check if in contact with player
        if (collision.gameObject.CompareTag("Player") && !isDead)
        {
            // Apply damage at interval
            if (Time.time >= lastDamageTime + damageInterval)
            {
                lastDamageTime = Time.time;
                Debug.Log("SnailScript: Attempting to damage player");
                
                // Try to get references if we don't have them
                if (playerHealth == null || playerStateMachine == null)
                {
                    playerHealth = collision.gameObject.GetComponent<HealthController>();
                    playerStateMachine = collision.gameObject.GetComponent<PlayerStateMachine>();
                }
                
                // Apply damage using HealthController if available
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(damageAmount);
                    Debug.Log("SnailScript: Applied damage via HealthController");
                }
                // Directly modify PlayerStateMachine's Health if HealthController not available
                else if (playerStateMachine != null)
                {
                    playerStateMachine.Health = Mathf.Max(0, playerStateMachine.Health - damageAmount);
                    Debug.Log("SnailScript: Applied damage directly to PlayerStateMachine. New health: " + playerStateMachine.Health);
                }
                else
                {
                    Debug.LogWarning("SnailScript: Could not find way to damage player");
                }
            }
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        // Check if hit by knife
        if (other.CompareTag("Knife") && !isDead)
        {
            Die();
        }
    }
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("SnailScript: Collision with " + collision.gameObject.name + " (tag: " + collision.gameObject.tag + ")");
        
        // Alternative knife detection
        if (collision.gameObject.CompareTag("Knife") && !isDead)
        {
            Die();
        }
    }
    
    void Die()
    {
        isDead = true;
        
        // Disable movement
        rb.linearVelocity = Vector2.zero;
        
        // Destroy the snail
        Destroy(gameObject, 0.5f);
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
