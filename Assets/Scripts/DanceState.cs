using UnityEngine;

public class DanceState : PlayerBaseState
{
    public DanceState(PlayerStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Enter()
    { 
        // Play dance animation
        if (stateMachine.Animator != null)
        {
            stateMachine.Animator.Play("Dance");
        }
    }

    public override void Tick(float deltaTime)
    {
        // Add logic for what should happen during the Tick phase
        // For example, check for input to transition out of the dance state
        if (stateMachine.InputReader.IsDancedPressed() == false)
        {
            stateMachine.SwitchState(stateMachine.IdleState); // Example: Switch to IdleState
        }
    }

    public override void Exit()
    {
        // Add logic for what should happen when exiting the dance state
        // For example, stop the dance animation
        if (stateMachine.Animator != null)
        {
            stateMachine.Animator.SetTrigger("StopDance");
        }
    }
}