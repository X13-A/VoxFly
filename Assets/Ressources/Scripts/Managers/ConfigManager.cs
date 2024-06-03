using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SDD.Events;

public class ConfigManager : Singleton<ConfigManager>
{
    [SerializeField] List<Config> m_Configs;
    public List<Config> Configs { set {  m_Configs = value; } get { return m_Configs; } }

    private Config m_CurrentConfig;
    public Config CurrentConfig { set { m_CurrentConfig = value; } get { return m_CurrentConfig; } }

    protected override void Awake()
    {
        base.Awake();
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
