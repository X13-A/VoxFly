using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Xml.Linq;

public class WorldConfigDropDown : MonoBehaviour
{
    private TMP_Dropdown dropdown;

    void Awake()
    {
        dropdown = GetComponent<TMP_Dropdown>();
    }

    void Start()
    {
        // Initialize the dropdown with the config list
        InitializeDropdown();

        // Add listener for when the value changes
        dropdown.onValueChanged.AddListener(OnDropdownValueChanged);
    }

    public void InitializeDropdown()
    {
        List<string> configNames = new List<string>();
        List<WorldConfig> configs = WorldConfigManager.Instance.Configs;
        string defaultConfigName = WorldConfigManager.Instance.CurrentConfig?.ConfigName;
        int defaultIndex = 0;

        for (int i = 0; i < configs.Count; i++)
        {
            configNames.Add(configs[i].ConfigName);
            if (configs[i].ConfigName == defaultConfigName)
            {
                defaultIndex = i;
            }
        }

        dropdown.ClearOptions();
        dropdown.AddOptions(configNames);
        dropdown.value = defaultIndex;
    }

    private void OnDropdownValueChanged(int index)
    {
        WorldConfig selectedConfig = WorldConfigManager.Instance.Configs[index];
        WorldConfigManager.Instance.SetConfig(index);
        Debug.Log("Select config " + selectedConfig.ConfigName);
    }
}