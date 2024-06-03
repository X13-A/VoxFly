using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SDD.Events;

public class WorldConfigManager : Singleton<WorldConfigManager>
{
    [SerializeField] List<WorldConfig> m_Configs;
    public List<WorldConfig> Configs { set { m_Configs = value; } get { return m_Configs; } }

    private WorldConfig m_CurrentConfig;
    public WorldConfig CurrentConfig { set { m_CurrentConfig = value; } get { return m_CurrentConfig; } }

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
        EventManager.Instance.Raise(new WorldConfigChangedEvent());
    }
}
