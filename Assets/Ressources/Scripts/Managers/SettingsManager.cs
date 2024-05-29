using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SDD.Events;
using TMPro;

public class SettingsManager : MonoBehaviour
{
    [Header("Titles")]
    [SerializeField] TMP_Text m_SoundTitle;
    [SerializeField] TMP_Text m_ConfigTitle;
    [SerializeField] TMP_Text m_MapTitle;

    [Header("Panels")]
    [SerializeField] GameObject m_ConfigPanel;
    [SerializeField] GameObject m_SoundPanel;
    [SerializeField] GameObject m_MapPanel;

    List<GameObject> m_Panels;
    List<TMP_Text> m_Titles;

    private enum SETTINGSMODE { sound, config, map }

    private SETTINGSMODE m_SettingsMode;

    void Awake()
    {
        m_Panels = new List<GameObject>(
            new GameObject[] { m_ConfigPanel, m_MapPanel, m_SoundPanel });
        m_Titles = new List<TMP_Text>(
            new TMP_Text[] { m_ConfigTitle, m_MapTitle, m_SoundTitle });
        soundMode();
    }

    void switchTitle(TMP_Text title)
    {
        m_Titles.ForEach(item => item.fontStyle = (item == title) ? FontStyles.Bold : FontStyles.Normal);
    }

    void switchPanel(GameObject panel)
    {
        m_Panels.ForEach(item => item.SetActive(panel == item));
    }

    public void configMode()
    {
        if(m_SettingsMode!=SETTINGSMODE.config)
        {
            m_SettingsMode = SETTINGSMODE.config;
            switchTitle(m_ConfigTitle);
            switchPanel(m_ConfigPanel);
        }
    }

    public void soundMode()
    {
        if (m_SettingsMode != SETTINGSMODE.sound)
        {
            m_SettingsMode = SETTINGSMODE.sound;
            switchTitle(m_SoundTitle);
            switchPanel(m_SoundPanel);
        }
    }

    public void mapMode()
    {
        if (m_SettingsMode != SETTINGSMODE.map)
        {
            m_SettingsMode = SETTINGSMODE.map;
            switchTitle(m_MapTitle);
            switchPanel(m_MapPanel);
        }
    }

}
