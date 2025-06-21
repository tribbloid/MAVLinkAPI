#nullable enable
using System;
using System.Collections.Generic;
using Autofill;
using MAVLinkAPI.Util.NullSafety;
using UnityEngine;
using TMPro;
using UnityEditor.Search;
using UnityEngine.Assertions;

namespace MAVLinkAPI.UI
{
    public class InputWithHistory : MonoBehaviour
    {
        [Autofill(AutofillType.SelfAndChildren)]
        public TMP_InputField input = null!;

        [Autofill(AutofillType.SelfAndChildren)]
        public TMP_Dropdown dropdown = null!;

        public bool isPersisted = true;
        public string? persistedIDOvrd;

        public string PersistedID
        {
            get
            {
                var v = persistedIDOvrd;
                if (String.IsNullOrWhiteSpace(v))
                    v = SearchUtils.GetHierarchyPath(gameObject, includeScene: true);
                Assert.IsFalse(String.IsNullOrEmpty(v), "persistentID cannot be null or empty");
                return v!;
            }
        }

        public int maxHistorySize = 30;

        private Maybe<List<string>> _history;

        public List<string> History => _history.Lazy(() =>
        {
            if (!isPersisted)
            {
                return new List<string>();
            }

            // load from PlayerPrefs ONCE
            string json = PlayerPrefs.GetString(PersistedID, null);
            if (!string.IsNullOrEmpty(json))
            {
                HistoryWrapper wrapper = JsonUtility.FromJson<HistoryWrapper>(json);
                if (wrapper != null)
                {
                    return wrapper.items;
                }

                // return new List<string>(); // Fallback to empty list if deserialization fails
            }

            return new List<string>(); // Initialize if no saved history
        });

        // Wrapper class for JsonUtility to serialize/deserialize List<string>
        [Serializable]
        private class HistoryWrapper
        {
            public List<string> items;
            public HistoryWrapper(List<string> items) => this.items = items;
        }


        // void Awake()
        // {
        //     // LoadHistory() is called in Start() to ensure persistenceID is set.
        // }

        void Start()
        {
            input.onSubmit.AddListener(OnInputSubmit);

            dropdown.onValueChanged.AddListener(OnDropdownSelect);
        }

        private void OnEnable()
        {
            RefreshHistory();
        }

        void OnInputSubmit(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                History.Remove(text);

                History.Insert(0, text); // Add to the beginning for most recent
                // Optional: Limit history size
                if (History.Count > maxHistorySize)
                    History.RemoveAt(
                        History.Count - 1);
                SaveHistory();
                RefreshHistory();
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


        private void SaveHistory()
        {
            if (!isPersisted) return;

            HistoryWrapper wrapper = new HistoryWrapper(History);
            string json = JsonUtility.ToJson(wrapper);
            PlayerPrefs.SetString(PersistedID, json);
            PlayerPrefs.Save(); // Ensure data is written to disk
        }

        void OnApplicationQuit()
        {
            SaveHistory();
        }
    }
}