using UnityEngine;
using System.Collections;

public class KnifeScript : MonoBehaviour
{
    // Static reference to track if player has a knife
    private static KnifeScript playerKnife = null;
    
    private Transform playerTransform;
    public Vector3 offset = new Vector3(0.5f, 0, 0);
    private bool isStickingToPlayer = false;
    private PlayerStateMachine playerStateMachine;

    // Throwing parameters
    public float throwSpeed = 15f;
    public float rotationSpeed = 720f;
    public float returnSpeed = 20f;
    public float pickupRadius = 2f; // Increased radius for easier pickup
    private bool isThrowing = false;
    private bool isReturning = false;
    private Vector3 targetPosition;
    private Vector2 throwDirection;
    private bool canBePickedUp = true;
    
    // Return delay after collision
    private float returnDelay = 1.0f; // Delay after collision
    private float maxFlightTime = 3.0f; // Maximum time knife can be in flight before returning
    private float throwStartTime; // When the throw started
    
    // Drop from lucky block settings
    public bool startAttachedToPlayer = false; // Set to false when dropped from a lucky block
    private float dropBounceForce = 5f; // Force applied when dropping from a block
    
    // Collision layers
    public LayerMask collisionMask; // Set this in inspector to exclude player layer
    private int playerLayer;
    
    // Rigidbody reference
    private Rigidbody2D rb;
    
    // Make sure the gameObject has the correct tag
    void Awake()
    {
        // Set the tag if it doesn't have it already
        if (gameObject.tag != "Knife")
        {
            gameObject.tag = "Knife";
            Debug.Log("KnifeScript: Set tag to 'Knife'");
        }
    }

    // Debug visualization
    private void OnDrawGizmosSelected()
    {
        // Visualize pickup radius in Scene view
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRadius);
    }

    void Start()
    {
        Debug.Log("KnifeScript: Starting initialization");
        
        // Try to find the player and its state machine at start
        FindAndSetupPlayer();
        
        // Get the rigidbody component
        SetupRigidbody();
        
        // Check for Collider2D
        SetupCollider();
        
        // If this knife should start attached to the player
        if (startAttachedToPlayer && playerTransform != null)
        {
            // Make the knife stick to the player from the start
            isStickingToPlayer = true;
            UpdateKnifePosition();
            Debug.Log("KnifeScript: Initial position set relative to player");
        }
        else
        {
            // If this knife is dropped from a lucky block, add some physics
            isStickingToPlayer = false;
            rb.isKinematic = false;
            rb.gravityScale = 0.7f; // Light gravity
            
            // Apply random force to make it bounce out
            Vector2 randomDirection = new Vector2(Random.Range(-1f, 1f), Random.Range(0.5f, 1f)).normalized;
            rb.AddForce(randomDirection * dropBounceForce, ForceMode2D.Impulse);
            
            Debug.Log("KnifeScript: Knife dropped from lucky block");
        }
        
        // Start periodic update check
        StartCoroutine(PeriodicStateCheck());
    }
    
    // Call this to drop the knife from a lucky block
    public void DropFromLuckyBlock(Vector3 position)
    {
        // First check if player already has a knife
        if (HasPlayerKnife())
        {
            Debug.Log("KnifeScript: Player already has a knife - destroying this one");
            Destroy(gameObject);
            return;
        }
        
        transform.position = position;
        startAttachedToPlayer = false;
        isStickingToPlayer = false;
        rb.isKinematic = false;
        rb.gravityScale = 0.7f; // Light gravity
        
        // Apply random force to make it bounce out
        Vector2 randomDirection = new Vector2(Random.Range(-1f, 1f), Random.Range(0.5f, 1f)).normalized;
        rb.AddForce(randomDirection * dropBounceForce, ForceMode2D.Impulse);
        
        // Add some rotation for effect
        rb.angularVelocity = Random.Range(-180f, 180f);
        
        Debug.Log("KnifeScript: Dropped from lucky block at " + position);
    }
    
    private void FindAndSetupPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            playerStateMachine = player.GetComponent<PlayerStateMachine>();
            playerLayer = player.layer;
            Debug.Log("KnifeScript: Found player - " + player.name);
        }
        else
        {
            Debug.LogError("KnifeScript: Could not find player with 'Player' tag!");
        }
    }
    
    private void SetupRigidbody()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.Log("KnifeScript: No Rigidbody2D found, adding one");
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        
        // Configure the rigidbody for knife physics
        rb.gravityScale = 0f; // Default gravity for attached knife
        rb.freezeRotation = false; // Allow rotation for visual effect
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous; // Better collision detection
        rb.interpolation = RigidbodyInterpolation2D.Interpolate; // Smoother movement
    }
    
    private void SetupCollider()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col == null)
        {
            Debug.LogError("KnifeScript: No Collider2D found on knife! Please add a BoxCollider2D or CircleCollider2D.");
            // Add a simple box collider as fallback
            BoxCollider2D boxCol = gameObject.AddComponent<BoxCollider2D>();
            boxCol.size = new Vector2(0.5f, 0.2f); // Small knife collider
        }
        
        // Make sure we have a trigger collider for pickup detection
        BoxCollider2D triggerCollider = gameObject.AddComponent<BoxCollider2D>();
        triggerCollider.isTrigger = true;
        triggerCollider.size = new Vector2(1f, 0.5f); // Slightly larger than the physical collider
        Debug.Log("KnifeScript: Added trigger collider for pickup detection");
    }
    
    // Periodic check to ensure we have valid references and state
    private IEnumerator PeriodicStateCheck()
    {
        while (true)
        {
            yield return new WaitForSeconds(1.0f);
            
            // If we've lost the player reference, try to find it again
            if (playerTransform == null)
            {
                FindAndSetupPlayer();
            }
            
            // Check if knife has been thrown but is stuck or out of bounds
            if (isThrowing && !isStickingToPlayer && !isReturning)
            {
                if (Time.time - throwStartTime > maxFlightTime)
                {
                    Debug.Log("KnifeScript: Flight time exceeded max limit, forcing return");
                    ForceReturn();
                }
            }
            
            // Sanity check: if knife is very far from player (100+ units), force return
            if (!isStickingToPlayer && playerTransform != null)
            {
                float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
                if (distanceToPlayer > 100f)
                {
                    Debug.Log("KnifeScript: Knife is too far from player, forcing return");
                    ForceReturn();
                }
            }
        }
    }

    void Update()
    {
        if (isStickingToPlayer && playerTransform != null && playerStateMachine != null)
        {
            UpdateKnifePosition();
        }
        else if (isThrowing)
        {
            // Apply rotation during flight
            transform.Rotate(Vector3.forward * rotationSpeed * Time.deltaTime);
            
            // Check if we've gone too long without hitting anything
            if (Time.time - throwStartTime > maxFlightTime)
            {
                Debug.Log("KnifeScript: Flight time exceeded max limit, forcing return");
                ForceReturn();
            }
        }
        else if (isReturning)
        {
            // Return to player logic
            if (playerTransform != null)
            {
                // Update target position (player might be moving)
                targetPosition = playerTransform.position;
                
                // Move towards player
                Vector3 returnDirection = (targetPosition - transform.position).normalized;
                rb.linearVelocity = returnDirection * returnSpeed;
                
                // Apply rotation during return
                transform.Rotate(Vector3.forward * -rotationSpeed * Time.deltaTime);
                
                // Check if close enough to be "picked up"
                float distanceToPlayer = Vector3.Distance(transform.position, targetPosition);
                if (distanceToPlayer < pickupRadius)
                {
                    PickUpKnife();
                }
                
                // Debug information
                Debug.DrawLine(transform.position, targetPosition, Color.green);
                if (Time.frameCount % 30 == 0) // Log every 30 frames to avoid spam
                {
                    Debug.Log("KnifeScript: Returning to player. Distance: " + distanceToPlayer);
                }
            }
            else
            {
                // Player reference lost, try to find it again
                FindAndSetupPlayer();
                
                if (playerTransform == null)
                {
                    // Still no player, stop returning
                    isReturning = false;
                    rb.linearVelocity = Vector2.zero;
                    Debug.LogError("KnifeScript: Cannot find player to return to!");
                }
            }
        }
    }
    
    // Called from ShootState to throw the knife
    public void ThrowKnife(Vector2 direction)
    {
        Debug.Log("KnifeScript: ThrowKnife called with direction " + direction);
        
        if (isStickingToPlayer)
        {
            isStickingToPlayer = false;
            isThrowing = true;
            isReturning = false;
            throwDirection = direction.normalized;
            throwStartTime = Time.time;
            
            // Enable physics and add force in throw direction
            rb.isKinematic = false;
            rb.linearVelocity = throwDirection * throwSpeed;
            
            // Adjust rotation to match throw direction
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            
            // Temporarily disable pickup until knife has traveled some distance
            canBePickedUp = false;
            
            // Ignore collisions with player while throwing
            if (playerTransform != null)
            {
                Collider2D playerCollider = playerTransform.GetComponent<Collider2D>();
                if (playerCollider != null)
                {
                    Physics2D.IgnoreCollision(playerCollider, GetComponent<Collider2D>(), true);
                    // Re-enable collision after a short delay
                    StartCoroutine(ReEnablePlayerCollision(playerCollider, 0.5f));
                }
            }
            
            Debug.Log("KnifeScript: Knife thrown with velocity " + rb.linearVelocity + " and angle " + angle);
            
            // Schedule a return after max flight time
            CancelInvoke("StartReturning"); // Cancel any previous invokes
            Invoke("StartReturning", maxFlightTime);
        }
        else
        {
            Debug.LogWarning("KnifeScript: Cannot throw knife - not sticking to player");
        }
    }
    
    // Coroutine to re-enable player collision after a delay
    private IEnumerator ReEnablePlayerCollision(Collider2D playerCollider, float delay)
    {
        yield return new WaitForSeconds(delay);
        Physics2D.IgnoreCollision(playerCollider, GetComponent<Collider2D>(), false);
        canBePickedUp = true;
        Debug.Log("KnifeScript: Re-enabled player collision");
    }
    
    // Update knife position relative to player
    private void UpdateKnifePosition()
    {
        // Flip the offset based on player direction
        Vector3 currentOffset = offset;
        if (!playerStateMachine.IsFacingRight)
        {
            currentOffset.x *= -1;
        }

        transform.position = playerTransform.position + currentOffset;
        
        // Match player's rotation/scale
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * (playerStateMachine.IsFacingRight ? 1 : -1);
        transform.localScale = scale;
        
        // Reset rotation when attached to player
        transform.rotation = Quaternion.identity;
        
        // Disable physics when attached to player
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.gravityScale = 0f;
        }
    }
    
    // Public method to return after a specific delay
    public void ReturnAfterDelay(float delay)
    {
        if (!isStickingToPlayer)
        {
            // Cancel any previous return invokes
            CancelInvoke("StartReturning");
            
            // If delay is 0 or negative, return immediately
            if (delay <= 0)
            {
                StartReturning();
            }
            else
            {
                // Otherwise invoke after delay
                Invoke("StartReturning", delay);
                Debug.Log("KnifeScript: Will return after " + delay + " seconds");
            }
        }
    }
    
    // Handle knife collisions
    void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("KnifeScript: Collision with " + collision.gameObject.name);
        
        // Ignore collisions with player if not being picked up
        if ((collision.gameObject.layer == playerLayer || collision.gameObject.CompareTag("Player")) && !canBePickedUp)
        {
            Physics2D.IgnoreCollision(collision.collider, GetComponent<Collider2D>());
            Debug.Log("KnifeScript: Ignoring collision with player");
            return;
        }
        
        // Only process if we're currently throwing
        if (isThrowing)
        {
            isThrowing = false;
            
            // Calculate reflection direction
            Vector2 incomingDirection = throwDirection;
            Vector2 surfaceNormal = collision.contacts[0].normal;
            Vector2 reflectedDirection = Vector2.Reflect(incomingDirection, surfaceNormal);
            
            // Apply a bounce force in the reflected direction
            rb.linearVelocity = reflectedDirection * throwSpeed * 0.5f;
            
            // The knife will now return immediately after a short bounce delay
            ReturnAfterDelay(returnDelay);
            
            Debug.Log("KnifeScript: Knife hit object and will return after " + returnDelay + " seconds");
        }
    }
    
    // Override the standard collision handler with a trigger version too
    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("KnifeScript: Trigger with " + other.gameObject.name);
        
        // If this is the player and we're not sticking to the player, get picked up
        if (other.gameObject.CompareTag("Player") && !isStickingToPlayer && canBePickedUp && isReturning)
        {
            Debug.Log("KnifeScript: Player trigger detected for pickup");
            playerTransform = other.transform;
            playerStateMachine = other.GetComponent<PlayerStateMachine>();
            PickUpKnife();
            return;
        }
        
        // If we hit any other trigger collider (that isn't the player) during throwing, bounce and return
        if (isThrowing && !other.gameObject.CompareTag("Player") && !other.isTrigger)
        {
            isThrowing = false;
            
            // Calculate a simple bounce direction (away from the center of the collider)
            Vector2 bounceDirection = (transform.position - other.transform.position).normalized;
            
            // Apply a small bounce force
            rb.linearVelocity = bounceDirection * throwSpeed * 0.3f;
            
            // The knife will return after a very short delay
            ReturnAfterDelay(returnDelay);
            
            Debug.Log("KnifeScript: Knife hit trigger object and will return after " + returnDelay + " seconds");
        }
    }
    
    private void StartReturning()
    {
        if (!isStickingToPlayer)
        {
            CancelInvoke("StartReturning"); // Make sure we don't have multiple return calls
            isThrowing = false;
            isReturning = true;
            Debug.Log("KnifeScript: Knife is now returning to player");
        }
    }
    
    // Force the knife to return immediately (can be called from outside)
    public void ForceReturn()
    {
        if (!isStickingToPlayer)
        {
            // Cancel any pending returns and start returning immediately
            ReturnAfterDelay(0);
        }
    }
    
    private void PickUpKnife()
    {
        // When a knife is picked up, it sticks to the player
        isReturning = false;
        isThrowing = false;
        isStickingToPlayer = true;
        
        // Set this as the player's knife
        playerKnife = this;
        
        // Destroy any other knives in the scene
        DestroyOtherKnives();
        
        // Reset physics
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.isKinematic = true;
        rb.gravityScale = 0f; // Reset gravity
        
        // Update position to attach to player
        UpdateKnifePosition();
        
        // Make self a child of the player for hierarchy organization
        transform.SetParent(playerTransform);
        
        Debug.Log("KnifeScript: Knife picked up by player");
    }
    
    // Reset static reference when knife is destroyed
    void OnDestroy()
    {
        CancelInvoke();
        
        // Clear static reference if this is the player's knife
        if (playerKnife == this)
        {
            playerKnife = null;
        }
    }
    
    // If object is disabled, make sure we clean up any pending invokes
    void OnDisable()
    {
        CancelInvoke();
    }
    
    // Static method to check if player already has a knife
    public static bool HasPlayerKnife()
    {
        // Check if we have a stored reference that's still valid
        if (playerKnife != null)
        {
            return true;
        }
        
        // Otherwise search for any knife attached to the player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            // Find all knives in the scene
            KnifeScript[] allKnives = GameObject.FindObjectsOfType<KnifeScript>();
            foreach (KnifeScript knife in allKnives)
            {
                // If any knife is sticking to the player, return true
                if (knife.isStickingToPlayer && knife.playerTransform == player.transform)
                {
                    playerKnife = knife;
                    return true;
                }
            }
        }
        
        return false;
    }
    
    // Method to destroy other knives in the scene
    private void DestroyOtherKnives()
    {
        KnifeScript[] allKnives = GameObject.FindObjectsOfType<KnifeScript>();
        foreach (KnifeScript knife in allKnives)
        {
            // Skip the current knife
            if (knife != this)
            {
                Debug.Log("KnifeScript: Destroying extra knife: " + knife.gameObject.name);
                Destroy(knife.gameObject);
            }
        }
    }
}