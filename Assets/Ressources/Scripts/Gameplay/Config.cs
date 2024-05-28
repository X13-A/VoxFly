using UnityEngine;
using UnityEngine.UI;
using System;
[Serializable]
public class Config
{
    public string name;

    public float pitchMult = 1f;
    public float yawMult = 1f;
    public float rollMult = 5f;
    public float forceMult = 1000f;

    public float liftPower;

    public float minThrust;
    public float maxThrust;
    public float throttleAdjustmentRate = 0.1f;

    // Constructeur de la classe Config
    public Config(string name, float pitchMult, float yawMult, float rollMult, float forceMult, float liftPower, float minThrust, float maxThrust, float throttleAdjustmentRate)
    {
        this.name = name;
        this.pitchMult = pitchMult;
        this.yawMult = yawMult;
        this.rollMult = rollMult;
        this.forceMult = forceMult;
        this.liftPower = liftPower;
        this.minThrust = minThrust; 
        this.maxThrust = maxThrust;
        this.throttleAdjustmentRate = throttleAdjustmentRate;
    }
}
