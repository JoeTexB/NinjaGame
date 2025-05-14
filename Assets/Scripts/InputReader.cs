using UnityEngine;

public class InputReader
{
    public bool CoinBlueInput = false;

    public Vector2 GetMovementInput()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        return new Vector2(horizontal, 0f);
    }

    public bool IsRunPressed()
    {
        // Only return Shift key state if CoinBlue is true
        Debug.Log("CoinBlueInput: " + CoinBlueInput);
        if (CoinBlueInput)
        {
            return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        }

        return false;
    }

    public bool IsJumpPressed()
    {
        return Input.GetButtonDown("Jump");
    }

    public bool IsCrouchHeld()
    {
        return Input.GetKey(KeyCode.C);
    }

    public bool IsShootPressed()
    {
        return Input.GetMouseButtonDown(0);
    }

    public bool IsDancedPressed()
    {
        return Input.GetKey(KeyCode.J);
    }

    // Method to explicitly set CoinBlueInput
    public void SetCoinBlue(bool value)
    {
        CoinBlueInput = value;
    }
}