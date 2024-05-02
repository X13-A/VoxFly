//
// Copyright (c) Brian Hernandez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using SDD.Events;
using UnityEngine;

public class plane : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private MouseFlightController controller = null;

    [Header("Physics")]
    [Tooltip("Pitch, Yaw, Roll")] public Vector3 turnTorque = new Vector3(90f, 25f, 45f);

    /*[Header("Autopilot")]
    [Tooltip("Sensitivity for autopilot flight.")] public float sensitivity = 5f;
    [Tooltip("Angle at which airplane banks fully into target.")] public float aggressiveTurnAngle = 10f;*/

    [Header("Input")]
    [SerializeField][Range(-1f, 1f)] private float pitch = 0f;
    [SerializeField][Range(-1f, 1f)] private float yaw = 0f;
    [SerializeField][Range(-1f, 1f)] private float roll = 0f;

    public float Pitch { set { pitch = Mathf.Clamp(value, -1f, 1f); } get { return pitch; } }
    public float Yaw { set { yaw = Mathf.Clamp(value, -1f, 1f); } get { return yaw; } }
    public float Roll { set { roll = Mathf.Clamp(value, -1f, 1f); } get { return roll; } }

    public float pitchMult = 1f;
    public float yawMult = 1f;
    public float rollMult = 5f;
    public float forceMult = 1000f;

    private Rigidbody rigid;
    public Rigidbody Rigid { set { rigid = value; } get { return rigid; } }

    private bool rollOverride = false;
    private bool pitchOverride = false;

    private bool regulatorActivate;
    public bool RegulatorActivate { set { regulatorActivate = value; } get { return regulatorActivate; } }

    [Header("Turbulence")]
    [SerializeField]
    public float turbulenceStrength = 0f;
    [SerializeField]
    private float turbulenceScale = 0.5f;

    [Header("Lift")]
    [SerializeField]
    float liftPower;
    [SerializeField]
    AnimationCurve liftAOACurve;
    [SerializeField]
    float inducedDrag;
    [SerializeField]
    AnimationCurve inducedDragCurve;

    [Header("Drag")]
    [SerializeField]
    AnimationCurve dragForward;
    [SerializeField]
    AnimationCurve dragBack;
    [SerializeField]
    AnimationCurve dragLeft;
    [SerializeField]
    AnimationCurve dragRight;
    [SerializeField]
    AnimationCurve dragTop;
    [SerializeField]
    AnimationCurve dragBottom;
    [SerializeField]
    Vector3 angularDrag;
    [SerializeField]
    float airbrakeDrag;

    [Header("Thrust")]
    [SerializeField]
    public float minThrust;
    [SerializeField]
    public float maxThrust;
    [SerializeField]
    private float throttleAdjustmentRate = 0.1f;
    public float Throttle { get; private set; }

    Vector3 lastVelocity;
    public Vector3 EffectiveInput { get; private set; }
    public Vector3 Velocity { get; private set; }
    public Vector3 LocalVelocity { get; private set; }
    public Vector3 LocalGForce { get; private set; }
    public Vector3 LocalAngularVelocity { get; private set; }
    public float AngleOfAttack { get; private set; }
    public float AngleOfAttackYaw { get; private set; }
    public bool AirbrakeDeployed { get; private set; }

    const float metersToFeet = 3.28084f;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody>();

        if (controller == null)
            Debug.LogError(name + ": Plane - Missing reference to MouseFlightController!");
    }

    private void Start()
    {
        //rigid.velocity = rigid.rotation * new Vector3(0, 0, initialSpeed);
        EventManager.Instance.Raise(new PlaneInformationEvent() { eMinThrust = minThrust, eMaxThrust = maxThrust }); ;
    }

    public static float MoveTo(float value, float target, float speed, float deltaTime, float min = 0, float max = 1)
    {
        float diff = target - value;
        float delta = Mathf.Clamp(diff, -speed * deltaTime, speed * deltaTime);
        return Mathf.Clamp(value + delta, min, max);
    }

    //similar to Vector3.Scale, but has separate factor negative values on each axis
    public static Vector3 Scale6(
        Vector3 value,
        float posX, float negX,
        float posY, float negY,
        float posZ, float negZ
    )
    {
        Vector3 result = value;

        if (result.x > 0)
        {
            result.x *= posX;
        }
        else if (result.x < 0)
        {
            result.x *= negX;
        }

        if (result.y > 0)
        {
            result.y *= posY;
        }
        else if (result.y < 0)
        {
            result.y *= negY;
        }

        if (result.z > 0)
        {
            result.z *= posZ;
        }
        else if (result.z < 0)
        {
            result.z *= negZ;
        }

        return result;
    }

    void SetTurbulence(float strength, float scale)
    {
        turbulenceStrength = strength;
        turbulenceScale = scale;
    }

    void UpdateTurbulence()
    {
        float planeAltitude = rigid.position.y * metersToFeet;
        /*
        if(planeAltitude < ?) SetTurbulence(0,0);
        else if(planeAltitude < ?) SetTurbulence(?,?);
        ...
        */
    }

    void UpdateDrag()
    {
        var lv = LocalVelocity;
        var lv2 = lv.sqrMagnitude;  //velocity squared


        //calculate coefficient of drag depending on direction on velocity
        var coefficient = Scale6(
            lv.normalized,
            dragRight.Evaluate(Mathf.Abs(lv.x)), dragLeft.Evaluate(Mathf.Abs(lv.x)),
            dragTop.Evaluate(Mathf.Abs(lv.y)), dragBottom.Evaluate(Mathf.Abs(lv.y)),
            dragForward.Evaluate(Mathf.Abs(lv.z)) + airbrakeDrag,   //include extra drag for forward coefficient
            dragBack.Evaluate(Mathf.Abs(lv.z))
        );

        var drag = coefficient.magnitude * lv2 * -lv.normalized;    //drag is opposite direction of velocity
        //Debug.Log("drag : " + drag);
        rigid.AddRelativeForce(drag);
    }




    Vector3 CalculateLift(float angleOfAttack, Vector3 rightAxis, float liftPower, AnimationCurve aoaCurve, AnimationCurve inducedDragCurve)
    {
        var liftVelocity = Vector3.ProjectOnPlane(LocalVelocity, rightAxis);    //project velocity onto YZ plane
        var v2 = liftVelocity.sqrMagnitude;                                     //square of velocity

        //lift = velocity^2 * coefficient * liftPower
        //coefficient varies with AOA
        //Debug.Log("ANGLEOFATTACK : " + angleOfAttack * Mathf.Rad2Deg);
        var liftCoefficient = aoaCurve.Evaluate(angleOfAttack * Mathf.Rad2Deg);
        //Debug.Log("Velocity : "+liftVelocity+ " - Local : " + LocalVelocity + " - Angle : " + angleOfAttack * Mathf.Rad2Deg + " - Coeff : "+liftCoefficient);
        var liftForce = v2 * liftCoefficient * liftPower;


        //lift is perpendicular to velocity
        var liftDirection = Vector3.Cross(liftVelocity.normalized, rightAxis);
        //Debug.Log("lift velocity : "+LocalVelocity.normalized+" - liftDirection : "+liftDirection);
        var lift = liftDirection * liftForce;

        //induced drag varies with square of lift coefficient
        var dragForce = liftCoefficient * liftCoefficient;
        var dragDirection = -liftVelocity.normalized;
        var inducedDrag = dragDirection * v2 * dragForce * this.inducedDrag * inducedDragCurve.Evaluate(Mathf.Max(0, LocalVelocity.z));
        //Debug.Log(liftForce + " - " + v2 + " - " + liftCoefficient);
        return lift + inducedDrag;
    }

    // Méthode pour mettre à jour la poussée basée sur l'état actuel du throttle
    void UpdateThrust()
    {
        // Calculer la poussée actuelle en fonction du throttle
        // Throttle varie entre 0 et 1, donc on mappe cette valeur à l'intervalle [minThrust, maxThrust]
        float currentThrust = Mathf.Lerp(minThrust, maxThrust, Throttle);
        EventManager.Instance.Raise(new PlaneStateEvent() { eThrust = currentThrust });
        // Appliquer la poussée
        rigid.AddRelativeForce(Vector3.forward * currentThrust, ForceMode.Force);
        //Debug.Log("Current Thrust: " + currentThrust + " with Throttle at: " + Throttle);
    }

    public void AdjustThrottle(float adjustment)
    {
        // Modifier le throttle par l'ajustement, en s'assurant qu'il reste entre 0 et 1
        Throttle = Mathf.Clamp(Throttle + adjustment, 0, 1);
    }

    void UpdateLift()
    {
        if (LocalVelocity.sqrMagnitude < 1f) return;

        //float flapsLiftPower = FlapsDeployed ? this.flapsLiftPower : 0;
        //float flapsAOABias = FlapsDeployed ? this.flapsAOABias : 0;
        var liftForce = CalculateLift(
            AngleOfAttack, Vector3.right,
            liftPower,
            liftAOACurve,
            inducedDragCurve
        );

        //var yawForce = CalculateLift(AngleOfAttackYaw, Vector3.up, rudderPower, rudderAOACurve, rudderInducedDragCurve);

        rigid.AddRelativeForce(liftForce);
        //Debug.Log("lift force : " + liftForce);
        //rigid.AddRelativeForce(yawForce);
    }

    void CalculateAngleOfAttack()
    {
        if (LocalVelocity.sqrMagnitude < 0.1f)
        {
            AngleOfAttack = 0;
            AngleOfAttackYaw = 0;
            return;
        }

        AngleOfAttack = Mathf.Atan2(-LocalVelocity.y, LocalVelocity.z);
        //Debug.Log("LOCAL : " + LocalVelocity);
        //Debug.Log(Mathf.Atan2(-LocalVelocity.y, LocalVelocity.z) * Mathf.Rad2Deg);
        AngleOfAttackYaw = Mathf.Atan2(LocalVelocity.x, LocalVelocity.z);
    }

    void CalculateGForce(float dt)
    {
        var invRotation = Quaternion.Inverse(rigid.rotation);
        var acceleration = (Velocity - lastVelocity) / dt;
        LocalGForce = invRotation * acceleration;
        lastVelocity = Velocity;
    }

    void CalculateState(float dt)
    {
        var invRotation = Quaternion.Inverse(rigid.rotation);
        Velocity = rigid.velocity;
        LocalVelocity = invRotation * Velocity;  //transform world velocity into local space
        LocalAngularVelocity = invRotation * rigid.angularVelocity;  //transform into local space

        CalculateAngleOfAttack();
    }

    private void Update()
    {
        // When the player commands their own stick input, it should override what the
        // autopilot is trying to do.
        rollOverride = false;
        pitchOverride = false;

        float keyboardRoll = Input.GetAxis("Horizontal");
        if (Mathf.Abs(keyboardRoll) > .25f)
        {
            rollOverride = true;
        }

        float keyboardPitch = Input.GetAxis("Vertical");
        if (Mathf.Abs(keyboardPitch) > .25f)
        {
            pitchOverride = true;
            rollOverride = true;
        }
        if (Input.GetKey(KeyCode.R))
        {
            if (!regulatorActivate) regulatorActivate = true;
            Debug.Log("REGULATOR");
        }

        if (Input.GetKey(KeyCode.Space))
        {
            // Augmenter le throttle
            if(regulatorActivate)regulatorActivate = false;
            AdjustThrottle(throttleAdjustmentRate * Time.deltaTime);
        }
        else if(!regulatorActivate)
        {
            // Diminuer le throttle
            AdjustThrottle(-throttleAdjustmentRate * Time.deltaTime);
        }


        // Calculate the autopilot stick inputs.
        float autoYaw = 0f;
        float autoPitch = 0f;
        float autoRoll = 0f;
        //if (controller != null)
        //    RunAutopilot(controller.MouseAimPos, out autoYaw, out autoPitch, out autoRoll);

        // Use either keyboard or autopilot input.
        yaw = autoYaw;
        pitch = (pitchOverride) ? keyboardPitch : autoPitch;
        roll = (rollOverride) ? keyboardRoll : autoRoll;
    }

    /*private void RunAutopilot(Vector3 flyTarget, out float yaw, out float pitch, out float roll)
    {
        // This is my usual trick of converting the fly to position to local space.
        // You can derive a lot of information from where the target is relative to self.
        var localFlyTarget = transform.InverseTransformPoint(flyTarget).normalized * sensitivity;
        var angleOffTarget = Vector3.Angle(transform.forward, flyTarget - transform.position);

        // IMPORTANT!
        // These inputs are created proportionally. This means it can be prone to
        // overshooting. The physics in this example are tweaked so that it's not a big
        // issue, but in something with different or more realistic physics this might
        // not be the case. Use of a PID controller for each axis is highly recommended.

        // ====================
        // PITCH AND YAW
        // ====================

        // Yaw/Pitch into the target so as to put it directly in front of the aircraft.
        // A target is directly in front the aircraft if the relative X and Y are both
        // zero. Note this does not handle for the case where the target is directly behind.
        yaw = Mathf.Clamp(localFlyTarget.x, -1f, 1f);
        pitch = -Mathf.Clamp(localFlyTarget.y, -1f, 1f);

        // ====================
        // ROLL
        // ====================

        // Roll is a little special because there are two different roll commands depending
        // on the situation. When the target is off axis, then the plane should roll into it.
        // When the target is directly in front, the plane should fly wings level.

        // An "aggressive roll" is input such that the aircraft rolls into the target so
        // that pitching up (handled above) will put the nose onto the target. This is
        // done by rolling such that the X component of the target's position is zeroed.
        var agressiveRoll = Mathf.Clamp(localFlyTarget.x, -1f, 1f);

        // A "wings level roll" is a roll commands the aircraft to fly wings level.
        // This can be done by zeroing out the Y component of the aircraft's right.
        var wingsLevelRoll = transform.right.y;

        // Blend between auto level and banking into the target.
        var wingsLevelInfluence = Mathf.InverseLerp(0f, aggressiveTurnAngle, angleOffTarget);
        roll = Mathf.Lerp(wingsLevelRoll, agressiveRoll, wingsLevelInfluence);
    }*/

    private void FixedUpdate()
    {
        // Ultra simple flight where the plane just gets pushed forward and manipulated
        // with torques to turn.
        //rigid.AddRelativeForce(Vector3.forward * thrust * forceMult, ForceMode.Force);
        float dt = Time.fixedDeltaTime;
        float time = Time.fixedTime;

        //calculate at start, to capture any changes that happened externally
        CalculateState(dt);
        CalculateGForce(dt);

        //apply updates
        UpdateThrust();
        UpdateDrag();
        UpdateLift();
        UpdateTurbulence();

        // Calculate and apply turbulence effects
        Vector3 currentTurbulence = Vector3.zero;

        currentTurbulence.x = (Mathf.PerlinNoise(time * turbulenceScale, 0.0f) * 2 - 1) * turbulenceStrength;
        currentTurbulence.y = (Mathf.PerlinNoise(0.0f, time * turbulenceScale) * 2 - 1) * turbulenceStrength;
        currentTurbulence.z = (Mathf.PerlinNoise(time * turbulenceScale, time * turbulenceScale) * 2 - 1) * turbulenceStrength;

        // Apply torques with turbulence effects
        rigid.AddRelativeTorque(new Vector3(turnTorque.x * pitch * pitchMult,
                                            turnTorque.y * yaw * yawMult,
                                            -turnTorque.z * roll * rollMult) * forceMult + currentTurbulence,
                                ForceMode.Force);
    }
}

