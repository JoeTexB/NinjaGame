using UnityEngine;
using TMPro;

public class HUDScript : MonoBehaviour
{
    public TMP_Text CoinsText;
    public GameObject HealthText;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        CoinsText.text = "Coins: " + PlayerStateMachine.Instance.Coins;
        
    }
}
