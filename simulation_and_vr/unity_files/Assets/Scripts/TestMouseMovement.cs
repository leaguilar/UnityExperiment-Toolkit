using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

namespace Assets.Scripts
{
    public class TestMouseMovement : ControlTest
    {
        public Camera Camera;

        public float RequiredRotationAngle = 45f;

        public float leftRotation, rightRotation;

        public float previousDirection;

        protected override bool TestRequirements()
        {
            var change = GetChange();

            if (change > 0)
            {
                leftRotation += change;
            }
            else
            {
                rightRotation -= change;
            }

            return leftRotation > RequiredRotationAngle && rightRotation > RequiredRotationAngle;
        }

        protected new void OnEnable()
        {
            base.OnEnable();
            previousDirection = Camera.transform.eulerAngles.y;
        }

        /// <inheritdoc />
        protected override void OnTestFinished()
        {
            Database.SendMetaData("Practice", "Finished mouse practice.");
        }

        private float GetChange()
        {
            var current = Camera.transform.eulerAngles.y;

            if (previousDirection > 270 && current < 90)
            {
                previousDirection -= 360;
            }
            else if (previousDirection < 90 && current > 270)
            {
                previousDirection += 360;
            }

            var angle = current - previousDirection;
            previousDirection = current;

            return angle;
        }
    }
}
