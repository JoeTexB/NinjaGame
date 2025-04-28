using UnityEngine;

public class KnifeScript : MonoBehaviour
{
    private Transform playerTransform;
    public Vector3 offset = new Vector3(0.5f, 0, 0);
    private bool isStickingToPlayer = false;
    private PlayerStateMachine playerStateMachine; // Reference to player state machine

    void Start()
    {
        // Try to find the player and its state machine at start
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
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