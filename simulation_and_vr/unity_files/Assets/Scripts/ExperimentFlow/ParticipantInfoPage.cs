using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
    public class ParticipantInfoPage : SetupPage
    {
        public TMP_InputField IdInput;

        public Image IdImage;

        public TMP_InputField AgeInput;

        public Image AgeImage;

        public Dropdown GenderDropdown;

        [Header("Valid input colors")]
        [Space]
        public ColorBlock ValidColor = new ColorBlock
        {
            normalColor = new Color(0f, 0.992f, 0.765f, 1f),
            highlightedColor = new Color(0f, 0.878f, 0.675f, 1f),
            pressedColor = new Color(0f, 0.698f, 0.537f, 1f),
            disabledColor = new Color(0.784f, 0.784f, 0.784f, 1f),
            colorMultiplier = 1f,
            fadeDuration = 0.1f
        };

        [Header("Invalid input colors")]
        [Space]
        public ColorBlock InvalidColor = new ColorBlock
        {
            normalColor = new Color(0.992f, 0.431f, 0f, 1f),
            highlightedColor =  new Color(0.878f, 0.384f, 0f, 1f),
            pressedColor = new Color(0.698f, 0.306f, 0f, 1f),
            disabledColor = new Color(0.784f, 0.784f, 0.784f, 1f),
            colorMultiplier = 1f,
            fadeDuration = 0.1f
        };

        protected override void OnApplyPage()
        {
            Database.ParticipantId = IdInput.text.Trim();
            Database.ParticipantAge = int.Parse(AgeInput.text);

            switch (GenderDropdown.value)
            {
                case 1:
                    Database.ParticipantGender = Gender.Male;
                    break;
                case 2 :
                    Database.ParticipantGender = Gender.Female;
                    break;
                case 3: 
                    Database.ParticipantGender = Gender.Other;
                    break;
                default:
                    throw new InvalidOperationException("The gender selection is invalid.");
            }

            Database.SendParticipantInfo();
        }

        protected override bool CanApplyPage()
        {
            return ValidateDataAssembly() && ValidateId() && ValidateAge() && ValidateGender() && WebGLTools.ValidateDeployment();
        }

        protected new void OnEnable()
        {
            base.OnEnable();

            // These two values are statically set for each HIT
            // *At the moment* we can generate valid challenge - redeem code pairs based on this 
            // (Redeem codes are provided at the same time as the hit creation (challenge creation))
            var group = WebGLTools.GetParameter("group", StringComparison.InvariantCultureIgnoreCase);
            var ExpID = WebGLTools.GetParameter("ExpID", StringComparison.InvariantCultureIgnoreCase);

            // These two values we won't be able to get them (attach them to the URL) until the job is accepted
            // Given that depending on who runs the experiments these values could be present or not, we will either
            // generate a random one (assignmentId/sessionId) or ask for the value (workerId/participantId)
            // These values cannot be used for redeem code generation *at the moment* and are used only to identify data in post processing
            var sessionId = WebGLTools.GetParameter("assignmentId", StringComparison.InvariantCultureIgnoreCase);
            var workerId = WebGLTools.GetParameter("workerId", StringComparison.InvariantCultureIgnoreCase);

            StartCoroutine(WebGLTools.FetchConfigJsonData());

            if (group != null && int.TryParse(group, out var groupId))
            {
                Database.ParticipantGroup = groupId;
            }
            else
            {
                Debug.LogWarning("No group specified, using default of 0");
                Database.ParticipantGroup = 0;
            }

            if (!string.IsNullOrWhiteSpace(ExpID))
            {
                Database.ExperimentId = ExpID;
            }
            else
            {
                Debug.LogWarning("No experiment id specified, using default of Empty");
                Database.ExperimentId = string.Empty;
            }

            if (!string.IsNullOrWhiteSpace(sessionId))
            {
                Database.SessionId = sessionId;
            }
            else
            {
                Debug.LogWarning("No sessionId specified, using default of Random");
                Database.SessionId = Guid.NewGuid().ToString().Replace("-",""); // Why do they use -
            }

            // TODO If worker ID provided hide the input field
            if (!string.IsNullOrWhiteSpace(workerId))
            {
                IdInput.text = workerId;
            }
            else
            {
                Debug.LogWarning("No user id specified, using default of Empty");
                IdInput.text = string.Empty;
            }


            ValidateId();
            ValidateAge();
            ValidateGender();

            IdInput.onValueChanged.AddListener(OnIdChanged);
            AgeInput.onValueChanged.AddListener(OnAgeChanged);
            GenderDropdown.onValueChanged.AddListener(OnGenderChanged);
        }

        private void OnGenderChanged(int arg0)
        {
            ValidateGender();
        }

        private void OnIdChanged(string newText)
        {
            ValidateId();
        }

        private void OnAgeChanged(string newText)
        {
            ValidateAge();
        }

        private bool ValidateId()
        {
            var isValid = !string.IsNullOrWhiteSpace(IdInput.text);
            IdImage.color = isValid ? ValidColor.normalColor : InvalidColor.normalColor;

            return isValid;
        }

        private bool ValidateAge()
        {
            var isValid = false;

            if(int.TryParse(AgeInput.text, out var age))
            {
                if (age >= 18 && age <= 122)
                {
                    isValid = true;

                }
            }

            AgeImage.color = isValid ? ValidColor.normalColor : InvalidColor.normalColor;

            return isValid;
        }

        private bool ValidateGender()
        {
            var isValid = GenderDropdown.value != 0;
            GenderDropdown.colors = isValid ? ValidColor : InvalidColor;

            return isValid;
        }

        private bool ValidateDataAssembly()
        {
            var isValid = !string.IsNullOrWhiteSpace(WebGLTools.myconfig.dataAssemblyUrl);
            if (isValid)
            {
                Database.DataCollectionServerURL=WebGLTools.myconfig.dataAssemblyUrl;
                Debug.Log(Database.DataCollectionServerURL);
            }
            return isValid;
        }

    }
}
