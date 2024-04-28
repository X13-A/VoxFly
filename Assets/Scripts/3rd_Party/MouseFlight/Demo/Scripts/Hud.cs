//
// Copyright (c) Brian Hernandez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace MFlight.Demo
{
    public class Hud : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private MouseFlightController mouseFlight = null;

        [Header("HUD Elements")]
        [SerializeField] private RectTransform boresight = null;
        [SerializeField] private RectTransform mousePos = null;
        [SerializeField]
        TMP_Text airspeed;
        [SerializeField]
        TMP_Text altitude;
        [SerializeField]
        TMP_Text turbulenceIntensity;
        [SerializeField]
        TMP_Text aoaIndicator;
        [SerializeField]
        Slider dangerBar;

        private Camera playerCam = null;

        const float metersToKnots = 1.94384f;
        const float metersToFeet = 3.28084f;

        

        [SerializeField]
        Plane plane;

        private void Awake()
        {
            if (mouseFlight == null)
                Debug.LogError(name + ": Hud - Mouse Flight Controller not assigned!");

            playerCam = Camera.main;

          
        }

        private void Update()
        {
            if (mouseFlight == null || playerCam == null)
                return;

            UpdateGraphics(mouseFlight);
            UpdateAirspeed();
            UpdateAltitude();
            UpdateAOA();
            UpdateTurbulenceIntensity();
        }

        void UpdateAOA()
        {
            aoaIndicator.text = string.Format("{0:0.0} AOA", plane.AngleOfAttack * Mathf.Rad2Deg);
        }

        void UpdateAirspeed()
        {
            var speed = plane.LocalVelocity.z * metersToKnots;
            airspeed.text = string.Format("{0:0}", speed);
        }

        void UpdateTurbulenceIntensity()
        {
            var intensity = plane.enableTurbulence ? plane.turbulenceIntensity : 0f;
            turbulenceIntensity.text = string.Format("{0:0}", intensity);
        }

        void UpdateAltitude()
        {
            var altitude = plane.Rigid.position.y * metersToFeet;
            this.altitude.text = string.Format("{0:0}", altitude);
        }

        private void UpdateGraphics(MouseFlightController controller)
        {
            if (boresight != null)
            {
                boresight.position = playerCam.WorldToScreenPoint(controller.BoresightPos);
                boresight.gameObject.SetActive(boresight.position.z > 1f);
            }

            if (mousePos != null)
            {
                mousePos.position = playerCam.WorldToScreenPoint(controller.MouseAimPos);
                mousePos.gameObject.SetActive(mousePos.position.z > 1f);
            }
        }

        public void SetReferenceMouseFlight(MouseFlightController controller)
        {
            mouseFlight = controller;
        }
    }
}
