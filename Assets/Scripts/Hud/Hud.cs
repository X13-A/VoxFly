//
// Copyright (c) Brian Hernandez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Hud : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private MouseFlightController mouseFlight = null;

    [Header("HUD Elements")]
    //[SerializeField] private RectTransform boresight = null;
    //[SerializeField] private RectTransform mousePos = null;
    [SerializeField]
    TMP_Text airspeed;
    [SerializeField]
    TMP_Text altitude;
    [SerializeField]
    GameObject aiguille;
    [SerializeField]
    float maxSpeed = 800f;
    [SerializeField]
    GameObject planeImage;
    [SerializeField]
    GameObject cloudContainer;
    [SerializeField]
    GameObject cloudImage;
    [SerializeField]
    GameObject terrainImage;
    //TMP_Text turbulenceIntensity;
    [SerializeField]
    TMP_Text aoaIndicator;
    [SerializeField]
    Compass compass;
    //[SerializeField]
    //Slider dangerBar;

    private Camera playerCam = null;

    const float toKilometersPerHour = 60f;
    const float metersToFeet = 3.28084f;



    [SerializeField]
    Plane plane;

    private void Awake()
    {
        if (mouseFlight == null)
            Debug.LogError(name + ": Hud - Mouse Flight Controller not assigned!");

        playerCam = Camera.main;
        compass.SetPlane(plane);
        compass.SetCamera(playerCam);

    }

    private void Update()
    {
        if (mouseFlight == null || playerCam == null)
            return;

        //UpdateGraphics(mouseFlight);
        UpdateAirspeed();
        UpdateAltitude();
        UpdateAOA();
        //UpdateTurbulenceIntensity();
    }

    void UpdateAOA()
    {
        aoaIndicator.text = string.Format("{0:0.0} AOA", plane.AngleOfAttack * Mathf.Rad2Deg);
    }

    void UpdateAirspeed()
    {
        var speed = plane.LocalVelocity.magnitude*toKilometersPerHour;
        // Calculer l'angle de rotation proportionnel à la vitesse
        float angle = (speed / maxSpeed) * 200f;
        
        // Appliquer la rotation autour de l'axe Y (modifiez l'axe si nécessaire)
        aiguille.transform.rotation = Quaternion.Euler(0, 0, -angle);

        airspeed.text = string.Format("{0:0} km/h", speed);
    }

    /*void UpdateTurbulenceIntensity()
    {
        var intensity = plane.enableTurbulence ? plane.turbulenceIntensity : 0f;
        turbulenceIntensity.text = string.Format("{0:0}", intensity);
    }*/

    void UpdateAltitude()
    {
        float cloudAltitude = cloudContainer.transform.position.y * metersToFeet;
        float planeAltitude = plane.Rigid.position.y * metersToFeet;

        if (planeAltitude > cloudAltitude)
        {
            planeAltitude = cloudAltitude;
        }

        float newYPosition = terrainImage.transform.position.y + (planeAltitude / cloudAltitude) * (cloudImage.transform.position.y-terrainImage.transform.position.y);
        Debug.Log("planeAltitude : " + planeAltitude + " - ImagePos : " + newYPosition);
        this.altitude.text = string.Format("{0:0}", planeAltitude);
        Vector3 newPosition = new Vector3(planeImage.transform.position.x, newYPosition, planeImage.transform.position.z);
        planeImage.transform.position = newPosition;
    }

    /*private void UpdateGraphics(MouseFlightController controller)
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
    }*/

    public void SetReferenceMouseFlight(MouseFlightController controller)
    {
        mouseFlight = controller;
    }
}

