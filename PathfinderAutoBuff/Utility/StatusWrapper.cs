using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingmaker;
using Kingmaker.GameModes;
using Kingmaker.UI.Common;
using static PathfinderAutoBuff.Main;

namespace PathfinderAutoBuff.Utility
{
    public static class StatusWrapper
    {
        //Mod enabled wrapepr
        public static bool IsEnabled()
        {
            return Main.Enabled;
        }

        //UIEnabled Settings wrapper
        public static bool UIEnabled()
        {
            return Main.Settings.uIEnabled;
        }

        //Check if the mode is valid to show UI
        public static bool IsValidMode(GameModeType mode)
        {
            if (mode == GameModeType.Default || mode == GameModeType.Pause)
                return true;
            return false;
        }

        //Check if Game HUD is shown (for cutscenes, for example)
        public static bool IsHUDShown()
        {
            return Game.Instance.UI.Canvas?.HUDController.CurrentState == UISectionHUDController.HUDState.AllVisible;
        }

    }
}
