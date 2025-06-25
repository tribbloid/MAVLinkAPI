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
        public TMP_Dropdown dropdown = null!;

        [Autofill(AutofillType.SelfAndChildren)]
        public TMP_InputField input = null!;

        public bool isPersisted = true;
        public string? persistedIDOvrd;

        public string PersistedID
        {
            get
            {
                var v = persistedIDOvrd;
                if (string.IsNullOrWhiteSpace(v))
                    v = SearchUtils.GetHierarchyPath(gameObject, true);
                Assert.IsFalse(string.IsNullOrEmpty(v), "persistentID cannot be null or empty");
                return v!;
            }
        }

        public int maxHistorySize = 30;

        private Maybe<List<string>> _history;

        public List<string> History => _history.Lazy(() =>
        {
            if (!isPersisted) return new List<string>();

            // load from PlayerPrefs ONCE
            var json = PlayerPrefs.GetString(PersistedID, null);
            if (!string.IsNullOrEmpty(json))
            {
                var wrapper = JsonUtility.FromJson<HistoryWrapper>(json);
                if (wrapper != null) return wrapper.items;

                // return new List<string>(); // Fallback to empty list if deserialization fails
            }

            return new List<string>(); // Initialize if no saved history
        });

        // Wrapper class for JsonUtility to serialize/deserialize List<string>
        [Serializable]
        private class HistoryWrapper
        {
            public List<string> items;

            public HistoryWrapper(List<string> items)
            {
                this.items = items;
            }
        }

        private void Start()
        {
            input.onSubmit.AddListener(OnInputSubmit);

            input.onEndEdit.AddListener(OnInputChanged); // TODO: need to select an ad-hoc option to sync dropdown

            dropdown.onValueChanged.AddListener(OnDropdownSelect);

            RefreshHistory();
            
            if (dropdown.options.Count > 0) input.text = dropdown.options[dropdown.value].text;

            // OnInputChanged(input.text);
        }

        // private void OnEnable()
        // {
        // }

        private void OnInputSubmit(string text)
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

        private void OnInputChanged(string text)
        {
            dropdown.AddOptions(new List<string> { text });
            dropdown.SetValueWithoutNotify(dropdown.options.Count - 1);
            // Any Edit will cause dropdown selectin to reset
        }

        private void OnDropdownSelect(int index)
        {
            if (dropdown != null && History.Count > index && index >= 0) input.text = History[index];
        }

        public void RefreshHistory()
        {
            if (dropdown == null) return;

            dropdown.ClearOptions();
            if (History.Count > 0) dropdown.AddOptions(History);

            dropdown.RefreshShownValue();
        }


        private void SaveHistory()
        {
            if (!isPersisted) return;

            var wrapper = new HistoryWrapper(History);
            var json = JsonUtility.ToJson(wrapper);
            PlayerPrefs.SetString(PersistedID, json);
            PlayerPrefs.Save(); // Ensure data is written to disk
        }

        private void OnApplicationQuit()
        {
            SaveHistory();
        }
    }
}