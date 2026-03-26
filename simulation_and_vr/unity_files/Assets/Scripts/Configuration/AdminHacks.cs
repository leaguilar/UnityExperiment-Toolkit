using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityStandardAssets.Characters.FirstPerson;
using Object = UnityEngine.Object;

public class AdminHacks : MonoBehaviour
{
    private const float DefaultWalkSpeed = 1.3f;

    private const float FastWalkSpeed = 4f;

    private const float HighWalkSpeed = 7f;
    
    private const KeyCode DefaultSpeedKey = KeyCode.I;
         
    private const KeyCode FastSpeedKey = KeyCode.O;
     
    private const KeyCode HighSpeedKey = KeyCode.P;
     
    private const KeyCode LoadSetup = KeyCode.Alpha1;
         
    private const KeyCode LoadPractice = KeyCode.Alpha2;
         
    private const KeyCode LoadTestBase = KeyCode.Alpha3;
       
    private const KeyCode LoadTestAtrium = KeyCode.Alpha4;
        
    private const KeyCode LoadTestGlass = KeyCode.Alpha5;

    private const KeyCode LoadEnd = KeyCode.Alpha6;

    private const float MessageStayDuration = 6f;

    private const int BorderSize = 4;

    [CanBeNull] private static GUIStyle consoleStyle;

    // 64859972
    private static readonly KeyCode[] AdminKeys = new[]
    {
        KeyCode.Alpha6, 
        KeyCode.Alpha4, 
        KeyCode.Alpha8, 
        KeyCode.Alpha5, 
        KeyCode.Alpha9, 
        KeyCode.Alpha9,
        KeyCode.Alpha7, 
        KeyCode.Alpha2
    };

    private static readonly Command[] Commands = 
    {
        new Command(KeyCode.H, ShowHelp, "Shows help info."), 
        new Command(KeyCode.Delete, ExitAdminMode, "Exit admin mode."),
        new Command(KeyCode.L, () => SetWalkSpeed(DefaultWalkSpeed), "Set walk speed to Default."), 
        new Command(KeyCode.O, DecreaseWalkSpeed, "Decrease walk speed."), 
        new Command(KeyCode.P, IncreaseWalkSpeed, "Increase walk speed."), 
        new Command(KeyCode.Alpha1, () => LoadScene("Setup"), "Load \"Setup\" scene."), 
        new Command(KeyCode.Alpha2, () => LoadScene("Practice"), "Load \"Practice\" scene."), 
        new Command(KeyCode.Alpha3, () => LoadScene("Zollverein_Base"), "Load \"Zollverein_Base\" scene."), 
        new Command(KeyCode.Alpha4, () => LoadScene("Zollverein_Atrium"), "Load \"Zollverein_Atrium\" scene."), 
        new Command(KeyCode.Alpha5, () => LoadScene("Zollverein_Glass"), "Load \"Zollverein_Glass\" scene."), 
        new Command(KeyCode.Alpha6, () => LoadScene("End"), "Load \"End\" scene."), 
    };

    private static readonly List<DebugMessage> Messages = new List<DebugMessage>(24);

    private static bool isAdmin;

    private static int currentPasswordIndex;

    private void OnEnable()
    {
        currentPasswordIndex = 0;
    }

    private void Update()
    {
        if (!isAdmin)
        {
            TestForLogin();
            return;
        }

        TestForCommand();
    }

    private static void TestForLogin()
    {
        if (Input.anyKeyDown)
        {
            if (currentPasswordIndex < 0 || currentPasswordIndex >= AdminKeys.Length)
            {
                currentPasswordIndex = 0;
            }

            var key = AdminKeys[currentPasswordIndex];

            if (!Input.GetKeyDown(key))
            {
                currentPasswordIndex = 0;
            }
            else
            {
                currentPasswordIndex++;

                if (currentPasswordIndex >= AdminKeys.Length)
                {
                    Database.SendMetaData("Admin", "Entered admin mode.");
                    EnterAdminMode();
                }
            }
        }
    }

    private static void TestForCommand()
    {
        if (!isAdmin || !Input.anyKeyDown)
        {
            return;
        }

        foreach (var command in Commands)
        {
            if(Input.GetKeyDown(command.Key))
            {
                Database.SendMetaData("Admin", command.Title);
                command.Action.Invoke();
                return;
            }
        }
    }

    private static void EnterAdminMode()
    {
        if (isAdmin)
        {
            return;
        }

        isAdmin = true;
        currentPasswordIndex = 0;
        LogMessage("You now have admin privileges, press h for command list.");
    }

    private static void ExitAdminMode()
    {
        if (!isAdmin)
        {
            return;
        }

        isAdmin = false;
        currentPasswordIndex = 0;
        LogMessage("Exiting admin mode.");
    }

    private static void ShowHelp()
    {
        var message = "Overview over all available admin commands:";

        foreach (var command in Commands)
        {
            message += $"\n{command.Key.ToString()} :\t{command.Title}";
        }

        LogMessage(message);
    }

    private static void SetWalkSpeed(float speed)
    {
        var controller = Object.FindObjectOfType<FirstPersonController>();

        if (controller != null)
        {
            controller.WalkSpeed = speed;
            LogMessage($"Setting walk speed to {speed:f1}");
        }
        else
        {
            LogMessage("No character controller available.");
        }
    }

    private static void IncreaseWalkSpeed()
    {
        var controller = Object.FindObjectOfType<FirstPersonController>();

        if (controller != null)
        {
            controller.WalkSpeed = Mathf.Min(controller.WalkSpeed + 2f, 15);
            LogMessage($"Increasing walk speed to {controller.WalkSpeed:f1}");
        }
        else
        {
            LogMessage("No character controller available.");
        }
    }

    private static void DecreaseWalkSpeed()
    {
        var controller = Object.FindObjectOfType<FirstPersonController>();

        if (controller != null)
        {
            controller.WalkSpeed = Mathf.Max(controller.WalkSpeed - 2f, DefaultWalkSpeed);
            LogMessage($"Decreasing walk speed to {controller.WalkSpeed:f1}");
        }
        else
        {
            LogMessage("No character controller available.");
        }
    }

    private static void LoadScene(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return;
        }

        SceneManager.LoadScene(name);
        LogMessage($"Loading scene: {name}");
    }

    private static void LogMessage(string message)
    {
        if (Messages.Count >= 24)
        {
            Messages.RemoveAt(0);
        }

        Messages.Add(new DebugMessage($"{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()} : {message}"));
    }

    private void OnGUI()
    {
        if (Messages.Count == 0)
        {
            return;
        }

        if (consoleStyle == null)
        {
            consoleStyle = new GUIStyle();
            consoleStyle.normal.textColor = Color.red;
            consoleStyle.normal.background = GenerateStyleBackground();
            consoleStyle.padding = new RectOffset(4, 2, 4, 2);
            consoleStyle.border = new RectOffset(BorderSize + 1, BorderSize + 1, BorderSize + 1, BorderSize + 1);
        }

        if (consoleStyle.normal.background == null)
        {
            consoleStyle.normal.background = GenerateStyleBackground();
        }

        var pos = new Vector2(20, 20);
        GUI.color = Color.white;

        for (var i = 0; i < Messages.Count; i++)
        {
            if (Time.time - Messages[i].CreationTime > MessageStayDuration)
            {
                Messages.RemoveAt(i);
                i--;
                continue;
            }

            var message = Messages[i].Message;
            var guiContent = new GUIContent(message);
            var size = consoleStyle.CalcSize(guiContent);

            GUI.Label(new Rect(pos, size), guiContent, consoleStyle);
            pos.y += size.y;
        }
    }

    private Texture2D GenerateStyleBackground()
    {
        const int w = 2 * BorderSize + 4;
        const int h = w;
        const int r = BorderSize;

        var vis = new Color(1, 1, 1, 0.7f);
        var invis = new Color(1, 1, 1, 0);

        var texture = new Texture2D(w, h);

        for (var x = 0; x < w; x++)
        {
            for (var y = 0; y < h; y++)
            {
                var xAbs = x <= r ? r - x : x - w + r;
                var yAbs = y <= r ? r - y : y - h + r;

                if (xAbs >= 0 && yAbs >= 0)
                {
                    if (xAbs * xAbs + yAbs * yAbs >= r * r)
                    {
                        texture.SetPixel(x, y, invis);
                        continue;
                    }
                }

                texture.SetPixel(x, y, vis);
            }
        }
        
        texture.Apply(true);

        return texture;
    }

    private struct Command
    {
        public readonly KeyCode Key;

        public readonly Action Action;

        public readonly string Title;

        public Command(KeyCode key, Action action, string title)
        {
            this.Key = key;
            this.Action = action;
            this.Title = title;
        }
    }

    private struct DebugMessage
    {
        public readonly string Message;

        public readonly float CreationTime;

        public DebugMessage(string message)
        {
            Message = message;
            CreationTime = Time.time;
        }
    }
}
