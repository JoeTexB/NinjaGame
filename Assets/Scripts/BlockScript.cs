using UnityEngine;

public class BlockScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private Rigidbody rb;
    void Start()
    {
       rb = GetComponent<Rigidbody>();
       rb.useGravity = false;
       //rb.gravityScale = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Debug.Log("Player hit the block");
            //other.gameObject.GetComponent<PlayerStateMachine>().SwitchState(other.gameObject.GetComponent<PlayerStateMachine>().LuckyBlockState);
            rb.useGravity = true;
            
        }
    }
}
