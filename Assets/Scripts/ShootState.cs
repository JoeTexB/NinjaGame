using UnityEngine;

public class ShootState : PlayerBaseState
{
    // Store reference to the state machine
    private bool hasThrown = false;
    private KnifeScript knifeScript;
    
    // Time window to stay in shoot state
    private float shootStateTime = 0.5f;
    private float enterTime;
    
    // Animation name - should be different from wall cling animation
    private string shootAnimationName = "Shoot"; // Change to match your actual shoot animation

    public ShootState(PlayerStateMachine stateMachine) : base(stateMachine) { }

    public override void Enter()
    {
        // Reset state when entering
        hasThrown = false;
        enterTime = Time.time;
        
        Debug.Log("Entered ShootState - Looking for knife attached to player");
        
        // Play shoot animation - NOT using the "Cling" animation
        if (stateMachine.Animator != null)
        {
            // Try to play Shoot animation if it exists, otherwise fallback to Idle
            if (HasAnimation(shootAnimationName))
            {
                stateMachine.Animator.Play(shootAnimationName);
            }
            else
            {
                // Fallback to idle if shoot animation doesn't exist
                stateMachine.Animator.Play("Idle");
                Debug.LogWarning("ShootState: No '" + shootAnimationName + "' animation found, using Idle instead.");
            }
        }
        
        // Look for knife attached to the player
        knifeScript = null;
        
        // Check if the player has a knife as a child object
        foreach (Transform child in stateMachine.transform)
        {
            KnifeScript knife = child.GetComponent<KnifeScript>();
            if (knife != null)
            {
                knifeScript = knife;
                Debug.Log("Found knife attached to player: " + child.name);
                break;
            }
        }
        
        // If no knife found as a child, player doesn't have a knife
        if (knifeScript == null)
        {
            Debug.LogWarning("No knife found attached to player. Player must pick up a knife first.");
            // Switch back to appropriate state immediately
            stateMachine.SwitchState(stateMachine.IdleState);
            return;
        }
        
        // Only throw if we have a knife
        if (knifeScript != null)
        {
            // Calculate throw direction based on player facing
            Vector2 throwDirection = stateMachine.IsFacingRight ? Vector2.right : Vector2.left;
            
            // If aiming up
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            {
                throwDirection = new Vector2(throwDirection.x * 0.5f, 1f).normalized;
            }
            // If aiming down (works both on ground and in air now)
            else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            {
                throwDirection = new Vector2(throwDirection.x * 0.5f, -1f).normalized;
            }
            
            // Throw the knife
            knifeScript.ThrowKnife(throwDirection);
            hasThrown = true;
            
            Debug.Log("Player threw knife in direction: " + throwDirection);
        }
    }

    public override void Tick(float deltaTime)
    {
        // Allow player to move while in shoot state (reduced control)
        Vector2 moveInput = stateMachine.InputReader.GetMovementInput();
        float moveSpeed = stateMachine.MoveSpeed * 0.5f; // Half speed during shooting
        
        // Apply movement
        if (stateMachine.RB != null)
        {
            stateMachine.RB.linearVelocity = new Vector2(
                moveInput.x * moveSpeed,
                stateMachine.RB.linearVelocity.y); // Preserve Y velocity
        }
        
        // If we've been in the shoot state long enough, transition back
        if (Time.time - enterTime > shootStateTime)
        {
            CheckSwitchStates();
        }
    }

    public override void Exit()
    {
        Debug.Log("Player exited Shoot State");
        // Clear knife reference on exit to ensure we find the current knife next time
        knifeScript = null;
    }
    
    // Helper method to check if an animation exists
    private bool HasAnimation(string animName)
    {
        if (stateMachine.Animator != null)
        {
            // Check the animator's controller for the animation
            var controller = stateMachine.Animator.runtimeAnimatorController;
            var clips = controller.animationClips;
            foreach (var clip in clips)
            {
                if (clip.name == animName)
                    return true;
            }
        }
        return false;
    }

    // Helper method for transition checks
    private void CheckSwitchStates()
    {
        // Always transition out after throwing
        if (hasThrown)
        {
            // Jump takes priority if pressed
            if (stateMachine.InputReader.IsJumpPressed() && stateMachine.JumpsRemaining > 0)
            {
                stateMachine.SwitchState(stateMachine.JumpState);
                return;
            }

            if (stateMachine.IsGrounded())
            {
                if (stateMachine.InputReader.IsCrouchHeld())
                {
                    stateMachine.SwitchState(stateMachine.CrouchState);
                }
                else
                {
                    Vector2 moveInput = stateMachine.InputReader.GetMovementInput();
                    if (moveInput == Vector2.zero)
                        stateMachine.SwitchState(stateMachine.IdleState);
                    else if (stateMachine.InputReader.IsRunPressed())
                        stateMachine.SwitchState(stateMachine.RunState);
                    else
                        stateMachine.SwitchState(stateMachine.WalkState);
                }
            }
            else
            {
                // Airborne: check for wall cling or fall
                if (stateMachine.IsTouchingWall() && stateMachine.RB.linearVelocity.y <= 0)
                {
                    stateMachine.SwitchState(stateMachine.WallClingState);
                }
                else
                {
                    stateMachine.SwitchState(stateMachine.FallState);
                }
            }
        }
    }
}