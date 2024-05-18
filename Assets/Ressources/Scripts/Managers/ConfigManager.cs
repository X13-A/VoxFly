using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SDD.Events;

public class ConfigManager : MonoBehaviour
{
    [SerializeField] List<Config> m_Configs;
    public List<Config> Configs { set {  m_Configs = value; } get { return m_Configs; } }
    public static ConfigManager m_Instance;
    public static ConfigManager Instance { get { return m_Instance; } }

    private Config m_CurrentConfig;
    public Config CurrentConfig { set { m_CurrentConfig = value; } get { return m_CurrentConfig; } }

    void Awake()
    {
        if (m_Instance == null)
        {
            m_Instance = this;
        }
        if (m_Configs.Count > 0)
        {
            m_CurrentConfig = m_Configs[0];
        }              
    }

    public void SetConfig(int i)
    {
        m_CurrentConfig = m_Configs[i];

    }

    void OnEnable()
    {

    }

    void OnDisable()
    {

    }
}
