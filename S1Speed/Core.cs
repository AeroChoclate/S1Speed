using MelonLoader;
using S1Speed.Utils;
using System.Reflection;
using UnityEngine;

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

        public override void OnInitializeMelon()
        {
            playerMovementType = Type.GetType("ScheduleOne.PlayerScripts.PlayerMovement, Assembly-CSharp");

            if (playerMovementType != null)
            {
                speedField = playerMovementType.GetField("StaticMoveSpeedMultiplier",
                    BindingFlags.Public | BindingFlags.Static);
            }

            LoggerInstance.Msg("Speed mod initialized safely.");
        }

        public override void OnUpdate()
        {
            HandleInput();
        }

        private void HandleInput()
        {
            if (Input.GetKeyDown(KeyCode.F6))
            {
                speedEnabled = !speedEnabled;

                ApplySpeed();

                LoggerInstance.Msg($"Speed: {(speedEnabled ? "ON" : "OFF")}");
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
    }
}