using UnityEngine;

public class KnifeReturnHelper : MonoBehaviour
{
    private KnifeScript knifeScript;
    
    void Start()
    {
        // Don't try to find the knife in Start to avoid race condition
    }
    
    void Update()
    {
        // Press R key to force the knife to return
        if (Input.GetKeyDown(KeyCode.R))
        {
            // Find knife if necessary
            if (knifeScript == null)
            {
                GameObject knife = GameObject.FindGameObjectWithTag("Knife");
                if (knife != null)
                {
                    knifeScript = knife.GetComponent<KnifeScript>();
                    Debug.Log("KnifeReturnHelper: Found knife: " + knife.name);
                }
                else
                {
                    Debug.LogWarning("KnifeReturnHelper: No knife found with 'Knife' tag");
                    return;
                }
            }
            
            // Force knife to return
            if (knifeScript != null)
            {
                knifeScript.ForceReturn();
                Debug.Log("KnifeReturnHelper: Forced knife to return");
            }
        }
    }
}
