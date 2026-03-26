using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Video;

namespace Assets.Scripts
{
    public class HideWhileVideoLoading : MonoBehaviour
    {
        [SerializeField]
        private VideoPlayer videoPlayer;

        [SerializeField]
        private MonoBehaviour behavior;

        void Update()
        {
            if (videoPlayer == null || behavior == null)
            {
                return;
            }

            if (videoPlayer.isPrepared)
            {
                behavior.enabled = true;
            }
            else
            {
                behavior.enabled = false;
            }
        }
    }
}
