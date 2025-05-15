using UnityEngine;

public class KnifeManager : MonoBehaviour
{
    public GameObject knifePrefab; // Assign in Inspector
    private GameObject currentKnife;
    
    // Singleton pattern
    public static KnifeManager Instance { get; private set; }
    
    // Time tracking for emergency knife respawns
    private float lastKnifeCheckTime;
    private float knifeCheckInterval = 5.0f;
    
    void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Optional: persist between scenes
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        // Initial knife check 
        FindOrCreateKnife();
        lastKnifeCheckTime = Time.time;
    }
    
    void Update()
    {
        // Periodically check if we have a valid knife in the scene
        if (Time.time > lastKnifeCheckTime + knifeCheckInterval)
        {
            CheckKnifeAvailability();
            lastKnifeCheckTime = Time.time;
        }
        
        // Emergency knife spawning with K key
        if (Input.GetKeyDown(KeyCode.K))
        {
            SpawnNewKnife();
        }
        
        // Force immediate knife return with J key (useful for debugging)
        if (Input.GetKeyDown(KeyCode.J))
        {
            ForceImmediateReturn();
        }
        
        // Regular force return with R key
        if (Input.GetKeyDown(KeyCode.R))
        {
            ForceRetrieveKnife();
        }
    }
    
    // Check if the knife exists and is usable
    public void CheckKnifeAvailability()
    {
        GameObject knife = FindKnife();
        
        if (knife == null)
        {
            Debug.LogWarning("KnifeManager: No knife found in scene, creating one");
            SpawnNewKnife();
        }
        else if (knife.GetComponent<KnifeScript>() == null)
        {
            Debug.LogWarning("KnifeManager: Knife found but has no KnifeScript component");
            Destroy(knife);
            SpawnNewKnife();
        }
        else
        {
            // Knife exists and has script component
            currentKnife = knife;
        }
    }
    
    // Find a knife in the scene
    private GameObject FindKnife()
    {
        GameObject knife = GameObject.FindGameObjectWithTag("Knife");
        return knife;
    }
    
    // Find an existing knife or create a new one if needed
    private void FindOrCreateKnife()
    {
        GameObject knife = FindKnife();
        if (knife != null)
        {
            currentKnife = knife;
            Debug.Log("KnifeManager: Found existing knife: " + knife.name);
        }
        else
        {
            SpawnNewKnife();
        }
    }
    
    // Spawn a new knife at the player position
    public void SpawnNewKnife()
    {
        if (knifePrefab == null)
        {
            Debug.LogError("KnifeManager: No knife prefab assigned! Please assign in Inspector.");
            return;
        }
        
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("KnifeManager: Cannot spawn knife - no player found!");
            return;
        }
        
        // If we already have a knife, destroy it
        if (currentKnife != null)
        {
            Debug.Log("KnifeManager: Destroying old knife before spawning new one");
            Destroy(currentKnife);
        }
        
        // Spawn the new knife slightly above the player
        Vector3 spawnPosition = player.transform.position + new Vector3(0, 1.5f, 0);
        currentKnife = Instantiate(knifePrefab, spawnPosition, Quaternion.identity);
        
        // Ensure it has the Knife tag
        currentKnife.tag = "Knife";
        
        Debug.Log("KnifeManager: Spawned new knife: " + currentKnife.name);
    }
    
    // Force the knife to return with normal bounce
    public void ForceRetrieveKnife()
    {
        GameObject knife = FindKnife();
        
        if (knife != null)
        {
            KnifeScript knifeScript = knife.GetComponent<KnifeScript>();
            if (knifeScript != null)
            {
                knifeScript.ForceReturn();
                Debug.Log("KnifeManager: Force retrieved knife");
            }
            else
            {
                Debug.LogWarning("KnifeManager: Found knife but it has no KnifeScript component!");
                Destroy(knife);
                SpawnNewKnife();
            }
        }
        else
        {
            Debug.LogWarning("KnifeManager: No knife found to retrieve, spawning new one");
            SpawnNewKnife();
        }
    }
    
    // Force immediate teleport-return of knife to player with no bounce or delay
    private void ForceImmediateReturn()
    {
        GameObject knife = FindKnife();
        if (knife != null)
        {
            KnifeScript knifeScript = knife.GetComponent<KnifeScript>();
            if (knifeScript != null)
            {
                // We'll directly call private methods using reflection to force 
                // immediate attachment to player
                System.Type type = knifeScript.GetType();
                
                // First find player
                type.GetMethod("FindAndSetupPlayer", 
                    System.Reflection.BindingFlags.NonPublic | 
                    System.Reflection.BindingFlags.Instance)?.Invoke(knifeScript, null);
                
                // Then force pickup
                type.GetMethod("PickUpKnife", 
                    System.Reflection.BindingFlags.NonPublic | 
                    System.Reflection.BindingFlags.Instance)?.Invoke(knifeScript, null);
                
                Debug.Log("KnifeManager: Forced IMMEDIATE attachment of knife to player");
            }
            else
            {
                Debug.LogWarning("KnifeManager: Found knife but it has no KnifeScript component!");
                Destroy(knife);
                SpawnNewKnife();
            }
        }
        else
        {
            Debug.LogWarning("KnifeManager: No knife found for immediate return, spawning new one");
            SpawnNewKnife();
        }
    }
}
