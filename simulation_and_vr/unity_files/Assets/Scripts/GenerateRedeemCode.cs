using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(InputField))]
public class GenerateRedeemCode : MonoBehaviour
{
    public InputField VerificationOutput;

    void Start()
    {
        if (VerificationOutput == null)
        {
            VerificationOutput = this.gameObject.GetComponent<InputField>();
        }

        var verificationCode = GetVerificationText();

        VerificationOutput.text = verificationCode;
        
        VerificationOutput.selectionAnchorPosition = 0;
        VerificationOutput.caretPosition = verificationCode.Length;
        VerificationOutput.selectionFocusPosition = verificationCode.Length;

        VerificationOutput.Select();
        VerificationOutput.ActivateInputField();

        GUIUtility.systemCopyBuffer = verificationCode;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private string GetVerificationText()
    {
        var expId = Database.ExperimentId ?? string.Empty;
        var parGroup = Database.ParticipantGroup.ToString();
        var salt = "Science is cool!!";

        if (string.IsNullOrWhiteSpace(expId))
        {
            //return "[Invalid Data]";
        }

        var text = expId + parGroup + salt;

        Debug.Log(text);

        var bytes = Encoding.UTF8.GetBytes(text);

        Debug.Log(bytes);
        var hashBytes = MD5.Create().ComputeHash(bytes);
        Debug.Log(hashBytes);

        var builder = new StringBuilder();

        builder.Append($"{hashBytes[0]:x2}");
        builder.Append($"{hashBytes[1]:x2}");
        builder.Append($"{hashBytes[2]:x2}");
        builder.Append($"{hashBytes[3]:x2}");
        return builder.ToString();
    }
}
