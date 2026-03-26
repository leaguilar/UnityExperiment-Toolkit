using UnityEngine;
using System;

public class CommandLineParser : MonoBehaviour
{
    [Tooltip("Specify a default config file path to use in the Editor when no command-line argument is provided.")]
    public string configFilePathEditor = "";

    public static string ConfigFilePath; // { get; set; }

    public void InitConfig(){
        Awake();
    }

    void Awake()
    {
        // Look for the argument that starts with "-config="
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

        // If no command-line argument is found, and we're in the Editor, use the inspector value.
        if (string.IsNullOrEmpty(ConfigFilePath))
        {
#if UNITY_EDITOR
            if (!string.IsNullOrEmpty(configFilePathEditor))
            {
                ConfigFilePath = configFilePathEditor;
                Debug.Log("Using config file path from Editor: " + ConfigFilePath);
            }
            else
            {
                Debug.LogError("No config file specified via command-line or in the Editor Inspector!");
            }
#else
            Debug.LogError("No config file specified on the command line!");
#endif
        }
    }
}
