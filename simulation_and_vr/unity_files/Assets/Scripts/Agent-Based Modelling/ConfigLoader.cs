#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System;
using System.Reflection;
using System.Collections.Generic;

[System.Serializable]
public class TaskData
{
    public string start;
    public string end;
    public string pointsOfInterest;
    public int numberOfAgents;
    public int spawnInterval;
    public string taskName;
    public string agentType;
    public int numberOfNeeds;
    public float agentSpeed;
    public float agentSize;
    public float agentRadius;
    public float privacyRadius;
    public float poiTime;
    public bool revisit;
    public bool chooseNonDeterministically;
    public string taskColor;
}

[System.Serializable]
public class ConfigData
{
    public string Scene;
    public string Scenario;
    public string Purpose;
    public int EngineScript_sampleInterval;
    public int simId;
    public int sampleNum;
    public List<TaskData> tasks;
}

[ExecuteInEditMode]
public class ConfigLoader : MonoBehaviour
{
    [Tooltip("Specify a default config file path to use in the Editor when no command-line argument is provided.")]
    public string configFilePathEditor = "";
    public ConfigData configData;
    public static string ConfigFilePath { get; private set; }

    void InitConfig()
    {
        string[] args = Environment.GetCommandLineArgs();
        foreach (string arg in args)
        {
            if (arg.StartsWith("-config="))
            {
                ConfigFilePath = arg.Substring("-config=".Length);
                Debug.Log("Config file path from command line: " + ConfigFilePath);
                break;
            }
        }

        if (string.IsNullOrEmpty(ConfigFilePath))
        {
            if (!string.IsNullOrEmpty(configFilePathEditor))
            {
                ConfigFilePath = configFilePathEditor;
                Debug.Log("Using config file path from Editor: " + ConfigFilePath);
            }
            else
            {
                Debug.LogError("No config file specified via command-line or in the Editor Inspector!");
            }
        }
    }

public void LoadSceneIfNotLoaded(string sceneToLoad)
    {
        Scene scene = SceneManager.GetSceneByName(sceneToLoad);

        if (scene.IsValid() && scene.isLoaded)
        {
            SceneManager.SetActiveScene(scene);
            Debug.Log("Scene already loaded and set as active: " + sceneToLoad);
        }
        else
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                // Load the scene additively in edit mode
                var loadedScene = EditorSceneManager.OpenScene(sceneToLoad, OpenSceneMode.Additive);
                EditorSceneManager.SetActiveScene(loadedScene);
                Debug.Log("Scene loaded and set as active in Edit Mode: " + sceneToLoad);
            }
            else
#endif
            {
                // Runtime loading
                SceneManager.sceneLoaded += OnSceneLoaded;
                SceneManager.LoadScene(sceneToLoad, LoadSceneMode.Additive);
            }
        }

        void OnSceneLoaded(Scene loadedScene, LoadSceneMode mode)
        {
            if (loadedScene.name == sceneToLoad)
            {
                SceneManager.SetActiveScene(loadedScene);
                Debug.Log("Scene loaded and set as active: " + sceneToLoad);
                SceneManager.sceneLoaded -= OnSceneLoaded;
            }
        }
    }

// Helper method to get a scene by name
private Scene GetSceneByName(string sceneName)
{
    for (int i = 0; i < SceneManager.sceneCount; i++)
    {
        Scene scene = SceneManager.GetSceneAt(i);
        if (scene.name == sceneName)
        {
            return scene;
        }
    }
    return default(Scene); // Return an invalid scene if not found
}

    void Start()
    {
        InitConfig();

        if (string.IsNullOrEmpty(ConfigFilePath))
        {
            Debug.LogError("No config file path found.");
            return;
        }

        if (!File.Exists(ConfigFilePath))
        {
            Debug.LogError("Config file not found: " + ConfigFilePath);
            return;
        }

        string json = File.ReadAllText(ConfigFilePath);
        configData = JsonUtility.FromJson<ConfigData>(json);

        if (configData == null)
        {
            Debug.LogError("Failed to parse config file. Ensure the JSON structure matches 'ConfigData'.");
            return;
        }

        UnityEngine.Random.InitState(configData.simId);
        Debug.Log("Loaded config for scene: " + configData.Scene);
        PrintClassVariables(configData);

        for (int i = 0; i < configData.tasks.Count; i++)
        {
            Debug.Log($"## Task # {i+1} data");
            PrintClassVariables(configData.tasks[i]);
        }

        if (ConfigManager.Instance != null)
        {
            ConfigManager.Instance.SetConfig(configData);
        }

        LoadSceneIfNotLoaded(configData.Scene);
    }

    void PrintClassVariables(object obj)
    {
        Type type = obj.GetType();
        FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        foreach (var field in fields)
        {
            var value = field.GetValue(obj);
            Debug.Log($"Field: {field.Name}, Value: {value}");
        }
    }
}
