using UnityEngine;
using TMPro;

public class HUDScript : MonoBehaviour
{
    public TMP_Text CoinsText;
    public TMP_Text HealthText;

    void Update()
    {
        if (PlayerStateMachine.Instance != null && CoinsText != null)
        {
            CoinsText.text = "Coins: " + PlayerStateMachine.Instance.Coins;
            HealthText.text = "Health: " + PlayerStateMachine.Instance.Health;
        }
    }
}