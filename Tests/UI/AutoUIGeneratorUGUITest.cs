using System;
using System.Linq;
using MAVLinkAPI.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace MAVLinkAPI.Tests.UI
{
    [Serializable]
    public record PlayerData(string Name, int Level, float Health);

    public class PlayerUIGen : AutoUIGeneratorUGUI<PlayerData>
    {
    }

    [TestFixture]
    public class AutoUIGeneratorUGUITests
    {
        private GameObject _canvasGo;
        private PlayerUIGen _generatorUGUI;

        [SetUp]
        public void SetUp()
        {
            _canvasGo = new GameObject("Canvas");
            var canvas = _canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var uiRoot = new GameObject("UIRoot").AddComponent<RectTransform>();
            uiRoot.SetParent(canvas.transform, false);

            _generatorUGUI = _canvasGo.AddComponent<PlayerUIGen>();
            _generatorUGUI.uiRoot = uiRoot;
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_canvasGo);
        }

        // [Test] // this will not pass,
        // see https://stackoverflow.com/questions/6280506/is-there-a-way-to-set-properties-on-struct-instances-using-reflection
        public void SetValueSpike()
        {
            // Arrange
            var playerData = new PlayerData("Test Player", 1, 100f);

            // Act
            var healthField = typeof(PlayerData).GetField("health");
            healthField.SetValue(playerData, 75.5f);

            // Assert
            Assert.AreEqual(75.5f, playerData.Health);
        }

        [Test]
        public void GenerateUIForStruct_CreatesCorrectElements()
        {
            _generatorUGUI.Value = new PlayerData("Test", 5, 100f);
            _generatorUGUI.Start();

            var uiElements = _generatorUGUI.uiRoot.GetComponentsInChildren<InputField>();
            Assert.AreEqual(3, uiElements.Length);

            var nameField = uiElements.First(f => f.gameObject.name == "name");
            var levelField = uiElements.First(f => f.gameObject.name == "level");
            var healthField = uiElements.First(f => f.gameObject.name == "health");

            Assert.IsNotNull(nameField);
            Assert.IsNotNull(levelField);
            Assert.IsNotNull(healthField);

            Assert.AreEqual("Test", nameField.text);
            Assert.AreEqual("5", levelField.text);
            Assert.AreEqual("100", healthField.text);
        }

        [Test]
        public void UIElements_UpdateStructWhenChanged()
        {
            var uiRoot = new GameObject("UIRoot").AddComponent<RectTransform>();
            var autoUIGenerator = uiRoot.gameObject.AddComponent<PlayerUIGen>();
            autoUIGenerator.Value = new PlayerData(string.Empty, 0, 0f);
            autoUIGenerator.uiRoot = uiRoot;

            autoUIGenerator.GenerateUIForStruct(uiRoot);

            var uiElements = uiRoot.GetComponentsInChildren<InputField>();
            var nameField = uiElements.First(f => f.gameObject.name == "name");
            var levelField = uiElements.First(f => f.gameObject.name == "level");
            var healthField = uiElements.First(f => f.gameObject.name == "health");

            nameField.text = "NewPlayer";
            nameField.onValueChanged.Invoke("NewPlayer");
            levelField.text = "10";
            levelField.onValueChanged.Invoke("10");
            healthField.text = "75.5";
            healthField.onValueChanged.Invoke("75.5");

            Assert.AreEqual("NewPlayer", autoUIGenerator.Value.Name);
            Assert.AreEqual(10, autoUIGenerator.Value.Level);
            Assert.AreEqual(75.5f, autoUIGenerator.Value.Health);

            Object.DestroyImmediate(uiRoot.gameObject);
        }
    }
}