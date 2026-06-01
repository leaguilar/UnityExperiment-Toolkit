using UnityEngine;
using TMPro;

public class PatientInfoUI : MonoBehaviour
{
    [Header("UI Reference")]
    public TextMeshProUGUI infoText; // 拖入你刚才创建的 TMP Text

    [Header("Patient Data (Fill these in)")]
    public string patientName = "John Doe";
    public int age = 45;
    public string gender = "Male";
    public string symptom = "Headache & Fever";
    public string assignedBed = "Bed 05";

    // 每次面板激活时，自动排版文字
    void OnEnable()
    {
        if (infoText != null)
        {
            infoText.text =
                $"<b><size=120%>Name:</size></b> {patientName}\n" +
                $"<b>Age:</b> {age}  |  <b>Gender:</b> {gender}\n" +
                $"<b>Symptom:</b> {symptom}\n" +
                $"<b>Assigned Bed:</b> <color=yellow>{assignedBed}</color>";
        }
    }
}