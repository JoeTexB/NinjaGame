using UnityEngine;

public class KnifeScript : MonoBehaviour
{
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

    // Debug visualization
    private void OnDrawGizmosSelected()
    {
        // Visualize pickup radius in Scene view
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRadius);
    }

    void Start()
    {
        // Try to find the player and its state machine at start
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            playerStateMachine = player.GetComponent<PlayerStateMachine>();
        }
    }

    void Update()
    {
        if (isStickingToPlayer && playerTransform != null && playerStateMachine != null)
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
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player") && !isStickingToPlayer)
        {
            playerTransform = other.transform;
            playerStateMachine = other.GetComponent<PlayerStateMachine>();
            isStickingToPlayer = true;
        }
    }
}