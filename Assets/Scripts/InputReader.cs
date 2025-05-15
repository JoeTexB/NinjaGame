using UnityEngine;

public class InputReader
{
    public bool CoinBlueInput = false;
    public bool CanDance = false;

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
        bool shootInput = Input.GetMouseButtonDown(0);
        if (shootInput)
        {
            Debug.Log("ShootPressed: Mouse button 0 was pressed!");
        }
        return shootInput;
    }

    public bool IsDancedPressed()
    {
        // Only allow dance if CanDance is true
        Debug.Log("CanDance: " + CanDance);
        return CanDance && Input.GetKey(KeyCode.J);
    }

    // Method to explicitly set CoinBlueInput
    public void SetCoinBlue(bool value)
    {
        CoinBlueInput = value;
    }

    public void SetCanDance(bool value)
    {
        Debug.Log("Setting CanDance to: " + value);
        CanDance = value;
    }
}