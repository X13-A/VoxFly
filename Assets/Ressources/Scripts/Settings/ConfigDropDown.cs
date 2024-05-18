using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ConfigDropDown : MonoBehaviour
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
        List<string> configNames = new List<string>(); // Initialize the list 
        List<Config> configs = ConfigManager.Instance.Configs;
        string defaultConfigName = ConfigManager.Instance.CurrentConfig?.name;

        for (int i = 0; i < configs.Count; i++)
        {
            configNames.Add(configs[i].name);
            if (configs[i].name == defaultConfigName)
            {
                dropdown.value = i;
            }
        }

        // Clear any existing options
        dropdown.ClearOptions();

        // Add new options from the config list
        dropdown.AddOptions(configNames);
    }

    private void OnDropdownValueChanged(int index)
    {
        Config selectedConfig = ConfigManager.Instance.Configs[index];
        ConfigManager.Instance.SetConfig(index);
        Debug.Log("Select config " + selectedConfig.name);
    }
}