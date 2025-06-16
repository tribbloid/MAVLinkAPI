#nullable enable
using System;
using System.Collections.Generic;
using Autofill;
using Unity.VisualScripting;
using UnityEngine;
using TMPro;

namespace MAVLinkAPI.UI
{
    public class InputWithHistory : MonoBehaviour
    {
        [Autofill(AutofillType.SelfAndChildren)] public TMP_InputField input;

        [Autofill(AutofillType.SelfAndChildren)] public TMP_Dropdown dropdown;

        [NonSerialized] public List<string> History = new();

        public string? persistenceID;

        public int maxHistorySize = 30;
        
        // Wrapper class for JsonUtility to serialize/deserialize List<string>
        [System.Serializable]
        private class HistoryWrapper
        {
            public List<string> items;
            public HistoryWrapper(List<string> items) => this.items = items;
        }

        void Awake()
        {
            // LoadHistory() is called in Start() to ensure persistenceID is set.
        }

        void Start()
        {
            LoadHistory();
            
            if (input != null)
            {
                input.onEndEdit.AddListener(OnInputSubmit);
            }

            if (dropdown != null)
            {
                dropdown.onValueChanged.AddListener(OnDropdownSelect);
                RefreshHistory(); // Initial population after loading history
            }
        }

        void OnInputSubmit(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                if (!History.Contains(text))
                {
                    History.Insert(0, text); // Add to the beginning for most recent
                    // Optional: Limit history size
                    if (History.Count > maxHistorySize) History.RemoveAt(History.Count - 1);
                    SaveHistory();
                    RefreshHistory();
                }
                input.text = ""; 
            }
        }

        void OnDropdownSelect(int index)
        {
            if (dropdown != null && History.Count > index && index >= 0)
            {
                input.text = History[index];
            }
        }

        public void RefreshHistory()
        {
            if (dropdown == null) return;

            dropdown.ClearOptions();
            if (History.Count > 0)
            {
                dropdown.AddOptions(History);
            }
            dropdown.RefreshShownValue();
        }

        private void LoadHistory()
        {
            if (string.IsNullOrEmpty(persistenceID))
            {
                History = new List<string>();
                return;
            }
            
            string json = PlayerPrefs.GetString(persistenceID, null);
            if (!string.IsNullOrEmpty(json))
            {
                HistoryWrapper wrapper = JsonUtility.FromJson<HistoryWrapper>(json);
                if (wrapper != null && wrapper.items != null)
                {
                    History = wrapper.items;
                }
                else
                {
                    History = new List<string>(); // Fallback to empty list if deserialization fails
                }
            }
            else
            {
                History = new List<string>(); // Initialize if no saved history
            }
        }

        private void SaveHistory()
        {
            if (string.IsNullOrEmpty(persistenceID)) return;

            HistoryWrapper wrapper = new HistoryWrapper(History);
            string json = JsonUtility.ToJson(wrapper);
            PlayerPrefs.SetString(persistenceID, json);
            PlayerPrefs.Save(); // Ensure data is written to disk
        }

        void OnApplicationQuit()
        {
            SaveHistory();
        }
    }
}