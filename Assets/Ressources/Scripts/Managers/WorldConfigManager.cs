using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SDD.Events;

public class WorldConfigManager : MonoBehaviour
{
    [SerializeField] List<WorldConfig> m_Configs;
    public List<WorldConfig> Configs { set { m_Configs = value; } get { return m_Configs; } }
    public static WorldConfigManager m_Instance;
    public static WorldConfigManager Instance { get { return m_Instance; } }

    private WorldConfig m_CurrentConfig;
    public WorldConfig CurrentConfig { set { m_CurrentConfig = value; } get { return m_CurrentConfig; } }

    void Awake()
    {
        if (m_Instance == null)
        {
            m_Instance = this;
            if (m_Configs.Count > 0)
            {
                m_CurrentConfig = m_Configs[0];
            }
        }
        else
        {
            Destroy(this);
        }
    }

    public void SetConfig(int i)
    {
        m_CurrentConfig = m_Configs[i];
        EventManager.Instance.Raise(new WorldConfigChangedEvent());
    }
}
