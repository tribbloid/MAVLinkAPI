using System.Collections;
using MAVLinkAPI.UI;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;

namespace MAVLinkAPI.Tests.UI
{
    public class InputWithHistorySpec
    {
        private GameObject testHost;
        private InputWithHistory historyDropDown;
        private TMP_InputField inputField;
        private TMP_Dropdown dropdown;

        private const string HistoryKey = "InputWithHistory_TestKey";

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            PlayerPrefs.DeleteKey(HistoryKey);
            PlayerPrefs.Save();

            testHost = new GameObject("TestHost");

            GameObject inputFieldGO = new GameObject("TestInputField");
            inputFieldGO.transform.SetParent(testHost.transform);
            inputField = inputFieldGO.AddComponent<TMP_InputField>();
            GameObject inputViewportGO = new GameObject("Text Area");
            inputViewportGO.transform.SetParent(inputFieldGO.transform);
            inputField.textViewport = inputViewportGO.AddComponent<RectTransform>();
            GameObject inputTextGO = new GameObject("Text");
            inputTextGO.transform.SetParent(inputViewportGO.transform);
            inputField.textComponent = inputTextGO.AddComponent<TextMeshProUGUI>();

            GameObject dropdownGO = new GameObject("TestDropdown");
            dropdownGO.transform.SetParent(testHost.transform);
            dropdown = dropdownGO.AddComponent<TMP_Dropdown>();
            GameObject dropdownCaptionTextGO = new GameObject("Label");
            dropdownCaptionTextGO.transform.SetParent(dropdownGO.transform);
            dropdown.captionText = dropdownCaptionTextGO.AddComponent<TextMeshProUGUI>();
            GameObject dropdownTemplateGO = new GameObject("Template");
            dropdownTemplateGO.transform.SetParent(dropdownGO.transform);
            dropdownTemplateGO.AddComponent<RectTransform>();
            dropdown.template = dropdownTemplateGO.GetComponent<RectTransform>();
            GameObject dropdownItemGO = new GameObject("Item");
            dropdownItemGO.transform.SetParent(dropdownTemplateGO.transform);
            dropdown.itemText = dropdownItemGO.AddComponent<TextMeshProUGUI>();
            dropdownTemplateGO.SetActive(false);

            historyDropDown = testHost.AddComponent<InputWithHistory>();
            historyDropDown.input = inputField;
            historyDropDown.dropdown = dropdown;
            historyDropDown.persistenceID = HistoryKey; // Set key for persistence tests

            yield return null;
        }

        [TearDown]
        public void TearDown()
        {
            if (testHost != null)
            {
                Object.DestroyImmediate(testHost);
            }
            PlayerPrefs.DeleteKey(HistoryKey);
            PlayerPrefs.Save();
        }

        [UnityTest]
        public IEnumerator InputSubmission_AddsToHistoryAndDropdown_Test()
        {
            string testInput = "test entry 1";
            inputField.text = testInput;
            inputField.onEndEdit.Invoke(testInput);
            yield return null;

            Assert.AreEqual(1, historyDropDown.History.Count, "History count mismatch.");
            Assert.AreEqual(testInput, historyDropDown.History[0], "History content mismatch.");
            Assert.AreEqual(1, dropdown.options.Count, "Dropdown options count mismatch.");
            Assert.AreEqual(testInput, dropdown.options[0].text, "Dropdown option text mismatch.");
        }

        [UnityTest]
        public IEnumerator InputSubmission_ClearsInputField_Test()
        {
            string testInput = "this should be cleared";
            inputField.text = testInput;
            inputField.onEndEdit.Invoke(testInput);
            yield return null;

            Assert.AreEqual(string.Empty, inputField.text, "Input field should be cleared after submission.");
        }

        [UnityTest]
        public IEnumerator DropdownSelection_UpdatesInputField_Test()
        {
            string item1 = "select this";
            historyDropDown.History.Add(item1);
            historyDropDown.RefreshHistory();
            yield return null;

            Assert.AreEqual(1, dropdown.options.Count, "Dropdown not populated correctly for test.");

            dropdown.value = 0;
            dropdown.onValueChanged.Invoke(0);
            yield return null;

            Assert.AreEqual(item1, inputField.text, "InputField not updated after dropdown selection.");
        }

        [UnityTest]
        public IEnumerator HistoryPersistence_SavesAndLoads_Test()
        {
            string entryToPersist = "persistent entry";
            inputField.onEndEdit.Invoke(entryToPersist);
            yield return null;

            Assert.AreEqual(1, historyDropDown.History.Count, "Initial history save failed.");

            Object.DestroyImmediate(historyDropDown);

            var newHistoryDropDown = testHost.AddComponent<InputWithHistory>();
            newHistoryDropDown.input = inputField;
            newHistoryDropDown.dropdown = dropdown;
            newHistoryDropDown.persistenceID = HistoryKey; // Ensure new instance also has the key

            yield return null; // Allow Awake/Start to run on the new component

            Assert.AreEqual(1, newHistoryDropDown.History.Count, "History not loaded correctly after restart.");
            Assert.AreEqual(entryToPersist, newHistoryDropDown.History[0], "Persistent entry content mismatch.");
            Assert.AreEqual(1, dropdown.options.Count, "Dropdown not populated from loaded history.");
            Assert.AreEqual(entryToPersist, dropdown.options[0].text, "Dropdown option text mismatch after load.");
        }

        [UnityTest]
        public IEnumerator DuplicateInput_IsNotAddedToHistory_Test()
        {
            string testInput = "duplicate test";
            inputField.onEndEdit.Invoke(testInput);
            yield return null;
            Assert.AreEqual(1, historyDropDown.History.Count, "First entry not added.");

            inputField.onEndEdit.Invoke(testInput);
            yield return null;

            Assert.AreEqual(1, historyDropDown.History.Count, "Duplicate entry was added to history.");
        }

        [UnityTest]
        public IEnumerator EmptyInput_IsNotAddedToHistory_Test()
        {
            inputField.onEndEdit.Invoke("");
            yield return null;

            Assert.AreEqual(0, historyDropDown.History.Count, "Empty string was added to history.");
            Assert.AreEqual(0, dropdown.options.Count, "Dropdown options not empty after empty input.");
        }

        [UnityTest]
        public IEnumerator Persistence_IsDisabled_WhenIdIsNull_Test()
        {
            // 1. Ensure persistence is disabled by setting ID to null
            historyDropDown.persistenceID = null;

            // 2. Submit an entry
            string testInput = "this should not be saved";
            inputField.onEndEdit.Invoke(testInput);
            yield return null;

            Assert.AreEqual(1, historyDropDown.History.Count, "History should still update in-memory for the current session.");

            // 3. Simulate restart
            Object.DestroyImmediate(historyDropDown);
            var newHistoryDropDown = testHost.AddComponent<InputWithHistory>();
            newHistoryDropDown.input = inputField;
            newHistoryDropDown.dropdown = dropdown;
            newHistoryDropDown.persistenceID = null; // Ensure new instance also has a null ID

            yield return null; // Allow Awake/Start

            // 4. Assert that history is empty because it wasn't loaded
            Assert.AreEqual(0, newHistoryDropDown.History.Count, "History should not be loaded when persistence ID is null.");
        }

        [UnityTest]
        public IEnumerator HistorySize_IsLimited_Test()
        {
            historyDropDown.maxHistorySize = 5; 

            for (int i = 0; i < historyDropDown.maxHistorySize + 1; i++)
            {
                string entry = $"entry {i}";
                inputField.onEndEdit.Invoke(entry);
                yield return null;
            }

            Assert.AreEqual(historyDropDown.maxHistorySize, historyDropDown.History.Count, "History size should be limited to maxHistorySize.");
            Assert.AreEqual($"entry {historyDropDown.maxHistorySize}", historyDropDown.History[0], "The newest entry should be at the start.");
            Assert.AreEqual("entry 1", historyDropDown.History[historyDropDown.maxHistorySize - 1], "The oldest entry ('entry 0') should have been removed.");
        }
    }
}
