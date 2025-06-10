using System.Collections.Generic;
using Autofill;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

namespace MAVLinkAPI.UI
{
    public class InputHistoryDropDown : MonoBehaviour
    {
        [Autofill] public InputField Input;

        [Autofill] public Dropdown Dropdown;

        [DoNotSerialize] public List<string> history = new();

        private const string HistoryPlayerPrefsKey = "InputHistoryDropDown_History";

        // Wrapper class for JsonUtility to serialize/deserialize List<string>
        [System.Serializable]
        private class HistoryWrapper
        {
            public List<string> items;
            public HistoryWrapper(List<string> items) => this.items = items;
        }

        void Awake()
        {
            LoadHistory();
        }

        void Start()
        {
            if (Input != null)
            {
                Input.onEndEdit.AddListener(OnInputSubmit);
            }

            if (Dropdown != null)
            {
                Dropdown.onValueChanged.AddListener(OnDropdownSelect);
                RefreshHistory(); // Initial population after loading history
            }
        }

        void OnInputSubmit(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                if (!history.Contains(text))
                {
                    history.Insert(0, text); // Add to the beginning for most recent
                    // Optional: Limit history size
                    // if (history.Count > MaxHistorySize) history.RemoveAt(history.Count - 1);
                    SaveHistory();
                    RefreshHistory();
                }
                // Optionally clear the input field after submission
                // Input.text = ""; 
            }
        }

        void OnDropdownSelect(int index)
        {
            if (Dropdown != null && history.Count > index && index >= 0)
            {
                Input.text = history[index];
            }
        }

        public void RefreshHistory()
        {
            if (Dropdown == null) return;

            Dropdown.ClearOptions();
            if (history.Count > 0)
            {
                Dropdown.AddOptions(history);
            }
            Dropdown.RefreshShownValue();
        }

        private void LoadHistory()
        {
            string json = PlayerPrefs.GetString(HistoryPlayerPrefsKey, null);
            if (!string.IsNullOrEmpty(json))
            {
                HistoryWrapper wrapper = JsonUtility.FromJson<HistoryWrapper>(json);
                if (wrapper != null && wrapper.items != null)
                {
                    history = wrapper.items;
                }
                else
                {
                    history = new List<string>(); // Fallback to empty list if deserialization fails
                }
            }
            else
            {
                history = new List<string>(); // Initialize if no saved history
            }
        }

        private void SaveHistory()
        {
            HistoryWrapper wrapper = new HistoryWrapper(history);
            string json = JsonUtility.ToJson(wrapper);
            PlayerPrefs.SetString(HistoryPlayerPrefsKey, json);
            PlayerPrefs.Save(); // Ensure data is written to disk
        }

        void OnApplicationQuit()
        {
            SaveHistory();
        }
    }
}