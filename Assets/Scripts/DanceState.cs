using UnityEngine;

public class DanceState : PlayerBaseState
{
    private float enterTime;
    private float danceDuration = 4f; // 4 seconds
    private Vector3 originalScale;
    
    // Reference to the player's visual model transform (not the collider)
    private Transform visualModel;
    private Vector3 originalModelScale;
    
    // Adjust this value to change the player's size during dance
    private float danceScaleMultiplier = 7.0f; // Uniform scale multiplier for all dimensions

    public DanceState(PlayerStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Enter()
    { 
        enterTime = Time.time;
        
        // First, try to find the visual model (typically a child with SpriteRenderer or SkinnedMeshRenderer)
        if (visualModel == null)
        {
            // Look for the GameObject with the Animator component
            if (stateMachine.Animator != null)
            {
                visualModel = stateMachine.Animator.transform;
            }
            // If no specific visual model found, default to the first child
            if (visualModel == null && stateMachine.transform.childCount > 0)
            {
                visualModel = stateMachine.transform.GetChild(0);
            }
        }
        
        // If we have a visual model separate from the collider object
        if (visualModel != null && visualModel != stateMachine.transform)
        {
            // Store original scale of the visual model
            originalModelScale = visualModel.localScale;
            
            // Get the direction (positive or negative) of the X scale to maintain facing direction
            float xDirection = Mathf.Sign(originalModelScale.x);
            
            // Apply uniform scaling while preserving facing direction
            visualModel.localScale = new Vector3(
                Mathf.Abs(originalModelScale.x) * danceScaleMultiplier * xDirection, 
                Mathf.Abs(originalModelScale.y) * danceScaleMultiplier, 
                Mathf.Abs(originalModelScale.z) * danceScaleMultiplier);
            
            Debug.Log("Dance state entered. Set visual model scale to: " + visualModel.localScale);
        }
        else
        {
            // Fallback: Store the player's original scale (whole GameObject)
            originalScale = stateMachine.transform.localScale;
            
            // Get the direction of the X scale
            float xDirection = stateMachine.IsFacingRight ? 1.0f : -1.0f;
            
            // Apply a more cautious scale to the entire GameObject to avoid physics issues
            // but still make the dance somewhat visible
            stateMachine.SetPlayerScale(new Vector3(
                xDirection * 1.5f,  // Slightly larger X but preserve direction
                1.5f,               // Slightly larger Y 
                1.0f));             // Keep Z unchanged
            
            Debug.Log("Dance state entered. No separate visual model found, using cautious scale.");
        }

        // Play dance animation
        if (stateMachine.Animator != null)
        {
            stateMachine.Animator.Play("Dance");
        }
    }

    public override void Tick(float deltaTime)
    {
        // Only allow exit after 4 seconds
        if (Time.time - enterTime >= danceDuration)
        {
            // After 4 seconds, allow transition out if J is released
            if (!stateMachine.InputReader.IsDancedPressed())
            {
                stateMachine.SwitchState(stateMachine.IdleState); // Or previous state
            }
        }
        // Otherwise, do nothing (stay in dance)
    }

    public override void Exit()
    {
        // Restore original scale
        if (visualModel != null && visualModel != stateMachine.transform)
        {
            // Restore the visual model's original scale
            visualModel.localScale = originalModelScale;
            Debug.Log("Dance state exited. Restored visual model scale to: " + originalModelScale);
        }
        else
        {
            // Restore the whole player object's scale
            stateMachine.SetPlayerScale(originalScale);
            Debug.Log("Dance state exited. Restored player scale to: " + originalScale);
        }

        // Stop the dance animation if needed
        if (stateMachine.Animator != null)
        {
            stateMachine.Animator.SetTrigger("StopDance");
        }
    }
}