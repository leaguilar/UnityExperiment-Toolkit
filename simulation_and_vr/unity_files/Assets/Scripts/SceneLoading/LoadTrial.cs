using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts
{
    public class LoadTrial : MonoBehaviour
    {
        public List<string> Trials = new List<string>();

        public float Progress;

        public TMP_Text Text;

        private AsyncOperation loadingOperation;

        private Scene origin;

        void Awake()
        {
            if (Trials == null || Trials.Count == 0)
            {
                Debug.LogError("No trials defined, can't load.");
                return;
            }

            origin = SceneManager.GetActiveScene();

            var id = Database.ParticipantGroup - 1;
            if (id < 0)
            {
                loadingOperation = SceneManager.LoadSceneAsync(Trials[0], LoadSceneMode.Additive);
                loadingOperation.allowSceneActivation = true;
            }
            else if (id < Trials.Count)
            {
                loadingOperation = SceneManager.LoadSceneAsync(Trials[id], LoadSceneMode.Additive);
                loadingOperation.allowSceneActivation = true;
            }
            else
            {
                Debug.LogError("Not enough trials were defined, can't load.");
            }
        }

        void Update()
        {
            if (loadingOperation == null)
            {
                return;
            }

            Progress = loadingOperation.progress;

            if (Text != null)
            {
                Text.text = Progress.ToString("##0.0%");
            }

            if (loadingOperation.isDone)
            {
                SceneManager.UnloadSceneAsync(origin);
            }
        }
    }
}
