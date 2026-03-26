using UnityEngine;

public class ConfigManager : MonoBehaviour
{
    public static ConfigManager Instance { get; private set; }
    public ConfigData Config { get; private set; }

    private void Awake()
    {
        // Check if an instance already exists
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist this object across scenes
        }
        else
        {
            // If the instance already exists, destroy the new one to avoid duplicates
            if (Application.isEditor)
            {
                Destroy(gameObject);
                return;
            }
        }
    }

    private void OnEnable()
    {
        // Ensure the instance is reset when entering play mode
        if (Application.isPlaying)
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
        }
    }

    private void OnApplicationQuit()
    {
        // Reset the instance when quitting the application
        Instance = null;
    }

    public void SetConfig(ConfigData config)
    {
        Config = config;
    }
}
