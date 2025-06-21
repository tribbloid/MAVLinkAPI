using System.Collections;
using System.Collections.Generic;
using MAVLinkAPI.UI;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;

namespace MAVLinkAPI.Tests.UI
{
    public class InputWithHistorySpec
    {
        private GameObject _testHost;
        private InputWithHistory _historyDropDown;
        private TMP_InputField _inputField;
        private TMP_Dropdown _dropdown;

        private const string MAVLINK_HISTORY_KEY = "InputWithHistory_TestKey";

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            PlayerPrefs.DeleteKey(MAVLINK_HISTORY_KEY);
            PlayerPrefs.Save();

            _testHost = new GameObject("TestHost");

            GameObject inputFieldGo = new GameObject("TestInputField");
            inputFieldGo.transform.SetParent(_testHost.transform);
            _inputField = inputFieldGo.AddComponent<TMP_InputField>();
            GameObject inputViewportGo = new GameObject("Text Area");
            inputViewportGo.transform.SetParent(inputFieldGo.transform);
            _inputField.textViewport = inputViewportGo.AddComponent<RectTransform>();
            GameObject inputTextGo = new GameObject("Text");
            inputTextGo.transform.SetParent(inputViewportGo.transform);
            _inputField.textComponent = inputTextGo.AddComponent<TextMeshProUGUI>();

            GameObject dropdownGo = new GameObject("TestDropdown");
            dropdownGo.transform.SetParent(_testHost.transform);
            _dropdown = dropdownGo.AddComponent<TMP_Dropdown>();
            GameObject dropdownCaptionTextGo = new GameObject("Label");
            dropdownCaptionTextGo.transform.SetParent(dropdownGo.transform);
            _dropdown.captionText = dropdownCaptionTextGo.AddComponent<TextMeshProUGUI>();
            GameObject dropdownTemplateGo = new GameObject("Template");
            dropdownTemplateGo.transform.SetParent(dropdownGo.transform);
            dropdownTemplateGo.AddComponent<RectTransform>();
            _dropdown.template = dropdownTemplateGo.GetComponent<RectTransform>();
            GameObject dropdownItemGo = new GameObject("Item");
            dropdownItemGo.transform.SetParent(dropdownTemplateGo.transform);
            _dropdown.itemText = dropdownItemGo.AddComponent<TextMeshProUGUI>();
            dropdownTemplateGo.SetActive(false);

            _historyDropDown = _testHost.AddComponent<InputWithHistory>();
            _historyDropDown.input = _inputField;
            _historyDropDown.dropdown = _dropdown;
            _historyDropDown.persistenceID = MAVLINK_HISTORY_KEY; // Set key for persistence tests

            yield return null;
        }

        [TearDown]
        public void TearDown()
        {
            if (_testHost != null)
            {
                Object.DestroyImmediate(_testHost);
            }

            PlayerPrefs.DeleteKey(MAVLINK_HISTORY_KEY);
            PlayerPrefs.Save();
        }

        [UnityTest]
        public IEnumerator InputSubmission_AddsToHistoryAndDropdown_Test()
        {
            string testInput = "test entry 1";
            _inputField.text = testInput;
            _inputField.onSubmit.Invoke(_inputField.text);
            yield return null;

            Assert.AreEqual(1, _historyDropDown.History.Count, "History count mismatch.");
            Assert.AreEqual(testInput, _historyDropDown.History[0], "History content mismatch.");
            Assert.AreEqual(1, _dropdown.options.Count, "Dropdown options count mismatch.");
            Assert.AreEqual(testInput, _dropdown.options[0].text, "Dropdown option text mismatch.");
        }

        [UnityTest]
        public IEnumerator InputSubmission_ClearsInputField_Test()
        {
            string testInput = "this should be cleared";
            _inputField.text = testInput;
            _inputField.onSubmit.Invoke(testInput);
            yield return null;

            Assert.AreEqual(string.Empty, _inputField.text, "Input field should be cleared after submission.");
        }

        [UnityTest]
        public IEnumerator DropdownSelection_UpdatesInputField_Test()
        {
            string item1 = "select this";
            _historyDropDown.History.Add(item1);
            _historyDropDown.RefreshHistory();
            yield return null;

            Assert.AreEqual(1, _dropdown.options.Count, "Dropdown not populated correctly for test.");

            _dropdown.value = 0;
            _dropdown.onValueChanged.Invoke(0);
            yield return null;

            Assert.AreEqual(item1, _inputField.text, "InputField not updated after dropdown selection.");
        }

        [UnityTest]
        public IEnumerator HistoryPersistence_SavesAndLoads_Test()
        {
            string entryToPersist = "persistent entry";
            _inputField.onSubmit.Invoke(entryToPersist);
            yield return null;

            Assert.AreEqual(1, _historyDropDown.History.Count, "Initial history save failed.");

            Object.DestroyImmediate(_historyDropDown);

            var newHistoryDropDown = _testHost.AddComponent<InputWithHistory>();
            newHistoryDropDown.input = _inputField;
            newHistoryDropDown.dropdown = _dropdown;
            newHistoryDropDown.persistenceID = MAVLINK_HISTORY_KEY; // Ensure new instance also has the key

            yield return null; // Allow Awake/Start to run on the new component

            Assert.AreEqual(1, newHistoryDropDown.History.Count, "History not loaded correctly after restart.");
            Assert.AreEqual(entryToPersist, newHistoryDropDown.History[0], "Persistent entry content mismatch.");
            Assert.AreEqual(1, _dropdown.options.Count, "Dropdown not populated from loaded history.");
            Assert.AreEqual(entryToPersist, _dropdown.options[0].text, "Dropdown option text mismatch after load.");
        }

        [UnityTest]
        public IEnumerator DuplicateInput_IsNotAddedToHistory_Test()
        {
            string testInput = "duplicate test";
            _inputField.onSubmit.Invoke(testInput);
            yield return null;
            Assert.AreEqual(1, _historyDropDown.History.Count, "First entry not added.");

            _inputField.onSubmit.Invoke(testInput);
            yield return null;

            Assert.AreEqual(1, _historyDropDown.History.Count, "Duplicate entry was added to history.");
        }

        [UnityTest]
        public IEnumerator EmptyInput_IsNotAddedToHistory_Test()
        {
            _inputField.onSubmit.Invoke("");
            yield return null;

            Assert.AreEqual(0, _historyDropDown.History.Count, "Empty string was added to history.");
            Assert.AreEqual(0, _dropdown.options.Count, "Dropdown options not empty after empty input.");
        }

        [UnityTest]
        public IEnumerator Persistence_IsDisabled_WhenIdIsNull_Test()
        {
            // 1. Ensure persistence is disabled by setting ID to null
            _historyDropDown.persistenceID = null;

            // 2. Submit an entry
            string testInput = "this should not be saved";
            _inputField.onSubmit.Invoke(testInput);
            yield return null;

            Assert.AreEqual(1, _historyDropDown.History.Count,
                "History should still update in-memory for the current session.");

            // 3. Simulate restart
            Object.DestroyImmediate(_historyDropDown);
            var newHistoryDropDown = _testHost.AddComponent<InputWithHistory>();
            newHistoryDropDown.input = _inputField;
            newHistoryDropDown.dropdown = _dropdown;
            newHistoryDropDown.persistenceID = null; // Ensure new instance also has a null ID

            yield return null; // Allow Awake/Start

            // 4. Assert that history is empty because it wasn't loaded
            Assert.AreEqual(0, newHistoryDropDown.History.Count,
                "History should not be loaded when persistence ID is null.");
        }

        [UnityTest]
        public IEnumerator HistorySize_IsLimited_Test()
        {
            _historyDropDown.maxHistorySize = 5;

            for (int i = 0; i < _historyDropDown.maxHistorySize + 1; i++)
            {
                string entry = $"entry {i}";
                _inputField.onSubmit.Invoke(entry);
                yield return null;
            }

            Assert.AreEqual(_historyDropDown.maxHistorySize, _historyDropDown.History.Count,
                "History size should be limited to maxHistorySize.");
            Assert.AreEqual($"entry {_historyDropDown.maxHistorySize}", _historyDropDown.History[0],
                "The newest entry should be at the start.");
            Assert.AreEqual("entry 1", _historyDropDown.History[_historyDropDown.maxHistorySize - 1],
                "The oldest entry ('entry 0') should have been removed.");
        }

        [UnityTest]
        public IEnumerator ShouldMemorizeInputOnSubmit()
        {
            _inputField.text = "test input";
            _inputField.onSubmit.Invoke(_inputField.text);
            yield return null;

            Assert.AreEqual(1, _historyDropDown.History.Count);
            Assert.AreEqual("test input", _historyDropDown.History[0]);
        }

        [UnityTest]
        public IEnumerator ShouldNotMemorizeEmptyInput()
        {
            _inputField.text = "";
            _inputField.onSubmit.Invoke(_inputField.text);
            yield return null;

            Assert.AreEqual(0, _historyDropDown.History.Count);
        }

        [UnityTest]
        public IEnumerator ShouldUpdateHistoryOnNewInput()
        {
            _inputField.text = "first input";
            _inputField.onSubmit.Invoke(_inputField.text);
            yield return null;

            _inputField.text = "second input";
            _inputField.onSubmit.Invoke(_inputField.text);
            yield return null;

            Assert.AreEqual(2, _historyDropDown.History.Count);
        }

        [UnityTest]
        public IEnumerator ShouldMoveExistingInputToFront()
        {
            _historyDropDown.History = new List<string> { "first", "second", "third" };

            _inputField.text = "second";
            _inputField.onSubmit.Invoke(_inputField.text);
            yield return null;

            Assert.AreEqual(3, _historyDropDown.History.Count);
        }
    }
}