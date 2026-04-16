using MelonLoader;
using S1Speed.Utils;
using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

[assembly: MelonInfo(typeof(S1Speed.Core), Constants.MOD_NAME, Constants.MOD_VERSION, Constants.MOD_AUTHOR)]
[assembly: MelonGame(Constants.Game.GAME_STUDIO, Constants.Game.GAME_NAME)]

namespace S1Speed
{
    public class Core : MelonMod
    {
        private bool speedEnabled;

        private Type playerMovementType;
        private FieldInfo speedField;

        private const float SPEED = 5f;

        private MelonPreferences_Category configCategory;
        private MelonPreferences_Entry<string> keybindEntry;

        private KeyCode toggleKey = KeyCode.Insert;

        private GameObject menuObject;
        private bool menuVisible;

        private bool waitingForKey;

        private Text speedText;
        private Text keybindText;

        public override void OnInitializeMelon()
        {
            configCategory = MelonPreferences.CreateCategory("S1Speed");
            keybindEntry = configCategory.CreateEntry("ToggleKey", "Insert");

            if (Enum.TryParse(keybindEntry.Value, out KeyCode parsedKey))
            {
                toggleKey = parsedKey;
            }

            playerMovementType = Type.GetType("ScheduleOne.PlayerScripts.PlayerMovement, Assembly-CSharp");

            if (playerMovementType != null)
            {
                speedField = playerMovementType.GetField("StaticMoveSpeedMultiplier",
                    BindingFlags.Public | BindingFlags.Static);
            }

            LoggerInstance.Msg($"Initialized. Keybind: {toggleKey}");
        }

        public override void OnUpdate()
        {
            HandleMenuToggle();
            HandleKeybindChange();

            ApplySpeed();
        }

        private void HandleMenuToggle()
        {
            if (Input.GetKeyDown(toggleKey) && !waitingForKey)
            {
                menuVisible = !menuVisible;

                if (menuObject == null)
                    CreateMenu();

                menuObject.SetActive(menuVisible);
            }
        }

        private void ApplySpeed()
        {
            try
            {
                if (speedField == null) return;

                speedField.SetValue(null, speedEnabled ? SPEED : 1f);
            }
            catch (Exception ex)
            {
                LoggerInstance.Error("Speed error: " + ex.Message);
            }
        }

        private void HandleKeybindChange()
        {
            if (!waitingForKey) return;

            foreach (KeyCode key in Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKeyDown(key))
                {
                    toggleKey = key;
                    keybindEntry.Value = key.ToString();
                    keybindEntry.Save();

                    waitingForKey = false;

                    LoggerInstance.Msg($"New keybind: {toggleKey}");
                    UpdateButtonText();

                    break;
                }
            }
        }

        private void CreateMenu()
        {
            menuObject = new GameObject("S1SpeedMenu");

            var canvas = menuObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            menuObject.AddComponent<CanvasScaler>();
            menuObject.AddComponent<GraphicRaycaster>();

            GameObject panel = new GameObject("Panel");
            panel.transform.SetParent(menuObject.transform, false);

            var img = panel.AddComponent<Image>();
            img.color = new Color(0, 0, 0, 0.75f);

            RectTransform rect = panel.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(300, 200);
            rect.anchoredPosition = new Vector2(200, -150);

            GameObject speedBtn = CreateButton(panel, new Vector2(0, 40));
            speedText = CreateText(speedBtn, "Speed: OFF");

            speedBtn.GetComponent<Button>().onClick.AddListener(() =>
            {
                speedEnabled = !speedEnabled;
                UpdateButtonText();
            });

            GameObject keyBtn = CreateButton(panel, new Vector2(0, -40));
            keybindText = CreateText(keyBtn, $"Menu Key: {toggleKey}");

            keyBtn.GetComponent<Button>().onClick.AddListener(() =>
            {
                waitingForKey = true;
                keybindText.text = "Press any key...";
            });

            menuObject.SetActive(false);
        }

        private GameObject CreateButton(GameObject parent, Vector2 pos)
        {
            GameObject btnObj = new GameObject("Button");
            btnObj.transform.SetParent(parent.transform, false);

            var btn = btnObj.AddComponent<Button>();
            var img = btnObj.AddComponent<Image>();
            img.color = Color.green;

            RectTransform rt = btnObj.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(220, 40);
            rt.anchoredPosition = pos;

            return btnObj;
        }

        private Text CreateText(GameObject parent, string textValue)
        {
            GameObject txtObj = new GameObject("Text");
            txtObj.transform.SetParent(parent.transform, false);

            var text = txtObj.AddComponent<Text>();
            text.text = textValue;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.black;
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");

            RectTransform rt = text.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(220, 40);

            return text;
        }

        private void UpdateButtonText()
        {
            if (speedText != null)
                speedText.text = speedEnabled ? "Speed: ON" : "Speed: OFF";

            if (keybindText != null)
                keybindText.text = $"Key: {toggleKey}";
        }
    }
}