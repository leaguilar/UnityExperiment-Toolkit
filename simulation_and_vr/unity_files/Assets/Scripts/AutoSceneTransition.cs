using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class AutoSceneTransition : MonoBehaviour
{
    [Header("Settings")]
    public string nextSceneName = "LobbyZhuo";
    public float delaySeconds = 5.0f;
    
    [Header("UI Reference")]
    public TMP_Text countdownText;
    public string messagePrefix = "Practice Complete!\nMoving to Lobby in ";

    private void OnEnable()
    {
        StartCoroutine(CountdownRoutine());
    }

    private IEnumerator CountdownRoutine()
    {
        float remainingTime = delaySeconds;

        while (remainingTime > 0)
        {
            if (countdownText != null)
            {
                countdownText.text = messagePrefix + Mathf.CeilToInt(remainingTime) + "...";
            }
            
            yield return new WaitForSeconds(1.0f);
            remainingTime -= 1.0f;
        }

        if (countdownText != null) countdownText.text = "Loading...";
        
        Debug.Log("Auto-transitioning to: " + nextSceneName);
        SceneManager.LoadScene(nextSceneName);
    }
}
