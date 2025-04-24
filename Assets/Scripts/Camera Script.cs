using UnityEngine;

public class CameraScript : MonoBehaviour
{
    public GameObject player; 
    public Vector3 Offset;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
        Offset = transform.position - player.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        transform.position = player.transform.position + Offset;
    }
}
