using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldConfig : MonoBehaviour
{
    [Header("General parameters")]
    [SerializeField] private string configName = "New config";
    public string ConfigName => configName;

    [SerializeField] public float grassDepth = 3;

    [SerializeField] public int width = 50;
    [SerializeField] public int depth = 50;
    [SerializeField] public int height = 50;

    [Header("Optimization parameters")]
    [SerializeField] public int brickSize = 8;

    [Header("Terrain parameters")]
    [SerializeField] public uint terrainSeed;
    [SerializeField] public float terrainAmplitude = 1;
    [SerializeField] public float terrainStartY = 15;
    [SerializeField] public Vector2 terrainScale = new Vector2(3.33f, 3.33f);
    [SerializeField] public Vector2 terrainOffset = new Vector2();

    [Header("Caves parameters")]
    [SerializeField] public uint cavesSeed;
    [SerializeField] public Vector3 offset = new Vector3();
    [SerializeField] public Vector3 scale = new Vector3(3.33f, 3.33f, 3.33f);
    [SerializeField] public float threshold = 0.5f; // Seuil pour la g�n�ration des grottes
    /// <summary>
    /// The higher the value, the thinner the caves are on higher terrain
    /// </summary>
    [SerializeField] public float cavesHeightDiminution = 0.1f;

    [Header("Deep terrain parameters")]
    [SerializeField] public uint deepTerrainSeed;
    [SerializeField] public float deepTerrainAmplitude = 1;
    [SerializeField] public Vector2 deepTerrainScale = new Vector2(3.33f, 3.33f);
    [SerializeField] public Vector2 deepTerrainOffset = new Vector2();

    [Header("Land coverage parameters")]
    [SerializeField] public uint coverageSeed;
    [SerializeField] public Vector2 coverageScale = new Vector2(3.33f, 3.33f);
    [SerializeField] public Vector2 coverageOffset = new Vector2();
    [SerializeField] public float coverage = 0.5f;
    [SerializeField] public float coverageFactor = 1.0f;
}
